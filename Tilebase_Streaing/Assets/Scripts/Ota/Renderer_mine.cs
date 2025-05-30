using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Pcx{
    public class Renderer : MonoBehaviour
    {
        public MeshFilter meshFilter; // レンダリング対象の MeshFilter
        
        PlyImporter importer = new PlyImporter();
        MeshRenderer meshRenderer;
        private bool canRender = false; // レンダリング可能フラグ

        private const float renderInterval = 0.01f; // レンダリング間隔 (秒)

        void Start()
        {
            QualitySettings.vSyncCount = 0; // VSyncを無効化
            Application.targetFrameRate = 120; // 最大フレームレートを120FPSに設定
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = importer.GetDefaultMaterial(); 
       
            // 初期バッファ充填完了を受け取る
            Downloader.OnBufferReady += EnableRendering;
            StartCoroutine(RenderLoop());
        }

        // void Update()
        // {
        //     Debug.Log($"Current FPS: {1.0f / Time.deltaTime}");
        // }

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
                if (Time.realtimeSinceStartup >= nextRenderTime)
                {
                    // int fileNumber = -1;
                    // Debug.Log($"[lock Start] File:  Time: {Time.time}");
                    // lock (Downloader.renderQueue) // キューの操作を同期化
                    // {

                    if (Downloader.renderQueue.TryDequeue(out var item)) {
                        (byte[] data, int downloadIndex) = item;

                        if (downloadIndex >= 0){
                            yield return StartCoroutine(RenderFile(data, downloadIndex)); // レンダリング処理
                        }
                        nextRenderTime += renderInterval;
                    }

                    // if (Downloader.renderQueue.Count > 0)
                    // {
                    //     // fileNumber = Downloader.renderQueue.Dequeue();
                    //     (byte[] data, int downloadIndex) = Downloader.renderQueue.DeQueue();
                    //     if (downloadIndex >= 0)
                    //     {
                    //         // yield return StartCoroutine(RenderFile(fileNumber)); // レンダリング処理
                    //         yield return StartCoroutine(RenderFile(data, downloadIndex)); // レンダリング処理
                    //     }
                    //     // else
                    //     // {
                    //     //     yield return null; // レンダリング待機
                    //     // }
                    //     // lastRenderTime = Time.time; // 次の処理のために現在時刻を記録
                    //     nextRenderTime += renderInterval;
                        
                    // }
                    // // }
                    // Debug.Log($"[lock Finished] File:  Time: {Time.time}");


                }   
                yield return new WaitForSeconds(renderInterval); // レンダリング間隔を設定
                // yield return null; // 毎フレーム待機

            }
        }

        // IEnumerator RenderFile(int fileNumber)
        IEnumerator RenderFile(byte[] data, int downloadIndex)

        {
            //string path = "Assets/Download/" + fileNumber.ToString() + ".ply";
            //if (!File.Exists(path))
            //{
            //    Debug.LogError($"[Render Error] File not found: {path}");
            //    yield break;
            //}

            if (false) {
                yield break;
            }
       
            Debug.Log($"[Rendering Start] File: {downloadIndex}, Time: {Time.time}");            
            var mesh = importer.ImportAsMesh(data, downloadIndex); // ファイルをメッシュに変換            
            meshFilter.sharedMesh = mesh;           
            Debug.Log($"[Rendering Complete] File: {downloadIndex}, Time: {Time.time}");

        }

        private string GetFilePath(int fileNumber)
        {
            return Path.Combine(Application.dataPath, "Download", $"{fileNumber}.ply");
        }
    }
}