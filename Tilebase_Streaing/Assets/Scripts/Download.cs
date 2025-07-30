using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.IO;


public class Download : MonoBehaviour
{
    private CameraLogger cameraLogger;
    // private string baseUrl = "http://localhost:8000/merge_ply";             // マージ済みファイルリクエスト
    // private string baseUrl = "http://localhost:8000/Original_ply_20";       // オリジナル点群ファイルリクエスト
    private string baseUrl = "http://localhost:8000/get_file";
    public static bool logEnabled = false;
    public static int gridX = 2;
    public static int gridY = 3;
    public static int gridZ = 2;
    public static Vector3 globalMin = new Vector3(-1000f, -1000f, -1000f);  // 実際のPLY空間に合わせて
    public static Vector3 globalMax = new Vector3(2000f, 2000f, 2000f);

    // タイル分割ありでリクエスト
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
        DownloadUtility.DeleteAllXmlFiles();

        cameraLogger = FindObjectOfType<CameraLogger>();

        StartCoroutine(DownloadAllXMLs(OnXmlDownloadComplete));
    }

    private void OnXmlDownloadComplete()
    {
        startTimestamp = Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency * 1000.0;
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

    IEnumerator DownloadAllXMLs(System.Action onComplete)
    {
        string xmlFolder = Path.Combine(Application.dataPath, "XML");
        if (!Directory.Exists(xmlFolder)) Directory.CreateDirectory(xmlFolder);

        for (int i = 0; i < totalFrames; i++)
        {
            string gridParam = $"{gridX}_{gridY}_{gridZ}";
            string url = $"http://localhost:8000/get_xml?frame={i}&grid={gridParam}";

            UnityWebRequest uwr = UnityWebRequest.Get(url);
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                string savePath = Path.Combine(xmlFolder, $"{i:000}.xml");
                System.IO.File.WriteAllBytes(savePath, uwr.downloadHandler.data);
                UnityEngine.Debug.Log($"Saved XML {i:000}");
            }
            else
            {
                UnityEngine.Debug.LogError($"Failed to download XML for frame {i:100}: {uwr.error}");
            }
        }

        UnityEngine.Debug.Log("[DownloadAllXMLs] 完了: onComplete()を呼び出します");
        onComplete?.Invoke();
    }


    private List<int> GetRequestTileIndex(int frame)
    {
        GazeController gaze = FindObjectOfType<GazeController>();
        if (gaze == null)
        {
            UnityEngine.Debug.LogError("[GazeController] が見つかりません。シーンに追加されていますか？");
            return new List<int>(); // ← 空のリストなど適切なreturnを必ず入れる
        }

        gaze.SetFrame(frame);
        Vector3 origin = gaze.CurrentPosition;
        Vector3 direction = gaze.CurrentForward;
        return TileSelector.GetVisibleTilesFromXML(frame, origin, direction);
    }
}

public static class DownloadUtility
{
    public static void DeleteAllXmlFiles()
    {
        string xmlFolder = Path.Combine(Application.dataPath, "XML");
        if (Directory.Exists(xmlFolder))
        {
            foreach (string file in Directory.GetFiles(xmlFolder, "*.xml"))
            {
                File.Delete(file);
            }
            UnityEngine.Debug.Log("XMLファイルを削除しました。");
        }
    }
}