using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace WinFormsBarcodeScannerApp.Services
{
    /// <summary>
    /// 回单上传服务：严格按需求执行"两步上传"：
    /// Step1: POST {backend.url}/hylir-common-center/upload (multipart/form-data, file)，返回文件 URL（字符串）
    /// Step2: 根据单号首字母判断接口，POST sheetNo / receiptUrl
    /// </summary>
    public static class UploadService
    {
        // 使用单例 HttpClient，避免端口耗尽
        private static readonly HttpClient _httpClient = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(5) // 5 分钟超时
            };
            client.DefaultRequestHeaders.Add("User-Agent", "WinFormsBarcodeScannerApp/1.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            return client;
        }

        public static void StartUploadAsync(
            string backendUrl,
            string sheetNo,
            string filePath,
            Action<string> onSuccessUrl,
            Action<string> onError,
            Action? onUploadComplete = null)
        {
            // 参数验证
            if (string.IsNullOrWhiteSpace(backendUrl))
            {
                onError?.Invoke("BackendUrl 未配置。");
                return;
            }
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                onError?.Invoke("文件不存在或路径为空。");
                return;
            }
            if (string.IsNullOrWhiteSpace(sheetNo))
            {
                onError?.Invoke("单号为空，无法上传。");
                return;
            }

            // 使用 Task.Run 异步执行
            Task.Run(async () =>
            {
                try
                {
                    bool success = await UploadAsync(backendUrl, sheetNo, filePath, onSuccessUrl, onError);
                    // 上传成功完成，通知调用方
                    if (success)
                    {
                        onUploadComplete?.Invoke();
                    }
                    
                }
                catch (Exception ex)
                {
                    LogService.Error("[Upload] 未捕获的异常", ex);
                    onError?.Invoke("上传失败：未预期的错误 - " + ex.Message);
                }
            });
        }

        private static async Task<bool> UploadAsync(
            string backendUrl,
            string sheetNo,
            string filePath,
            Action<string> onSuccessUrl,
            Action<string> onError)
        {
            long totalStart = DateTime.Now.Ticks;
            string baseUrl = backendUrl.TrimEnd('/');

            try
            {
                // Step 1: 上传图片文件（multipart/form-data）
                string fileUrl = await UploadImageAsync(baseUrl, filePath);
                long step1End = DateTime.Now.Ticks;
                LogService.Info($"[Upload] Step1 完成，耗时: {(step1End - totalStart) / 10000}ms");

                // Step 2: 上传回单信息
                await UploadReceiptInfoAsync(baseUrl, sheetNo, fileUrl);
                long step2End = DateTime.Now.Ticks;
                LogService.Info($"[Upload] Step2 完成，耗时: {(step2End - step1End) / 10000}ms");
                LogService.Info($"[Upload] 总耗时: {(step2End - totalStart) / 10000}ms");

                onSuccessUrl?.Invoke(fileUrl);
                return true;
            }
            catch (HttpRequestException httpEx)
            {
                await HandleHttpException(httpEx, onError);
                LogService.Error("[Upload] 请求超时", httpEx);
                onError?.Invoke("上传失败：请求超时（可能文件过大或网络较慢，请检查网络连接）"+ httpEx.Message);
                return false;
            }
            catch (TaskCanceledException taskEx)
            {
                LogService.Error("[Upload] 请求超时", taskEx);
                onError?.Invoke("上传失败：请求超时（可能文件过大或网络较慢，请检查网络连接）");
                return false;
            }
            catch (Exception ex)
            {
                LogService.Error("[Upload] 捕获到未预期的异常", ex);
                onError?.Invoke("上传失败：" + ex.Message + " (详细信息请查看 logs/app.log)");
                return false;
            }
        }

        /// <summary>
        /// Step 1: 上传图片文件（multipart/form-data）
        /// </summary>
        private static async Task<string> UploadImageAsync(string baseUrl, string filePath)
        {
            string uploadUrl = baseUrl + "/hylir-common-center/upload";
            string fileName = Path.GetFileName(filePath);
            LogService.Info($"[Upload] Step1 - URL: {uploadUrl}, 文件: {fileName}");

            // 读取文件字节
            long t1 = DateTime.Now.Ticks;
            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
            long t2 = DateTime.Now.Ticks;
            LogService.Info($"[Upload] 读取文件完成，大小: {fileBytes.Length} 字节, 耗时: {(t2 - t1) / 10000}ms");

            // 构建 multipart/form-data 请求
            using (var content = new MultipartFormDataContent())
            {
                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                content.Add(fileContent, "file", fileName);

                long t3 = DateTime.Now.Ticks;
                HttpResponseMessage response = await _httpClient.PostAsync(uploadUrl, content);
                long t4 = DateTime.Now.Ticks;
                LogService.Info($"[Upload] 请求发送完成，状态码: {response.StatusCode}, 耗时: {(t4 - t3) / 10000}ms");

                // 确保响应成功
                response.EnsureSuccessStatusCode();

                // 读取响应
                string fileUrl = await response.Content.ReadAsStringAsync();
                fileUrl = fileUrl.Trim();

                if (string.IsNullOrWhiteSpace(fileUrl))
                {
                    throw new Exception("上传接口未返回文件 URL。");
                }

                LogService.Info($"[Upload] Step1 成功，返回 URL: {fileUrl}");
                return fileUrl;
            }
        }

        /// <summary>
        /// Step 2: 上传回单信息
        /// </summary>
        private static async Task UploadReceiptInfoAsync(string baseUrl, string sheetNo, string fileUrl)
        {
            string apiBase = baseUrl + "/hylir-mes-center";
            string relativePath = sheetNo.StartsWith("J", StringComparison.OrdinalIgnoreCase)
                ? "/api/v1/integration/chery/tmmmjissheet/receipt/upload"
                : "/api/v1/integration/chery/tmmmjitsheet/receipt/upload";

            string receiptUrl = apiBase + relativePath;
            LogService.Info($"[Upload] Step2 - URL: {receiptUrl}, 单号: {sheetNo}");

            // 构建 URL 编码的请求体
            string postData = $"sheetNo={Uri.EscapeDataString(sheetNo)}&receiptUrl={Uri.EscapeDataString(fileUrl)}";

            using (StringContent receiptContent = new StringContent(postData, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded"))
            {
                long t1 = DateTime.Now.Ticks;
                HttpResponseMessage receiptResponse = await _httpClient.PostAsync(receiptUrl, receiptContent);
                long t2 = DateTime.Now.Ticks;
                LogService.Info($"[Upload] Step2 请求完成，状态码: {receiptResponse.StatusCode}, 耗时: {(t2 - t1) / 10000}ms");

                receiptResponse.EnsureSuccessStatusCode();

                // 读取响应（不关心内容，但需要消费流）
                await receiptResponse.Content.ReadAsStringAsync();
                LogService.Info($"[Upload] Step2 成功");
            }
        }

        /// <summary>
        /// 处理 HTTP 异常，尝试读取错误响应内容
        /// </summary>
        private static async Task HandleHttpException(HttpRequestException httpEx, Action<string> onError)
        {
            LogService.Error("[Upload] HttpRequestException", httpEx);
            string errorMsg = "上传失败：";

            if (httpEx.InnerException != null)
            {
                errorMsg += httpEx.InnerException.Message;
            }
            else
            {
                errorMsg += httpEx.Message;
            }

            onError?.Invoke(errorMsg);
        }
    }
}
