using NAudio.Wave;
using System;
using System.IO;
using System.Threading;

namespace WinFormsBarcodeScannerApp.Utils
{
    /// <summary>
    /// 基于 NAudio 的音频播放器
    /// 支持多种音频格式，可与其他音频同时播放
    /// </summary>
    public class AudioPlayer : IDisposable
    {
        private IWavePlayer _waveOutDevice;
        private AudioFileReader _audioFileReader;
        private ManualResetEvent _playbackCompleteEvent;

        public void Play(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Console.WriteLine("文件不存在: " + filePath);
                return;
            }

            // 在后台线程播放，不受 Dispose 影响
            Thread playbackThread = new Thread(() =>
            {
                try
                {
                    using (var waveOut = new WaveOutEvent())
                    using (var audioFile = new AudioFileReader(filePath))
                    {
                        waveOut.Init(audioFile);
                        waveOut.Play();
                        // 等待播放完成
                        while (waveOut.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("播放失败: " + ex.Message);
                }
            });
            playbackThread.IsBackground = true;
            playbackThread.Start();
        }

        /// <summary>
        /// 等待当前播放完成（阻塞）
        /// </summary>
        public void WaitForPlaybackToFinish()
        {
            _playbackCompleteEvent?.WaitOne();
        }

        public void Stop()
        {
            if (_waveOutDevice != null)
            {
                _waveOutDevice.Stop();
                _waveOutDevice.Dispose();
                _waveOutDevice = null;
            }

            _audioFileReader?.Dispose();
            _audioFileReader = null;

            _playbackCompleteEvent?.Dispose();
            _playbackCompleteEvent = null;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
