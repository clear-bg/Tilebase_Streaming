using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Concurrent;
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

        void Start()
        {
            QualitySettings.vSyncCount = 0; // VSyncを無効化
            Application.targetFrameRate = 120; // 最大フレームレートを120FPSに設定
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = importer.GetDefaultMaterial();

            // 初期バッファ充填完了を受け取る
            Download.OnBufferReady += EnableRendering;
            // StartCoroutine(RenderLoop());
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
                    (byte[] data, int downloadIndex) = item;

                    if (downloadIndex >= 0)
                    {
                        yield return StartCoroutine(RenderFile(data, downloadIndex)); // レンダリング処理

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
            var mesh = importer.ImportAsMesh(data, downloadIndex); // ファイルをメッシュに変換
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
        }
        void OnDisable()
        {
            Download.OnBufferReady -= EnableRendering;
            Download.renderQueue = new System.Collections.Concurrent.ConcurrentQueue<(byte[], int)>();
        }
    }
}