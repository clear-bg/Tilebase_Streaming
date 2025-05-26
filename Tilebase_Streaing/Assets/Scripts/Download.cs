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
        ConcurrentQueue<int> queue = new ConcurrentQueue<int>();
        string[,] urlArray;
        string url;

        int played_idx = 0;
        int downloaded_idx = 0;
        int stalling = 0;
        int layer = 4;

        double est_bandwidth = 0;
        double new_bandwidth = 0;

        ulong base_size = 0;
        double safety_factor = 1.2;

        ulong dl_size = 0;
        long dl_time = 0;

        List<List<object>> dl_list = new List<List<object>>();


        bool processing = true;
        bool wasCanceled = false;
        bool initial_buffer = true;
        bool stalling_frag = true;


        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        CancellationTokenSource cts;


        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private void Awake()
        {

            cts = new CancellationTokenSource();

            QualitySettings.vSyncCount = 0;
            //Application.targetFrameRate = 24;
            Time.fixedDeltaTime = 1/10f;
            //Debug.Log("Hello Unity!!");
        }

        void Start()
        {
            stopwatch.Start();
            InvokeRepeating("StartDownloadTask", 1f, 1/10f);
            //InvokeRepeating("VgfDownload", 1f, 1 / 10f);
            //StartCoroutine(DownloadRoutine().ToCoroutine());

        }

        async UniTask StartDownloadTask()
        {
            //Debug.Log($"DOWNLOADTASK: {DateTime.Now}:{DateTime.Now.Millisecond}");
            stopwatch.Stop();
            dl_list.Add(new List<object> { stopwatch.ElapsedMilliseconds, downloaded_idx });
            stopwatch.Start();
            //Debug.Log("Start DownloadTask !");
            if (cancellationTokenSource != null)
            {
                // 前回のタスクをキャンセル
                cancellationTokenSource.Cancel();
                //await UniTask.DelayFrame(1);
            }

            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            if (downloaded_idx < 500)
            {
                DownloadLayers(token, downloaded_idx).Forget();
                downloaded_idx++;
            }
            

        }

        async UniTask DownloadLayers(CancellationToken token, int idx)
        {
            //Debug.Log("##########################################################");
            //Debug.Log($"Start LayerDownlaod Task {downloaded_idx} Frame !");
            try
            {
                int num = ABR();
                //Debug.Log(num);
                dl_size = 0;
                dl_time = 0;
                for(int i=0; i < num; i++)
                {
                    await SerialDownload(idx, i, token);
                }

                //downloaded_idx++;
                //await SerialDownload(0, token);
                //await VgfDownload(0, token);
            }
            catch (OperationCanceledException)
            {
                //Debug.Log("終わんなかったからキャンセルしたよ");
            }
            finally
            {
                //downloaded_idx++;
                //dl_idx++;
            }
        }

        async UniTask SerialDownload(int idx, int layer_num, CancellationToken ct)
        {
            var sw = new System.Diagnostics.Stopwatch();
            int download_num = idx;
            
            string path = "Assets/Download/Ply/";
            if (layer_num == 0)
            {
                path = path + "Base/";
                if(download_num != 0)
                {
                    //download_num++;
                }
                
            }
            else
            {
                path = path + "Enhancement" + layer_num + "/";
            }
            //url = "http://127.0.0.1:8000/get_file/" + layer_num + "/";
            //url = "http://172.16.51.23:8000/get_file/" + layer_num + "/";
            //url = "http://172.16.51.1:8000/get_file/" + layer_num + "/";
            url = "http://172.16.51.1:8000/get_file/" + layer_num + "/" + download_num;

            //Debug.Log(Time.time);
            //Debug.Log($"Start serial download for frame {download_num} layer {layer_num} !");
            sw.Restart();
            using(UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                //Debug.Log(url);
                uwr.downloadHandler = new DownloadHandlerFile(path + download_num + ".ply", false);
                //uwr.downloadHandler = new DownloadHandlerBuffer();
                try
                {
                    stopwatch.Restart();
                    await uwr.SendWebRequest().ToUniTask(cancellationToken: ct);
                    stopwatch.Stop();
                    if(uwr.result != UnityWebRequest.Result.Success)
                    {
                        //Debug.Log(uwr.error);
                    }
                    else
                    {
                        dl_size += uwr.downloadedBytes;
                        if(layer_num == 0)
                        {
                            base_size = uwr.downloadedBytes;
                        }
                        if(download_num == 0)
                        {
                            //Debug.Log($"ダウンロード終了{DateTime.Now}");
                        }
                        //Debug.Log($"uwr.downloadedBytes : {dl_size}");
                        dl_time += stopwatch.ElapsedMilliseconds;
                        //Debug.Log($"dl_time : {dl_time}");
                    }
                }
                catch (OperationCanceledException)
                {
                    //Debug.Log($"Download for frame {download_num} layer {layer_num} was canceled");
                    throw;
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Download error:" + e.Message);
                }
                finally
                {
                    
                }
            }
            sw.Stop();
            //Debug.Log($"ダウンロードかかった時間： {sw.ElapsedMilliseconds}ms");
            //Debug.Log($"ふぁいるあいおかかった時間： {stopwatch.Elapsed}ms");
            //await Task.Delay(1000);
        }

        async UniTask VgfDownload()
        {
            var sw = new System.Diagnostics.Stopwatch();
            int download_num = downloaded_idx;
            downloaded_idx++;
            //string path = $"Assets/Download/Ply/VoxelGrid_1/";
            //url = $"http://172.16.51.1:8000/get_vgffile/1/{download_num}";
            string path = $"Assets/Download/Ply/VoxelGrid_2/";
            url = $"http://172.16.51.1:8000/get_vgffile/2/{download_num}";

            //Debug.Log($"Start serial download for frame {download_num} layer 0 !");
            sw.Restart();
            if (download_num < 500)
            {
                using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                uwr.downloadHandler = new DownloadHandlerFile(path + download_num + ".ply", false);
                try
                {
                    //Debug.Log(url);
                    stopwatch.Restart();
                    await uwr.SendWebRequest().ToUniTask();
                    stopwatch.Stop();
                    if (uwr.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log(uwr.error);
                    }
                    else
                    {
                        //Debug.Log($"{download_num}.plyをダウンロードしたよ");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Download error:" + e.Message);
                }
                finally
                {
                        //downloaded_idx++;
                }
            }
            }
            
            sw.Stop();
            //Debug.Log($"ダウンロードかかった時間： {sw.Elapsed}ms");
            //Debug.Log($"ふぁいるあいおかかった時間： {stopwatch.Elapsed}ms");
            //await Task.Delay(1000);
        }

        // Update is called once per frame
        void Update()
        {
            //DownloadTask();            
        }

        async void FixedUpdate()
        {
           
        }

        /*
        async UniTask DownloadManager(ConcurrentQueue<int> queue)
        // 点群ファイルのダウンロードをレイヤ毎に非同期で実行するよ
        {
            ulong download_size = 0;
            List<UniTask<ulong>> download_tasks = new List<UniTask<ulong>>();

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();

            while (queue.TryDequeue(out int layer_num))
            {
                UniTask<ulong> task = Download_Ply(layer_num, cts.Token);
                download_tasks.Add(task);
            }
            ulong[] results = await UniTask.WhenAll(download_tasks);
            downloaded_idx += 1;

            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;

            foreach (var result in results)
            {
                download_size += result;
            }
            //float dl_sec = elapsedTime.Milliseconds / 1000;
            //bandwidth = download_size / dl_sec;
            //est_bandwidth = (bandwidth_weight * est_bandwidth) + ((1 - bandwidth_weight) * bandwidth);

            Debug.Log(elapsedTime.TotalSeconds);
            //stopwatch.Reset();
            //Debug.Log(dl_sec);
            //Debug.Log(download_size);
            //Debug.Log($"Download {download_size} bit in {elapsedTime.Milliseconds}s");
            //Debug.Log("All downloads completed.");
            //Debug.Log($"Measured Bandwidth:{bandwidth} Average:{est_bandwidth}");

        }

        async UniTask<ulong> Download_Ply(int layer_num, CancellationToken token)
        // 点群をリクエストする処理
        {
            float start_time;
            string path = "Assets/Download/Ply/";
            if (layer_num == 0)
            {
                path = path + "Base/";
            }
            else
            {
                path = path + "Enhancement" + layer_num + "/";
            }
            //url = urlArray[downloaded_idx, layer_num];　// MPDファイルから読むならこれ
            //url = "http://127.0.0.1:8000/get_file/" + layer_num + "/";
            //url = "http://172.16.51.32:8000/get_file/" + layer_num + "/";
            url = "http://172.16.51.109:8000/get_file/" + layer_num + "/";

            var uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);

            try
            {
                uwr.downloadHandler = new DownloadHandlerFile(path + downloaded_idx + ".ply", true);
                //Debug.Log("connecting to " + url);
                start_time = Time.time;
                await uwr.SendWebRequest().WithCancellation(cts.Token);
                dl_time = Time.time - start_time;
                if ((uwr.result == UnityWebRequest.Result.ConnectionError) || (uwr.result == UnityWebRequest.Result.ProtocolError))
                {
                    Debug.Log(uwr.error);
                    return 0;
                }
                else
                {
                    if (layer_num == 0)
                    {
                        dl_idx++;
                    }
                    return uwr.downloadedBytes;
                }
            }
            catch (System.Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        async void DownloadTask()
        {
            //Debug.Log(dl_times + "回目のダウンロードにかかった時間");

            stopwatch.Start();

            
            await SerialDownload(0, cts.Token);
            //await SerialDownload(1, cts.Token);
            //await SerialDownload(2, cts.Token);
            //await SerialDownload(3, cts.Token);


            //Task.Delay(300).Wait();
            downloaded_idx++;
            stopwatch.Stop();

            TimeSpan download_time = stopwatch.Elapsed;
            Debug.Log($"ダウンロードにかかった時間: {download_time.TotalSeconds}s");

            stopwatch.Reset();
            dl_times++;
        }
        */

        int ABR()
        {
            //Debug.Log("?????????????????????????????????????????????????????????");
            double weight;

            if(dl_time != 0)
            {
                new_bandwidth = dl_size / (ulong)dl_time * 1000;
            }
            if(est_bandwidth == 0)
            {
                weight = 1;
            }
            else
            {
                weight = 0.5;
            }
            //Debug.Log($"dl_size : {dl_size}");
            //Debug.Log($"dl_time : {dl_time}");
            //Debug.Log($"new_bandwidth : {new_bandwidth}");
            est_bandwidth = weight * new_bandwidth + (1 - weight) * est_bandwidth;
            //Debug.Log($"estimated bandwidth : {est_bandwidth}");
            //Debug.Log($"Threshold : {base_size * safety_factor}");
            /*
            if (est_bandwidth > base_size * safety_factor * 4 * 10)
            {
                print("next frame download 4 layers");
                return 4;
            }else if (est_bandwidth > base_size * safety_factor * 3 * 10)
            {
                print("next frame download 3 layers");
                return 3;
            }else if (est_bandwidth > base_size * safety_factor * 2 * 10)
            {
                print("next frame download 2 layers");
                return 2;
            }else
            {
                print("next frame download 1 layers");
                return 1;
            }
            */

            for(int i = 0; i < layer-1; i++)
            {
                if(est_bandwidth > base_size * safety_factor * (layer-i) * 10)
                {
                    print($"next frame download {layer - i} layers");
                    return layer - i;
                }
            }
            print("next frame download 1 layers");
            return 1;

        }

        private void SaveLogToFile()
        {
            string logFilePath = "Assets/Logs/dl.csv"; // ログファイルのパス
            FileInfo fi = new FileInfo(logFilePath);
            // ログファイルが存在しない場合は作成し、すでに存在する場合は上書きする
            StreamWriter writer = new StreamWriter(logFilePath);

            foreach (List<object> row in dl_list)
            {
                List<string> rowStrings = new List<string>();
                foreach (var item in row)
                {
                    rowStrings.Add(item.ToString());
                }
                writer.WriteLine(string.Join(",", rowStrings));
            }

            writer.Flush();
            writer.Close();
        }


        void OnApplicationQuit()
        // 実行をやめるときの処理
        {
            cts?.Cancel();
            cancellationTokenSource?.Cancel();
            SaveLogToFile();
            //Directory.Delete("Assets/Download/", true);
            
            
        }
    }

}

