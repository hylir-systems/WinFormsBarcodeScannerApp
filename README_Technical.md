# WinFormsBarcodeScannerApp 技术交底文档

## 目录
1. [项目架构概览](#项目架构概览)
2. [核心回调机制](#核心回调机制)
3. [时序图详解](#时序图详解)
4. [关键文件说明](#关键文件说明)
5. [UI 线程同步](#ui-线程同步)

---

## 项目架构概览

```
┌─────────────────────────────────────────────────────────────────┐
│                      WinFormsBarcodeScannerApp                    │
├─────────────────────────────────────────────────────────────────┤
│  窗体层 (Root)                                                   │
│  ├── MainForm.cs          ← 主窗体，业务逻辑协调                   │
│  ├── ImageDetailForm.cs   ← 图片详情查看                          │
│  └── SettingsForm.cs     ← 设置窗体                              │
├─────────────────────────────────────────────────────────────────┤
│  控制器层 (Controllers/)                                          │
│  ├── HistoryManager.cs    ← 历史记录管理，ListView 绑定           │
│  ├── CameraController.cs  ← 摄像头控制                            │
│  ├── LayoutManager.cs    ← 窗体布局管理                           │
│  └── PreviewManager.cs   ← 预览画面管理                           │
├─────────────────────────────────────────────────────────────────┤
│  服务层 (Services/)                                               │
│  ├── UploadService.cs     ← 回单上传服务                          │
│  ├── LogService.cs       ← 日志服务                              │
│  ├── SettingsService.cs  ← 设置服务                              │
│  ├── BarcodeService.cs   ← 条码识别服务                          │
│  ├── CameraService.cs    ← 摄像头服务                             │
│  └── autocapture/        ← 自动采集模块                           │
│       ├── AutoCaptureService.cs  ← 自动采集控制                   │
│       ├── CapturePipeline.cs     ← 图像处理管道                   │
│       └── BarcodeDeduplicator.cs ← 条码去重                       │
├─────────────────────────────────────────────────────────────────┤
│  工具层 (utils/)                                                  │
│  └── AudioPlayer.cs      ← 音频播放器 (NAudio)                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## 核心回调机制

### 1. 上传服务回调链

上传流程采用 **回调链** 模式，将业务逻辑与网络操作解耦：

```
┌──────────────┐     ┌─────────────────┐     ┌──────────────┐
│   MainForm   │────▶│  UploadService  │────▶│   后端API    │
└──────────────┘     └─────────────────┘     └──────────────┘
       ▲                    │
       │                    │
       │    ┌───────────────┘
       │    │
       │    ▼
       │  ┌─────────────────────┐
       │  │  onUploadComplete   │───▶ 播放提示音
       │  └─────────────────────┘
       │
       │  ┌─────────────────────────────────────┐
       │  │ safeSuccessCallback / safeErrorCallback │
       │  │ (BeginInvoke 封装，确保 UI 线程执行)    │
       │  └─────────────────────────────────────┘
```

### 2. 回调类型说明

| 回调类型 | 签名 | 触发时机 | UI线程 |
|---------|------|---------|--------|
| `onSuccessUrl` | `Action<string>` | Step1 + Step2 都成功 | ✅ 安全 (BeginInvoke) |
| `onError` | `Action<string>` | 任何步骤失败 | ✅ 安全 (BeginInvoke) |
| `onUploadComplete` | `Action?` | 仅 Step1 + Step2 成功完成后 | ❌ 直接调用 (独立播放) |

### 3. 回调设计原则

#### 原则 1：UI 操作必须回到主线程

```csharp
// ❌ 错误：直接在后台线程操作 UI
UploadService.StartUploadAsync(..., (url) => {
    _txtResult.Text = url; // 可能跨线程访问异常
});

// ✅ 正确：使用 BeginInvoke 回到主线程
Action<string> safeSuccessCallback = url =>
{
    if (InvokeRequired)
        BeginInvoke((Action)(() => OnUploadSuccess(url)));
    else
        OnUploadSuccess(url);
};
```

#### 原则 2：独立操作使用独立回调

`onUploadComplete` 用于播放提示音，不需要回到主线程（NAudio 后台线程播放），
因此直接传入方法引用即可。

---

## 时序图详解

### 场景一：自动识别 → 上传成功 → 播放提示音

```
┌──────┐  ┌───────────┐  ┌──────────────┐  ┌──────────┐  ┌───────┐  ┌─────┐
│User  │  │MainForm   │  │CapturePipeline││AutoCapture││Camera │  │Server│
└──┬───┘  └─────┬─────┘  └──────┬───────┘  └────┬─────┘  └───┬────┘  └──┬──┘
   │            │               │                │            │         │
   │ 1.扫描     │               │                │            │         │
   │───────────▶│               │                │            │         │
   │            │ 2.ProcessFrame│                │            │         │
   │            │──────────────▶│                │            │         │
   │            │               │ 3.条码识别     │            │         │
   │            │               │───────────────▶│            │         │
   │            │               │                │ 4.IsDuplicate         │
   │            │               │                │◀────────────│         │
   │            │               │                │  false      │         │
   │            │               │◀───────────────│─────────────│         │
   │            │               │  CaptureResult │            │         │
   │            │◀──────────────│────────────────│─────────────│         │
   │            │ 5.识别成功     │                │            │         │
   │            │  AddRecord     │                │            │         │
   │            │────────────────────────────────────────────────────────│
   │            │               │                │            │         │
   │            │ 6.TryUploadReceiptAsync        │            │         │
   │            │────────────────────────────────────────────────────────│
   │            │               │                │            │         │
   │            │ 7.StartUploadAsync (onSuccess, onError, onComplete)   │
   │            │────────────────────────────────────────────────────────│
   │            │               │                │            │         │
   │            │   Task.Run(async () {          │            │         │
   │            │     await UploadAsync(...)     │            │         │
   │            │                           8.POST /upload  │─────────▶
   │            │                           (multipart)     │         │
   │            │                           ◀───────────────│         │
   │            │                           fileUrl         │         │
   │            │               │                │            │         │
   │            │                           9.POST /receipt │─────────▶
   │            │                           (sheetNo, url)  │         │
   │            │                           ◀───────────────│         │
   │            │                           success         │         │
   │            │     10.onSuccessUrl(fileUrl)              │         │
   │            │◀──────────────────────────────────────────────────────│
   │            │ 11.safeCallback → BeginInvoke               │         │
   │            │◀──────────────────────────────────────────────────────│
   │            │ 12.AppendUiLog("上传成功")                  │         │
   │            │────────────────────────────────────────────────────────│
   │            │     13.onUploadComplete → PlaySuccessSound()         │
   │            │────────────────────────────────────────────────────────│
   │            │     14.AudioPlayer.Play() (后台线程)      │         │
   │            │────────────────────────────────────────────────────────│
   │            │ 15.播放 success.mp3                          │         │
   │            │────────────────────────────────────────────────────────│
```

### 场景二：上传失败 → 移除记录 → 清除去重

```
┌──────┐  ┌─────────────┐  ┌───────────────┐  ┌─────────────────┐
│Server│  │UploadService│  │  MainForm     │  │HistoryManager   │
└──┬───┘  └──────┬──────┘  └───────┬───────┘  └────────┬────────┘
   │             │                 │                    │
   │ POST /upload│                 │                    │
   │◀────────────│                 │                    │
   │  error      │                 │                    │
   │             │                 │                    │
   │ 1.onError("...")             │                    │
   │◀──────────────────────────────────────────────────────────────│
   │             │                 │                    │
   │             │ 2.safeErrorCallback → BeginInvoke              │
   │             │──────────────────────────────────────────────────│
   │             │                 │ 3.OnUploadError               │
   │             │                 │◀─────────────────────────────│
   │             │                 │ 4._historyManager.RemoveRecord│
   │             │                 │─────────────────────────────▶│
   │             │                 │              5.OnRecordDeleted │
   │             │                 │◀─────────────────────────────│
   │             │                 │ 6._capturePipeline           │
   │             │                 │   .RemoveDuplicateBarcode    │
   │             │                 │─────────────────────────────▶│
   │             │                 │              7._deduplicator│
   │             │                 │                 .Remove     │
   │             │                 │◀─────────────────────────────│
   │             │                 │ 8.AppendUiLog("移除记录")    │
   │             │                 │◀─────────────────────────────│
```

---

## 关键文件说明

### 1. UploadService.cs

**职责**：封装两步上传逻辑

```csharp
public static void StartUploadAsync(
    string backendUrl,           // 后端地址
    string sheetNo,              // 单号
    string filePath,             // 本地图片路径
    Action<string> onSuccessUrl, // 成功回调（返回文件URL）
    Action<string> onError,      // 错误回调（返回错误信息）
    Action? onUploadComplete = null) // 完成回调（播放提示音）
```

**内部流程**：
1. `Task.Run()` 后台线程执行
2. `UploadAsync()` 执行两步上传
3. 成功 → `onSuccessUrl` + `onUploadComplete`
4. 失败 → `onError`

### 2. MainForm.cs

**职责**：业务协调，UI 回调处理

```csharp
private void TryUploadReceiptAsync(ScanRecord record)
{
    // 1. 定义业务回调
    void OnUploadSuccess(string fileUrl) => AppendUiLog("成功: " + fileUrl);
    void OnUploadError(string error) => {
        AppendUiLog("失败: " + error);
        _historyManager.RemoveRecord(record.Id);  // 失败移除记录
    };

    // 2. 封装 UI 线程安全回调
    Action<string> safeSuccessCallback = ...;
    Action<string> safeErrorCallback = ...;

    // 3. 调用上传服务
    UploadService.StartUploadAsync(...,
        safeSuccessCallback,
        safeErrorCallback,
        PlaySuccessSound);  // 完成后播放
}
```

### 3. HistoryManager.cs

**职责**：历史记录数据管理

**事件机制**：

```csharp
// 选中记录
public event Action<string>? OnRecordSelected;

// 删除记录（传递条码号，用于清除去重）
public event Action<string>? OnRecordDeleted;

// 内部删除逻辑
public void RemoveRecord(string id)
{
    var record = _records.FirstOrDefault(r => r.Id == id);
    if (record != null)
    {
        _records.RemoveAll(r => r.Id == id);
        OnRecordDeleted?.Invoke(record.OrderNo);  // 传递条码号
        Render();
    }
}
```

### 4. AudioPlayer.cs

**职责**：音频播放（NAudio）

```csharp
public void Play(string filePath)
{
    // 后台线程播放，不阻塞主线程
    Thread playbackThread = new Thread(() =>
    {
        using (var waveOut = new WaveOutEvent())
        using (var audioFile = new AudioFileReader(filePath))
        {
            waveOut.Init(audioFile);
            waveOut.Play();
            while (waveOut.PlaybackState == PlaybackState.Playing)
                Thread.Sleep(100);
        }
    });
    playbackThread.IsBackground = true;
    playbackThread.Start();
}
```

**设计要点**：
- 后台线程播放，避免 `using` 块释放导致中断
- 支持 MP3 格式（`SoundPlayer` 仅支持 WAV）

---

## UI 线程同步

### 问题背景

WinForms 控件不是线程安全的，跨线程访问会抛出 `InvalidOperationException`。

### 解决方案

#### 1. 使用 `InvokeRequired` + `BeginInvoke`

```csharp
private void SafeInvoke(Action action)
{
    if (InvokeRequired)
        BeginInvoke(action);
    else
        action();
}
```

#### 2. 回调链封装模式

```csharp
// MainForm 中
Action<string> safeCallback = msg =>
{
    if (InvokeRequired)
        BeginInvoke((Action)(() => SomeMethod(msg)));
    else
        SomeMethod(msg);
};

// 传递给服务层
UploadService.StartUploadAsync(..., safeCallback, ...);
```

### 线程安全调用位置

| 场景 | 是否需要 BeginInvoke |
|------|---------------------|
| `AppendUiLog()` | ✅ 需要 |
| `_historyManager.RemoveRecord()` | ✅ 需要 |
| `PlaySuccessSound()` | ❌ 不需要（独立后台播放） |

---

## 常见问题排查

### Q1: 上传后提示音不播放

**检查**：
1. `success.mp3` 是否在 `asserts/` 目录
2. NAudio 是否正确安装
3. 后台线程是否正常启动

### Q2: 跨线程访问异常

**症状**：`InvalidOperationException: 线程间操作无效`

**解决**：确保所有 UI 操作使用 `BeginInvoke`

### Q3: 删除记录后仍报重复识别

**原因**：`OnRecordDeleted` 未正确传递条码号

**解决**：确认 `HistoryManager` 传递的是 `record.OrderNo` 而非 `record.Id`

---

## 依赖版本

| 组件 | 版本 | 用途 |
|------|------|------|
| .NET | 10.0-windows | 运行时 |
| NAudio | 2.2.1 | 音频播放 |
| Emgu.CV | 最新 | 图像处理/条码识别 |
| Newtonsoft.Json | 最新 | JSON 序列化 |

