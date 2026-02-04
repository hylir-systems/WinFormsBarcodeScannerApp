using System;
using System.Collections.Generic;

namespace WinFormsBarcodeScannerApp.Services.autocapture
{
    /// <summary>
    /// 条码去重器：避免重复识别相同条码
    /// </summary>
    public sealed class BarcodeDeduplicator
    {
        private readonly Dictionary<string, DateTime> _recentBarcodes = new Dictionary<string, DateTime>();
        private const int DeduplicationMinutes = 5; // 5分钟内不重复识别

        public bool IsDuplicate(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return true;

            DateTime now = DateTime.Now;
            
            // 清理过期条目
            CleanupExpired(now);

            // 检查是否已存在
            if (_recentBarcodes.ContainsKey(barcode))
            {
                return true;
            }

            // 添加新条目
            _recentBarcodes[barcode] = now;
            return false;
        }

        private void CleanupExpired(DateTime now)
        {
            var expired = new List<string>();
            foreach (var kvp in _recentBarcodes)
            {
                if ((now - kvp.Value).TotalMinutes > DeduplicationMinutes)
                {
                    expired.Add(kvp.Key);
                }
            }

            foreach (var key in expired)
            {
                _recentBarcodes.Remove(key);
            }
        }

        public void Clear()
        {
            _recentBarcodes.Clear();
        }

        /// <summary>
        /// 移除指定条码（删除历史记录时调用）
        /// </summary>
        public void Remove(string barcode)
        {
            if (!string.IsNullOrWhiteSpace(barcode))
            {
                _recentBarcodes.Remove(barcode);
            }
        }
    }
}

