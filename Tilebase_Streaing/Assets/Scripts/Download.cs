using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.Networking;
using System;

namespace Pcx
{
    public class Download : MonoBehaviour
    {
        public static ConcurrentQueue<(byte[], int)>[] renderQueues;

        public string baseUrl = "http://172.16.51.65:8000/get_file"; // デスクトップ_研究室_有線
        // public string baseUrl = "http://172.16.51.59:8000/get_file"; // デスクトップ_研究室_無線
        // public string baseUrl = "http://172.16.51.59:8000/get_file"; // デスクトップ_家_有線
        // public string baseUrl = "http://172.16.51.65:8000/get_file"; // ノート_有線
        // public string baseUrl = "http://172.16.51.65:8000/get_file"; // ノート_無線
        public int totalFrames = 300; // 総フレーム数
        public int numTiles = 12;     // タイル数

        private int downloadIndex = 0;

        void Start()
        {
            // 各タイルごとにキューを初期化
            renderQueues = new ConcurrentQueue<(byte[], int)>[numTiles];
            for (int i = 0; i < numTiles; i++)
            {
                renderQueues[i] = new ConcurrentQueue<(byte[], int)>();
            }

            StartCoroutine(DownloadLoop());
        }

        IEnumerator DownloadLoop()
        {
            while (downloadIndex < totalFrames)
            {
                int[] currentTileIDs = GetCurrentTileIDs();

                foreach (int tileID in currentTileIDs)
                {
                    string url = $"{baseUrl}/{tileID}/{downloadIndex}";
                    yield return DownloadAndEnqueue(url, tileID, downloadIndex);
                }

                downloadIndex++;
                yield return null;
            }
        }

        int[] GetCurrentTileIDs()
        {
            // テスト用：固定で 0, 2, 4, 6, 8, 10 を選択
            return new int[] { 0, 2, 4, 6, 8, 10 };
        }

        IEnumerator DownloadAndEnqueue(string url, int tileID, int frameIndex)
        {
            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                uwr.downloadHandler = new DownloadHandlerBuffer();
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    byte[] data = uwr.downloadHandler.data;
                    renderQueues[tileID].Enqueue((data, frameIndex));
                    Debug.Log($"[Download] Enqueued Tile {tileID}, Frame {frameIndex}");
                }
                else
                {
                    Debug.LogWarning($"[Download] Failed: Tile {tileID}, Frame {frameIndex}, Error: {uwr.error}");
                }
            }
        }
    }
}
