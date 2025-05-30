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
        public static ConcurrentDictionary<int, ConcurrentQueue<(byte[], int)>> renderQueues = new ConcurrentDictionary<int, ConcurrentQueue<(byte[], int)>>();
        public string baseUrl = "http://yourserver.com/pointcloud"; // ベースURL(サーバIPアドレス)
        public int totalFrames = 300; // 総フレーム数
        private int downloadIndex = 0; // ダウンロード進行インデックス
        public int numClouds = 4; // 点群の数（PointCloudRendererに対応）

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        // Start is called before the first frame update
        void Start()
        {
            // 各点群用のキューを初期化
            for (int i = 0; i < numClouds; i++)
            {
                renderQueues[i] = new ConcurrentQueue<(byte[], int)>();
            }

            // ダウンロード開始
            StartCoroutine(DownloadLoop());
        }

        // Update is called once per frame
        void Update()
        {

        }

        IEnumerator DownloadLoop()
        {
            while (downloadIndex < totalFrames)
            {
                for (int cloudId = 0; cloudId < numClouds; cloudId++)
                {
                    string url = $"{baseUrl}/cloud{cloudId}/{downloadIndex}.ply";
                    Debug.Log($"[Download] Requesting {url}");
                    yield return DownloadAndEnqueue(url, cloudId, downloadIndex);
                }
                downloadIndex++;
                yield return null;
            }
        }

        IEnumerator DownloadAndEnqueue(string url, int cloudId, int index)
        {
            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                uwr.downloadHandler = new DownloadHandlerBuffer();
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    byte[] data = uwr.downloadHandler.data;
                    renderQueues[cloudId].Enqueue((data, index));
                    Debug.Log($"[Download] Enqueued Frame {index} for Cloud {cloudId}");
                }
                else
                {
                    Debug.LogWarning($"[Download] Failed to download {url}: {uwr.error}");
                }
            }
        }

        private void OnApplicationQuit()
        {
            cancellationTokenSource.Cancel();
        }
    }
}

