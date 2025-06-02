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
    public class Download : MonoBehaviour
    {
        public static ConcurrentQueue<(byte[], int, int)> renderQueue = new ConcurrentQueue<(byte[], int, int)>();
        public string baseUrl = "http://localhost:8000/get_file"; // サーバURL
        public int totalFrames = 300; // 総フレーム数
        public int numClouds = 4; // 点群の数（PointCloudRendererの数）

        private int downloadIndex = 0;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private bool initialBufferFilled = false;
        public int initialBufferSize = 30;
        public static event Action OnBufferReady;

        void Start()
        {
            for (int i = 0; i < numClouds; i++)
            {
                renderQueues[i] = new ConcurrentQueue<(byte[], int)>();
            }
            StartCoroutine(DownloadLoop());
        }

        public static ConcurrentQueue<(byte[], int)>[] renderQueues;

        IEnumerator DownloadLoop()
        {
            renderQueues = new ConcurrentQueue<(byte[], int)>[numClouds];
            for (int i = 0; i < numClouds; i++)
            {
                renderQueues[i] = new ConcurrentQueue<(byte[], int)>();
            }

            while (downloadIndex < totalFrames)
            {
                for (int cloudId = 0; cloudId < numClouds; cloudId++)
                {
                    string url = $"{baseUrl}/{cloudId}/{downloadIndex}";
                    yield return DownloadAndEnqueue(url, cloudId, downloadIndex);
                }
                downloadIndex++;
                if (!initialBufferFilled && CheckInitialBuffer())
                {
                    initialBufferFilled = true;
                    OnBufferReady?.Invoke();
                    Debug.Log("初期バッファ充填完了");
                }
                yield return null;
            }
        }

        IEnumerator DownloadAndEnqueue(string url, int cloudId, int frameIndex)
        {
            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                uwr.downloadHandler = new DownloadHandlerBuffer();
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    byte[] data = uwr.downloadHandler.data;
                    renderQueues[cloudId].Enqueue((data, frameIndex));
                    Debug.Log($"[Download] Enqueued Cloud {cloudId}, Frame {frameIndex}");
                }
                else
                {
                    Debug.LogWarning($"[Download] Failed to download Cloud {cloudId}, Frame {frameIndex}: {uwr.error}");
                }
            }
        }

        bool CheckInitialBuffer()
        {
            int totalBuffered = 0;
            for (int i = 0; i < numClouds; i++)
            {
                totalBuffered += renderQueues[i].Count;
            }
            return totalBuffered >= initialBufferSize;
        }

        private void OnApplicationQuit()
        {
            cancellationTokenSource.Cancel();
        }
    }
}
