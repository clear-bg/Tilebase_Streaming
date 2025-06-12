using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;


namespace Pcx
{
    public class Rendering : MonoBehaviour
    {
        public MeshFilter meshFilter; // レンダリング対象の MeshFilter
        PlyImporter importer = new PlyImporter();
        MeshRenderer meshRenderer;
        private bool canRender = false; // レンダリング可能フラグ
        private const float renderInterval = 0.01f; // 30fps

        private List<(int frame, double downloadedMs, double renderedMs, double intervalMs)> logs = new List<(int, double, double, double)>();

        private string logPath;
        private double prevRenderedMs = -1;

        void Start()
        {
            QualitySettings.vSyncCount = 0; // VSyncを無効化
            Application.targetFrameRate = 120; // 最大フレームレートを120FPSに設定
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = importer.GetDefaultMaterial();

            logPath = Path.Combine(Application.dataPath, "Log/log_rendering.csv");

            // 初期バッファ充填完了を受け取る
            Download.OnBufferReady += EnableRendering;
            StartCoroutine(RenderLoop());
        }

        void EnableRendering()
        {
            canRender = true;
        }

        IEnumerator RenderLoop()
        {


            float nextRenderTime = Time.realtimeSinceStartup;
            while (true)
            {

                if (!canRender)
                {
                    yield return null; // 初期バッファが満たされるまで待機
                    continue;
                }

                if (!Download.renderQueue.TryDequeue(out var item))
                {
                    yield break; // キューが空なら何もせず抜ける
                }

                (byte[] data, int frame, double downloadedMs) = item; // ← 必ず3要素で受ける！

                yield return StartCoroutine(RenderFile(data, frame));
                double now = System.Diagnostics.Stopwatch.GetTimestamp() / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
                double renderedMs = now - Download.startTimestamp;

                // 追加ここから
                double intervalMs = prevRenderedMs >= 0 ? renderedMs - prevRenderedMs : 0;
                logs.Add((frame, downloadedMs, renderedMs, intervalMs));
                prevRenderedMs = renderedMs;
                // 追加ここまで

                yield return new WaitForSeconds(renderInterval);

                // if (downloadIndex < 0)
                // {
                //     yield break; // 無効なインデックスはスキップ
                // }

                // yield return StartCoroutine(RenderFile(data, downloadIndex)); // レンダリング処理
                // nextRenderTime += renderInterval;

            }
        }

        IEnumerator RenderFile(byte[] data, int downloadIndex)
        {
            if (false)
            {
                yield break;
            }

            var mesh = importer.ImportAsMesh(data, downloadIndex); // ファイルをメッシュに変換
            meshFilter.sharedMesh = mesh;
        }

        void OnApplicationQuit()
        {
            ExportLogToCSV();
        }
        void OnDisable()
        {
            ExportLogToCSV();
        }

        void ExportLogToCSV()
        {
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