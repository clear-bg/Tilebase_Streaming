using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;


public class Download : MonoBehaviour
{
    private CameraLogger cameraLogger;
    public static int gridX = 2;
    public static int gridY = 3;
    public static int gridZ = 2;
    public static Vector3 globalMin = new Vector3(-1000f, -1000f, -1000f);  // 実際のPLY空間に合わせて
    public static Vector3 globalMax = new Vector3(2000f, 2000f, 2000f);

    // private string baseUrl = "http://localhost:8000/merge_ply";             // マージ済みファイルリクエスト
    private string baseUrl = "http://localhost:8000/Original_ply_20";       // オリジナル点群ファイルリクエスト
    // private string baseUrl = "http://localhost:8000/get_file";                 // タイル分割ありでリクエスト
    public static ConcurrentQueue<(byte[], int, double)> renderQueue = new ConcurrentQueue<(byte[], int, double)>();
    public int loopCount = -1;  // 再生回数。-1で無限ループ

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
        cameraLogger = FindObjectOfType<CameraLogger>();
        // Stopwatchの起動（基準タイミング）
        startTimestamp = Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency * 1000.0; // ms
        StartCoroutine(DownloadLoop());
    }

    IEnumerator DownloadLoop()
    {
        int loopCounter = 0;
        initialBufferFilled = false;

        while (loopCount == -1 || loopCounter < loopCount)
        {
            downloadIndex = 0;

            while (downloadIndex < totalFrames)
            {
                string url;
                string baseName = baseUrl.ToLower();

                if (baseName.Contains("get_file"))
                {
                    List<int> tileIndex = GetRequestTileIndex(downloadIndex);
                    string tileParam = string.Join(",", tileIndex);
                    string gridParam = $"{gridX}_{gridY}_{gridZ}";
                    string dataset = $"split_20_to_{gridParam}";
                    url = $"{baseUrl}?dataset={dataset}&frame={downloadIndex}&tiles={tileParam}&grid={gridParam}";
                }
                else if (baseName.Contains("merge_ply") || baseName.Contains("original_ply_20"))
                {
                    url = $"{baseUrl}?frame={downloadIndex}";
                }
                else
                {
                    UnityEngine.Debug.LogError($"Unknown endpoint baseUrl: {baseUrl}");
                    yield break;
                }

                UnityWebRequest uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
                uwr.downloadHandler = new DownloadHandlerBuffer();
                yield return uwr.SendWebRequest();

                double now = Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency * 1000.0;
                double elapsed = now - startTimestamp;

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    byte[] data = uwr.downloadHandler.data;
                    renderQueue.Enqueue((data, downloadIndex, elapsed));
                }
                else
                {
                    UnityEngine.Debug.LogError($"[Download Error URL] {url}\n[Download Error] File: {downloadIndex}, Error: {uwr.error}");
                }

                if (cameraLogger != null)
                {
                    cameraLogger.currentViewFrame = downloadIndex;
                }
                downloadIndex++;

                if (!initialBufferFilled && renderQueue.Count >= initialBufferSize)
                {
                    initialBufferFilled = true;
                    OnBufferReady?.Invoke();
                    UnityEngine.Debug.Log("初期バッファが充填されました。レンダリングを開始してください。");
                }
            }

            loopCounter++;
        }

        UnityEngine.Debug.Log("全ループ再生が完了しました。");
    }


    private List<int> GetRequestTileIndex(int frame)
    {
        // 強制的に 0〜124 の隣接タイルを返す
        return Enumerable.Range(0, 72).ToList();
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