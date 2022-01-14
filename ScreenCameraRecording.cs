using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Xabe.FFmpeg;
using System.Threading.Tasks;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Diagnostics;
using Xabe.FFmpeg.Events;
using System;

namespace CommonTool
{
    public class ScreenCameraRecording
    {
        private IConversion screenRecording;
        private CancellationTokenSource screenRecordingTokenSource;

        private IConversion cameraRecording;
        private CancellationTokenSource cameraRecordingTokenSource;

        public readonly string FFmpegPath;
        public readonly string ScreenOutputPath;
        public readonly string CameraOutputPath;
        public readonly string UUID;
        public readonly bool ToolSign;

        public ScreenCameraRecording()
        {
#if UNITY_EDITOR
            FFmpegPath = @"D:\ffmpeg";
            ToolSign = true;
            UUID = Guid.NewGuid().ToString();
            ScreenOutputPath = Path.Combine(@"D:\", "video", $"{UUID}_0.mkv");
            CameraOutputPath = Path.Combine(@"D:\", "video", $"{UUID}_1.mkv");
            FFmpeg.SetExecutablesPath(FFmpegPath);
            var dirScreen = Path.Combine(@"D:\", "video");
            if (!Directory.Exists(dirScreen)) Directory.CreateDirectory(dirScreen);
#endif
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            FFmpegPath = Application.dataPath;
            if (File.Exists(Path.Combine(FFmpegPath, "ffmpeg.exe")) && File.Exists(Path.Combine(FFmpegPath, "ffprobe.exe")))
            {
                ToolSign = true;
                UUID = Guid.NewGuid().ToString();
                ScreenOutputPath = Path.Combine(Application.dataPath, "video", $"{UUID}_0.mkv");
                CameraOutputPath = Path.Combine(Application.dataPath, "video", $"{UUID}_1.mkv");
                FFmpeg.SetExecutablesPath(FFmpegPath);
                var dirScreen = Path.Combine(Application.dataPath, "video");
                if (!Directory.Exists(dirScreen)) Directory.CreateDirectory(dirScreen);
            }
            else
            {
                UnityEngine.Debug.Log("缺少ffmpeg");
            }
#endif
        }

        /// <summary>
        /// 屏幕录像
        /// </summary>
        public async Task ScreenRecording()
        {
            if (ToolSign)
            {
                UnityEngine.Debug.Log("开启屏幕录像");
                try
                {
                    screenRecordingTokenSource = new CancellationTokenSource();
                    screenRecording = FFmpeg.Conversions.New()
                         .SetInputFormat($"ffmpeg -video_size 1920x1080 -framerate 30 -f gdigrab -i desktop -f dshow -i audio={"virtual-audio-capturer"} -c:v h264_nvenc -qp 30")
                         .SetOutput(ScreenOutputPath);
                    await screenRecording.Start(screenRecordingTokenSource.Token);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.Log(ex);
                }
            }
        }

        /// <summary>
        /// 摄像头录像
        /// </summary>
        public async Task CameraRecording()
        {
            if (ToolSign)
            {
                UnityEngine.Debug.Log("开启摄像头录像");
                try
                {
                    cameraRecordingTokenSource = new CancellationTokenSource();
                    cameraRecording = FFmpeg.Conversions.New()
                         .SetInputFormat($"ffmpeg -f dshow -i video=\"Microsoft® LifeCam HD-3000\" -framerate 30 -vcodec libx264 -qp 30 -preset ultrafast")
                         .SetOutput(CameraOutputPath);
                    await cameraRecording.Start(cameraRecordingTokenSource.Token);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.Log(ex);
                }
            }
        }

        public void StopScreenRecording()
        {
            if (screenRecordingTokenSource != null)
                screenRecordingTokenSource.Cancel();
        }

        public void StopCameraRecording()
        {
            if (cameraRecordingTokenSource != null)
                cameraRecordingTokenSource.Cancel();
        }
    }
}
