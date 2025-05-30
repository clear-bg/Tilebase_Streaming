using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;


public class Downloader : MonoBehaviour
{
    // public static Queue<int> renderQueue = new Queue<int>(); // レンダリング対象のキュー
    public static ConcurrentQueue<(byte[], int)> renderQueue = new ConcurrentQueue<(byte[], int)>();
    public int initialBufferSize = 30; // 初期バッファサイズ
    public int totalFrames = 300; // 総フレーム数
    private int downloadIndex = 0; // ダウンロード進行インデックス
    private bool initialBufferFilled = false;

    public delegate void BufferReady(); // 初期バッファ完了時の通知
    public static event BufferReady OnBufferReady;

    private List<float> downloadTimes = new List<float>(); // 各ダウンロード時間を格納するリスト
    private string csvFilePath="C:/Users/bandailab/Documents/データ/ダウンロード時間/2台体制/色あり/DownloadTimes_400mbit_20250428.csv";

    void Start()
    {
        StartCoroutine(CallTcStart());
        StartCoroutine(DownloadLoop());
    }

    IEnumerator DownloadLoop()
    {
        while (downloadIndex < totalFrames)
        {
            
            // string url = $"http://192.168.10.2:8000/loot_10_nc/{downloadIndex}.ply";  // 2台目
            
            string url = $"http://localhost:8000/loot_10_nc/{downloadIndex}.ply"; 
            // string url = $"http://localhost:8000/loot_20/{downloadIndex}.ply";  
            // string url = $"http://localhost:8000/longdress/longdress_vox10_{1050 + downloadIndex}.ply"; 
            string path = GetFilePath(downloadIndex);

            UnityWebRequest uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            // uwr.downloadHandler = new DownloadHandlerFile(path, true); // ローカルに直接保存
            // uwr.downloadHandler = new DownloadHandler(path, true); // ローカルに直接保存
            
            uwr.downloadHandler = new DownloadHandlerBuffer();

            float startTime = Time.time;
            Debug.Log($"[Download Start] File: {downloadIndex}, Time: {startTime}");
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                float endTime = Time.time;
                Debug.Log($"[Download Complete] File: {downloadIndex}, Time: {endTime}");
                float downloadTime = endTime - startTime;
                downloadTimes.Add(downloadTime);

                byte[] data = uwr.downloadHandler.data;
                Debug.Log(data);
                // Debug.Log($"[lock Start] File: {downloadIndex}, Time: {Time.time}")
                // lock (renderQueue) // キューの操作を同期化
                // {
                    // renderQueue.Enqueue(downloadIndex);
                renderQueue.Enqueue((data, downloadIndex));
                // }
            }
            else
            {
                Debug.LogError($"[Download Error] File: {downloadIndex}, Error: {uwr.error}");
            }

            downloadIndex++;

            // 初期バッファが充填されたら通知
            if (!initialBufferFilled && renderQueue.Count >= initialBufferSize)
            {
                initialBufferFilled = true;
                OnBufferReady?.Invoke(); // 初期バッファ完了を通知
                Debug.Log("初期バッファが充填されました。レンダリングを開始してください。");
            }
        }
        // SaveDownloadTimesToCSV();
    }
    private IEnumerator CallTcStart() //バックグラウンドタスクで帯域制限したい
    {
        string apiUrl = "http://192.168.10.2:8000/tc_start"; // FastAPIサーバーのエンドポイント

        UnityWebRequest uwr = UnityWebRequest.Get(apiUrl);
        Debug.Log("tc_startを呼び出します...");
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[tc_start 成功] レスポンス: {uwr.downloadHandler.text}");
        }
        else
        {
            Debug.LogError($"[tc_start エラー] Error: {uwr.error}");
        }
    }

    private string GetFilePath(int fileNumber)
    {
        return Path.Combine(Application.dataPath, "Download", $"{fileNumber}.ply");
    }

    private void SaveDownloadTimesToCSV()
    {
        try
        {
            using (StreamWriter sw = new StreamWriter(csvFilePath))
            {
                sw.WriteLine("Frame,DownloadTime(s)"); // ヘッダー行を追加

                float totalTime = 0;
                for (int i = 0; i < downloadTimes.Count; i++)
                {
                    sw.WriteLine($"{i},{downloadTimes[i]}");
                    totalTime += downloadTimes[i];
                }

                float averageTime = totalTime / downloadTimes.Count;
                sw.WriteLine($"\nAverage,{averageTime}"); // 平均ダウンロード時間を追加

                Debug.Log($"[CSV Saved] {csvFilePath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CSV Save Error] {e.Message}");
        }
    }

}
