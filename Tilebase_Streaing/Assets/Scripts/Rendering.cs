using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;

namespace Pcx
{
    public class Rendering : MonoBehaviour
    {
        public MeshFilter meshFilter; // レンダリング対象の MeshFilter
        PlyImporter importer = new PlyImporter();
        MeshRenderer meshRenderer;
        private bool canRender = false; // レンダリング可能フラグ
        private const float renderInterval = 1f / 30f; // レンダリング間隔 (秒)
        private List<float> renderIntervalTimes = new List<float>(); // レンダリング間隔を格納するリスト
        private float lastRenderTime = -1f; // 前回の描画時間
        private List<float> fpsRecords = new List<float>(); // フレーム間隔リスト


        private List<(int frame, double downloadedMs, double renderedMs, double intervalMs)> logs = new List<(int, double, double, double)>();

        private string logPath;
        private double prevRenderedMs = -1;

        void Start()
        {
            QualitySettings.vSyncCount = 0; // VSyncを無効化
            Application.targetFrameRate = 120; // 最大フレームレートを120FPSに設定
            meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();

            meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();

            meshRenderer.sharedMaterial = importer.GetDefaultMaterial();

            // logPath = Path.Combine(Application.dataPath, "Log/log_rendering.csv");
            logPath = null; // レンダリングログ無効化
            // ログファイルを初期化
            File.WriteAllText(logPath, "Frame,DownloadedTime(ms),RenderedTime(ms),RenderDelay(ms),FrameInterval(ms)\n", Encoding.UTF8);

            // 初期バッファ充填完了を受け取る
            Download.OnBufferReady += EnableRendering;
        }

        void EnableRendering()
        {
            if (canRender) return; // すでに起動していれば無視
            canRender = true;
            StartCoroutine(RenderLoop());
        }

        IEnumerator RenderLoop()
        {
            float nextFrameTime = Time.realtimeSinceStartup + renderInterval;

            while (true)
            {
                float now = Time.realtimeSinceStartup;
                if (now < nextFrameTime)
                {
                    yield return new WaitForSecondsRealtime(nextFrameTime - now);
                }


                if (Download.renderQueue.TryDequeue(out var item))
                {
                    (byte[] data, int downloadIndex, double downloadedMs) = item;

                    if (downloadIndex >= 0)
                    {
                        yield return StartCoroutine(RenderFile(data, downloadIndex)); // レンダリング処理

                        // CSVファイル保存関係
                        double now_csv = System.Diagnostics.Stopwatch.GetTimestamp() / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
                        double renderedMs = now_csv - Download.startTimestamp;
                        double intervalMs = prevRenderedMs >= 0 ? renderedMs - prevRenderedMs : 0;
                        logs.Add((downloadIndex, downloadedMs, renderedMs, intervalMs));
                        prevRenderedMs = renderedMs;
                        // CSVファイル保存関係 ここまで

                        float currentTime = Time.realtimeSinceStartup;
                        if (lastRenderTime > 0f)
                        {
                            float interval = currentTime - lastRenderTime;
                            fpsRecords.Add(interval);
                        }
                        lastRenderTime = currentTime;
                    }

                }
                nextFrameTime += renderInterval;
            }
        }

        IEnumerator RenderFile(byte[] data, int downloadIndex)
        {
            if (false)
            {
                yield break;
            }

            Debug.Log($"[Rendering Start] File: {downloadIndex}, Time: {Time.time}");
            var mesh = importer.ImportAsMesh(data, downloadIndex);
            meshFilter.sharedMesh = mesh;
            Debug.Log($"[Rendering Complete] File: {downloadIndex}, Time: {Time.time}");
        }

        void OnApplicationQuit()
        {
            if (fpsRecords.Count == 0) return;

            float sum = 0f;
            foreach (var interval in fpsRecords)
            {
                float fps = 1f / interval;

                sum += fps;
            }

            float avgFps = sum / fpsRecords.Count;

            Debug.Log($"平均FPS: {1f / fpsRecords.Average():F2}");
            ExportLogToCSV();
        }
        void OnDisable()
        {
            Download.OnBufferReady -= EnableRendering;
            Download.renderQueue = new System.Collections.Concurrent.ConcurrentQueue<(byte[], int, double)>();
            ExportLogToCSV();
        }

        void ExportLogToCSV()
        {
            if (string.IsNullOrEmpty(logPath)) return; // ← 追加：出力無効化

            var sb = new StringBuilder();
            sb.AppendLine("Frame,DownloadedTime(ms),RenderedTime(ms),RenderDelay(ms),FrameInterval(ms)");
            foreach (var log in logs)
            {
                double delay = log.renderedMs - log.downloadedMs;
                sb.AppendLine($"{log.frame},{log.downloadedMs:F3},{log.renderedMs:F3},{delay:F3},{log.intervalMs:F3}");
            }
            File.WriteAllText(logPath, sb.ToString(), Encoding.UTF8);
            Debug.Log($"CSV log exported: {logPath}");
        }

    }
}