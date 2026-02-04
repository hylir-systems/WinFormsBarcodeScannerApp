using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WinFormsBarcodeScannerApp.Services;

namespace WinFormsBarcodeScannerApp.Controllers
{
    /// <summary>
    /// 历史记录项
    /// </summary>
    public sealed class ScanRecord
    {
        public int Id;
        public string OrderNo;
        public DateTime Time;
        /// <summary>图片路径</summary>
        public string imagePath;
        /// <summary>图片URL</summary>
        public string imageUrl;
    }

    /// <summary>
    /// 历史记录管理器：负责扫描历史的管理与展示
    /// 关注点：历史数据、ListView 展示
    /// </summary>
    public sealed class HistoryManager : IDisposable
    {
        private readonly List<ScanRecord> _records = new List<ScanRecord>();
        private ListView _historyListView;
        private ImageList _historyImageList;
        private int _nextRecordId = 1;
        private const int MaxHistoryCount = 200;

        public event Action<int> OnRecordSelected;
        /// <summary>
        /// 删除记录时触发，参数为被删除记录的条码
        /// </summary>
        public event Action<string> OnRecordDeleted;

        public HistoryManager()
        {
        }

        /// <summary>
        /// 绑定 ListView 控件
        /// </summary>
        public void Bind(ListView historyListView, ImageList imageList)
        {
            _historyListView = historyListView;
            _historyImageList = imageList;
            //ItemActivate - 双击项目 / 按 Enter
            _historyListView.ItemActivate += (s, e) =>
            {
                if (_historyListView.SelectedItems.Count > 0)
                {
                    var record = _historyListView.SelectedItems[0].Tag as ScanRecord;
                    OpenDetail(record);
                }
            };

            // 按 Delete 键删除选中记录
            _historyListView.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Delete && _historyListView.SelectedItems.Count > 0)
                {
                    var record = _historyListView.SelectedItems[0].Tag as ScanRecord;
                    if (MessageBox.Show($"确认删除记录 [{record.OrderNo}]？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        OnRecordDeleted?.Invoke(record.OrderNo);
                        _records.RemoveAll(r => r.Id == record.Id);
                        Render();
                    }
                    e.SuppressKeyPress = true;
                }
            };
        }

        /// <summary>
        /// 初始化大图标视图
        /// </summary>
        public void InitializeListView()
        {
            _historyImageList.ColorDepth = ColorDepth.Depth32Bit;
            _historyImageList.ImageSize = new Size(64, 48);

            _historyListView.LargeImageList = _historyImageList;
            _historyListView.View = View.LargeIcon;
            _historyListView.AutoArrange = true;
            _historyListView.UseCompatibleStateImageBehavior = false;
            _historyListView.MultiSelect = false;
            _historyListView.HideSelection = false;
            _historyListView.BorderStyle = BorderStyle.FixedSingle;
            _historyListView.ShowItemToolTips = true;
        }

        /// <summary>
        /// 添加历史记录
        /// </summary>
        /// <param name="barcodeText">条码文本</param>
        /// <param name="imagePath">图片路径</param>
        public ScanRecord AddRecord(string barcodeText, string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return null;

            // 如果已有相同条码且时间间隔小于5分钟，则跳过（防重复）
            var recentRecord = _records.FirstOrDefault(r =>
                r.OrderNo == barcodeText &&
                (DateTime.Now - r.Time).TotalMinutes < 5);
            if (recentRecord != null)
            {
                LogService.Info($"[历史] 条码 {barcodeText} 在5分钟内已存在，跳过");
                return null;
            }

            var record = new ScanRecord
            {
                Id = _nextRecordId++,
                OrderNo = barcodeText,
                Time = DateTime.Now,
                imagePath = imagePath
            };

            // 超过最大数量时删除最早的记录
            if (_records.Count >= MaxHistoryCount && _records.Count > 0)
            {
                _records.RemoveAt(_records.Count - 1);
            }

            _records.Insert(0, record);
            Render();

            return record;
        }

        /// <summary>
        /// 更新记录的 URL
        /// </summary>
        public void UpdateRecordUrl(int recordId, string url)
        {
            var record = _records.FirstOrDefault(r => r.Id == recordId);
            if (record != null)
            {
                record.imageUrl = url;
            }
        }

        /// <summary>
        /// 清空所有历史记录
        /// </summary>
        public void Clear()
        {
            _records.Clear();
            Render();
        }

        /// <summary>
        /// 获取记录数量
        /// </summary>
        public int Count => _records.Count;

        /// <summary>
        /// 渲染历史列表
        /// </summary>
        public void Render([System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            if (_historyListView == null || _historyImageList == null)
                return;

            if (_historyListView.InvokeRequired)
            {
                _historyListView.BeginInvoke(new Action(() => Render(caller)));
                return;
            }

            _historyListView.BeginUpdate();
            try
            {
                _historyListView.Items.Clear();
                _historyImageList.Images.Clear();

                foreach (var r in _records)
                {
                    if (string.IsNullOrWhiteSpace(r.imagePath))
                        continue;

                    Image img = null;
                    try
                    {
                        if (File.Exists(r.imagePath))
                        {
                            using (var tmp = Image.FromFile(r.imagePath))
                                img = new Bitmap(tmp);
                        }
                    }
                    catch { }

                    if (img == null)
                        continue;

                    // 缩放图片到 ImageList 的尺寸
                    var scaledImg = new Bitmap(img, _historyImageList.ImageSize);
                    img.Dispose();

                    _historyImageList.Images.Add(scaledImg);
                    var item = new ListViewItem
                    {
                        Text = r.OrderNo ?? string.Empty,
                        ImageIndex = _historyImageList.Images.Count - 1,
                        Tag = r,
                        ToolTipText = r.imagePath ?? string.Empty
                    };
                    _historyListView.Items.Add(item);
                }
            }
            finally
            {
                _historyListView.EndUpdate();
            }
        }

        /// <summary>
        /// 打开记录详情
        /// </summary>
        public void OpenDetail(ScanRecord record)
        {
            if (record == null || string.IsNullOrWhiteSpace(record.imagePath))
                return;

            if (!File.Exists(record.imagePath))
            {
                LogService.Warn($"[历史] 详情文件不存在: {record.imagePath}");
                return;
            }

            try
            {
                using (var tmp = Image.FromFile(record.imagePath))
                using (var bitmap = new Bitmap(tmp))
                using (var dlg = new ImageDetailForm(record.OrderNo, record.Time, record.imagePath, bitmap))
                {
                    dlg.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                LogService.Error($"[历史] 加载详情图片失败: {record.imagePath}", ex);
            }
        }

        public void Dispose()
        {
            Clear();
        }

        /// <summary>
        /// 根据 ID 移除记录
        /// </summary>
        public void RemoveRecord(int id)
        {
            if (id < 0)
                return;

            var record = _records.FirstOrDefault(r => r.Id == id);
            if (record != null)
            {
                _records.RemoveAll(r => r.Id == id);
                // 传递条码号(OrderNo)，而不是记录ID
                OnRecordDeleted?.Invoke(record.OrderNo);
                Render();
            }
        }
    }
}
