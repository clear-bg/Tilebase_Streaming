using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Concurrent;
using System.Diagnostics;

public class Download : MonoBehaviour
{

    public string baseUrl = "http://172.16.51.65:8000/get_file"; // デスクトップ_研究室_有線
    // public string baseUrl = "http://172.16.51.59:8000/get_file"; // デスクトップ_研究室_無線
    // public string baseUrl = "http://192.168.1.18:8000/get_file"; // デスクトップ_家_有線
    // public string baseUrl = "http://172.16.51.65:8000/get_file"; // ノート_有線
    // public string baseUrl = "http://172.16.51.65:8000/get_file"; // ノート_無線
    public static ConcurrentQueue<(byte[], int, double)> renderQueue = new ConcurrentQueue<(byte[], int, double)>();
    public int initialBufferSize = 30; // 初期バッファサイズ
    public int totalFrames = 300; // 総フレーム数
    private int downloadIndex = 0; // ダウンロード進行インデックス
    private bool initialBufferFilled = false;

    public delegate void BufferReady(); // 初期バッファ完了時の通知
    public static event BufferReady OnBufferReady;
    private List<float> downloadTimes = new List<float>(); // 各ダウンロード時間を格納するリスト

    public static double startTimestamp = -1; // stopwatch起点

    void Start()
    {
        // Stopwatchの起動（基準タイミング）
        startTimestamp = Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency * 1000.0; // ms
        StartCoroutine(DownloadLoop());
    }

    IEnumerator DownloadLoop()
    {
        while (downloadIndex < totalFrames)
        {
            List<int> tileIndex = GetRequestTileIndex(downloadIndex);
            string tileParam = string.Join(",", tileIndex);

            string url = $"{baseUrl}?frame={downloadIndex}&tiles={tileParam}";
            // string path = GetFilePath(downloadIndex);
            // Debug.Log($"Request url: {url}");

            UnityWebRequest uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            uwr.downloadHandler = new DownloadHandlerBuffer();

            yield return uwr.SendWebRequest();

            double now = Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency * 1000.0;
            double elapsed = now - startTimestamp; // 0ms起点

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                byte[] data = uwr.downloadHandler.data;
                // Debug.Log(data);
                renderQueue.Enqueue((data, downloadIndex, elapsed)); // ← elapsed(ダウンロード時刻ms)も一緒に入れる！
            }
            else
            {
                UnityEngine.Debug.LogError($"[Download Error] File: {downloadIndex}, Error: {uwr.error}");
            }

            downloadIndex++;

            // 初期バッファが充填されたら通知
            if (!initialBufferFilled && renderQueue.Count >= initialBufferSize)
            {
                initialBufferFilled = true;
                OnBufferReady?.Invoke(); // 初期バッファ完了を通知
                UnityEngine.Debug.Log("初期バッファが充填されました。レンダリングを開始してください。");
            }
        }
    }

    // private string GetFilePath(int fileNumber)
    // {
    //     return Path.Combine(Application.dataPath, "Download", $"{fileNumber}.ply");
    // }

    private List<int> GetRequestTileIndex(int frame)
    {
        // アルゴリズムは後で追加，とりあえず固定のタイル番号をリクエスト
        return new List<int> { 2, 3, 4, 5, 8, 9, 10, 11 };
    }

}