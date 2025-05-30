using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.IO;

using UnityEngine.Networking;
using System.Xml.Linq;
using System.Linq;
using System.Threading;
using System;
using UnityEngine.UI;

namespace Pcx
{
    public class Rendering : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private PlyImporter importer = new PlyImporter();
        private const float renderInterval = 0.033f; // 30fps相当
        // Start is called before the first frame update
        void Start()
        {
            QualitySettings.vSyncCount = 0;
            if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
            StartCoroutine(RenderLoop());
        }

        // Update is called once per frame
        void Update()
        {
            // InvokeRepeating関数で制御するため，Update関数は使わない
        }

        IEnumerator RenderLoop()
        {
            float nextRenderTime = Time.realtimeSinceStartup;
            while (true)
            {
                if (Time.realtimeSinceStartup >= nextRenderTime)
                {
                    if (Downloader.renderQueue.TryDequeue(out var item))
                    {
                        (byte[] data, int downloadIndex) = item;
                        if (downloadIndex >= 0)
                        {
                            yield return StartCoroutine(RenderFile(data, downloadIndex)); // レンダリング処理
                        }
                        nextRenderTime += renderInterval;
                    }

                    yield return new WaitForSeconds(renderInterval); // レンダリング間隔を設定
                    // yield return null; // 毎フレーム待機
                }
            }
        }

        IEnumerator RenderFile(byte[] data, int downloadIndex)
        {
            Debug.Log($"[Rendering Start] File: {downloadIndex}, Time: {Time.time}");
            var mesh = importer.ImportAsMesh(data, downloadIndex); // ファイルをメッシュに変換            
            meshFilter.sharedMesh = mesh;
            Debug.Log($"[Rendering Complete] File: {downloadIndex}, Time: {Time.time}");
            yield break;
        }
    }
}