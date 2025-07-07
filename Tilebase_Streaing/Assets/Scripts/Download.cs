using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;


public class Download : MonoBehaviour
{
    private int gridX = 5;
    private int gridY = 5;
    private int gridZ = 5;

    private bool doTileDistribute = true;  // タイル分割/結合をするか決定
    // private string baseUrl = "http://localhost:8000/merge_ply";             // 完全マージ済みファイル用
    private string baseUrl = "http://localhost:8000/get_file";                 // エンドポイント統一
    public static ConcurrentQueue<(byte[], int, double)> renderQueue = new ConcurrentQueue<(byte[], int, double)>();
    public int initialBufferSize = 30; // 初期バッファサイズ
    public int totalFrames = 300; // 総フレーム数
    private int downloadIndex = 0; // ダウンロード進行インデックス
    private bool initialBufferFilled = false;

    public delegate void BufferReady(); // 初期バッファ完了時の通知
    public static event BufferReady OnBufferReady;
    private List<float> downloadTimes = new List<float>(); // 各ダウンロード時間を格納するリスト

    public static double startTimestamp = -1; // stopwatch起点

    private System.Random random = new System.Random();

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
            string url;
            if (doTileDistribute)
            {
                List<int> tileIndex = GetRequestTileIndex(downloadIndex);
                string tileParam = string.Join(",", tileIndex);
                string gridParam = $"{gridX}_{gridY}_{gridZ}";
                string dataset = $"split_20_to_{gridParam}";
                url = $"{baseUrl}?dataset={dataset}&frame={downloadIndex}&tiles={tileParam}&grid={gridParam}";
            }
            else
            {
                // baseUrlは「http://xxx/merge_ply」でコメントアウト等で切替
                url = $"{baseUrl}?frame={downloadIndex}";
            }

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
                UnityEngine.Debug.LogError($"[Download Error URL] {url}\n[Download Error] File: {downloadIndex}, Error: {uwr.error}");
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
        int index = (frame / 60) % tileSets.Length;  // 60フレーム = 2秒ごとに切替
        // return tileSets[index];

        // 強制的に 0〜124 の全タイルを返す（5x5x5 = 125個）
        return Enumerable.Range(0, 72).ToList();
        // return new List<int> { 1, 2 };


        // アルゴリズムは後で追加，とりあえず固定のタイル番号をリクエスト
        // return new List<int> { 2, 3, 4, 5, 8, 9, 10, 11 };
        // return new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
    }

    private List<int>[] tileSets = new List<int>[]
    {
        new List<int> {3, 5, 9, 11},              // tiles_1: 前面上側
        new List<int> {1, 3, 7, 9},               // tiles_2: 前面下部
        new List<int> {1, 3, 5},                  // tiles_3: 前面左
        new List<int> {7, 9, 11},                 // tiles_4: 前面右
        new List<int> {1, 3, 5, 7, 9, 11},        // tiles_5: 前面全て
    };

}