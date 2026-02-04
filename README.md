# WinFormsBarcodeScannerApp

一个基于 **C# WinForms** 的条码扫描应用程序。

## 功能特性

- 摄像头实时预览与拍照
- 条码识别（A1/A4 纸张模式）
- 扫描历史记录管理
- 扫描成功提示音
- 灵活的参数配置

## 技术栈

| 类别 | 技术/库 |
|------|---------|
| 框架 | .NET 10.0-windows |
| UI | WinForms (System.Windows.Forms) |
| 摄像头 | AForge.Video + AForge.Video.DirectShow |
| 条码识别 | Aspose.BarCode.dll（本地引用） |
| 图像处理 | Emgu.CV + System.Drawing.Bitmap |
| 音频 | NAudio |

## 项目结构

```
WinFormsBarcodeScannerApp/
├── WinFormsBarcodeScannerApp/
│   ├── asserts/              # 资源文件
│   │   ├── Aspose.Total.NET.lic
│   │   └── success.mp3
│   ├── Controllers/          # 控制器层
│   ├── Services/            # 业务逻辑服务
│   ├── utils/               # 工具类
│   ├── MainForm.cs          # 主窗口
│   ├── SettingsForm.cs      # 设置窗口
│   └── WinFormsBarcodeScannerApp.csproj
├── WinFormsBarcodeScannerApp.sln
├── .gitignore
└── README.md
```

## 环境要求

- .NET SDK: 10.0 或更高版本
- Visual Studio: 2022 或更高版本
- 操作系统: Windows 10/11

## 快速开始

### 1. 克隆项目

```bash
git clone https://github.com/hylir-systems/WinFormsBarcodeScannerApp.git
cd WinFormsBarcodeScannerApp
```

### 2. 还原依赖

项目使用 NuGet 管理大部分依赖，Aspose.BarCode 使用本地引用。

**方法一：Visual Studio（推荐）**

1. 用 Visual Studio 打开 `WinFormsBarcodeScannerApp.sln`
2. NuGet 包会自动还原

**方法二：命令行**

```powershell
cd D:\hylir\front-end\WinFormsBarcodeScannerApp
dotnet restore
```

### 3. 配置 Aspose 许可证

Aspose.BarCode 需要有效的许可证才能正常工作。

许可证文件已包含在项目中：`WinFormsBarcodeScannerApp/asserts/Aspose.Total.NET.lic`

如需使用自己的许可证，请替换该文件或联系 Aspose 购买。

### 4. 运行项目

**Visual Studio**: 按 F5 启动

**命令行**:

```powershell
dotnet run --configuration Release
```

## 配置说明

首次运行会在以下位置生成配置文件：

- 配置文件: `%APPDATA%\WinFormsBarcodeScannerApp\config.json`
- 扫描图片: `%APPDATA%\WinFormsBarcodeScannerApp\A4\`
- 日志文件: `%APPDATA%\WinFormsBarcodeScannerApp\logs\app.log`

## 许可证

本项目使用 **Aspose.BarCode** 进行条码识别，需要有效的许可证。

- 项目默认包含评估版许可证
- 如需正式使用，请购买完整许可证：https://purchase.aspose.com/

## 贡献指南

欢迎提交 Issue 和 Pull Request！

## 协议

本项目仅供学习和研究使用。
