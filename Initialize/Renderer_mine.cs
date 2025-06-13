using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;

namespace Pcx{
    public class Renderer : MonoBehaviour
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
            Downloader.OnBufferReady += EnableRendering;
            // StartCoroutine(RenderLoop());
        }


        void EnableRendering()
        {
            // canRender = true;
            StartCoroutine(RenderLoop());
        }

        IEnumerator RenderLoop()
        {

            float nextFrameTime = Time.realtimeSinceStartup + renderInterval;

            while (true)
            {

                float now = Time.realtimeSinceStartup;
                if(now < nextFrameTime){
                    yield return new WaitForSecondsRealtime(nextFrameTime-now);
                }




                if (Downloader.renderQueue.TryDequeue(out var item)) {
                    (byte[] data, int downloadIndex, string quality) = item;

                    if (downloadIndex >= 0){
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


                  
                // yield return new WaitForSeconds(renderInterval); // レンダリング間隔を設定
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
            
       
            // Debug.Log($"[Rendering Start] File: {downloadIndex}, Time: {Time.time}");  
            
            var mesh = importer.ImportAsMesh(data, downloadIndex); // ファイルをメッシュに変換            
            meshFilter.sharedMesh = mesh;   
            
              
            
            // Debug.Log($"[Rendering Complete] File: {downloadIndex}, Time: {Time.time}");
              

        }

        private string GetFilePath(int fileNumber)
        {
            return Path.Combine(Application.dataPath, "Download", $"{fileNumber}.ply");
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
            
            

            Debug.Log($"平均FPS: {1f / (fpsRecords.Average()):F2}");
        }


        
    }
}