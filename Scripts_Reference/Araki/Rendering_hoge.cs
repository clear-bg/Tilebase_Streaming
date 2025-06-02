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
        ConcurrentQueue<int> queue = new ConcurrentQueue<int>();
        string[,] urlArray;
        string url;

        int dl_times = 1;
        int i = 0;

        int num_of_frames = 300;

        int played_idx = 0;
        int downloaded_idx = 0;
        int stalling = 0;

        List<List<object>> re_list = new List<List<object>>();
        List<int> stall_list = new List<int>();

        float dl_time = 0;

        bool processing = true;
        bool wasCanceled = false;
        bool initial_buffer = true;
        bool stalling_frag = true;

        double bandwidth = 0;
        double est_bandwidth = 10000000000000;
        double bandwidth_weight = 0.8;

        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        CancellationTokenSource cts;

        MeshFilter meshFilter;
        MeshFilter meshFilter2;
        MeshFilter meshFilter3;
        MeshFilter meshFilter4;
        MeshFilter meshFilter5;
        MeshFilter meshFilter6;
        MeshFilter meshFilter7;
        MeshFilter meshFilter8;
        MeshRenderer meshRenderer;
        MeshRenderer meshRenderer2;
        MeshRenderer meshRenderer3;
        MeshRenderer meshRenderer4;
        MeshRenderer meshRenderer5;
        MeshRenderer meshRenderer6;
        MeshRenderer meshRenderer7;
        MeshRenderer meshRenderer8;
        PlyImporter importer = new PlyImporter();

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private void Awake()
        {

            cts = new CancellationTokenSource();

            GameObject pc2 = GameObject.Find("PointCloud2");
            meshFilter2 = pc2.AddComponent<MeshFilter>();
            meshRenderer2 = pc2.AddComponent<MeshRenderer>();
            meshRenderer2.sharedMaterial = importer.GetDefaultMaterial();

            GameObject pc3 = GameObject.Find("PointCloud3");
            meshFilter3 = pc3.AddComponent<MeshFilter>();
            meshRenderer3 = pc3.AddComponent<MeshRenderer>();
            meshRenderer3.sharedMaterial = importer.GetDefaultMaterial();

            GameObject pc4 = GameObject.Find("PointCloud4");
            meshFilter4 = pc4.AddComponent<MeshFilter>();
            meshRenderer4 = pc4.AddComponent<MeshRenderer>();
            meshRenderer4.sharedMaterial = importer.GetDefaultMaterial();

            GameObject pc5 = GameObject.Find("PointCloud5");
            meshFilter5 = pc5.AddComponent<MeshFilter>();
            meshRenderer5 = pc5.AddComponent<MeshRenderer>();
            meshRenderer5.sharedMaterial = importer.GetDefaultMaterial();

            GameObject pc6 = GameObject.Find("PointCloud6");
            meshFilter6 = pc6.AddComponent<MeshFilter>();
            meshRenderer6 = pc6.AddComponent<MeshRenderer>();
            meshRenderer6.sharedMaterial = importer.GetDefaultMaterial();

            GameObject pc7 = GameObject.Find("PointCloud7");
            meshFilter7 = pc7.AddComponent<MeshFilter>();
            meshRenderer7 = pc7.AddComponent<MeshRenderer>();
            meshRenderer7.sharedMaterial = importer.GetDefaultMaterial();

            GameObject pc8 = GameObject.Find("PointCloud8");
            meshFilter8 = pc8.AddComponent<MeshFilter>();
            meshRenderer8 = pc8.AddComponent<MeshRenderer>();
            meshRenderer8.sharedMaterial = importer.GetDefaultMaterial();


            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = importer.GetDefaultMaterial();

            QualitySettings.vSyncCount = 0;
            //Application.targetFrameRate = 24;
            Time.fixedDeltaTime = 1/10f;
            //Debug.Log("Hello Unity!!");
        }

        void Start()
        {
            stopwatch.Start();
            InvokeRepeating("RenderingTask", 2f, 1/10f);
            //StartCoroutine(DownloadRoutine().ToCoroutine());

        }

        // Update is called once per frame
        void Update()
        {
            //DownloadTask();            
        }

        async void FixedUpdate()
        {
            
        }

        async void RenderingTask()
        {
            //Debug.Log($"FIXED UPDATE: {DateTime.Now}:{DateTime.Now.Millisecond}");
            //stopwatch.Stop();
            //re_list.Add(new List<object> { stopwatch.ElapsedMilliseconds, played_idx });
            //stopwatch.Start();
            /*
            if (initial_buffer)
            {
                await Task.Delay(4000);
                initial_buffer = false;
            }
            */


            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            if (played_idx < 500)
            {
                //if (downloaded_idx - played_idx >= 1)
                //{
                sw.Reset();
                sw.Start();
                stalling_frag = true;
                
                
                await UniTask.WhenAll(
                    RenderPly(meshFilter, "Base", played_idx, cancellationTokenSource.Token),
                    RenderPly(meshFilter2, "Enhancement1", played_idx, cancellationTokenSource.Token),
                    RenderPly(meshFilter3, "Enhancement2", played_idx, cancellationTokenSource.Token),
                    RenderPly(meshFilter4, "Enhancement3", played_idx, cancellationTokenSource.Token)
                    //RenderPly(meshFilter5, "Enhancement4", played_idx, cancellationTokenSource.Token),
                    //RenderPly(meshFilter6, "Enhancement5", played_idx, cancellationTokenSource.Token),
                    //RenderPly(meshFilter7, "Enhancement6", played_idx, cancellationTokenSource.Token),
                    //RenderPly(meshFilter8, "Enhancement7", played_idx, cancellationTokenSource.Token)
                    );
                

                //RenderPly(meshFilter, "Base", played_idx);
                if (stalling_frag)
                {
                    Debug.Log($"ストーリング発生 : {played_idx}番目のフレーム　{DateTime.Now}の時");
                    stall_list.Add(played_idx);
                    stalling++;
                }
                //else
                //{
                //    played_idx++;
                //}
                played_idx++;


                //sw.Stop();
                //TimeSpan rendertime = sw.Elapsed;
                //Debug.Log($"Rendering Time: {rendertime.Milliseconds}s");
                //}
            }
            else
            {

            };
        }

        async UniTask RenderPly(MeshFilter meshFilter, string quality, int number, CancellationToken token)
        // 点群の描画処理
        {
            string PATH = "Assets/Download/Ply/" + quality + "/" + number.ToString() + ".ply";
            //string PATH = "Assets/Download/Ply/VoxelGrid_1/" + number.ToString() + ".ply";
            //string PATH = "Assets/Download/Ply/VoxelGrid_2/" + number.ToString() + ".ply";
            meshFilter.sharedMesh = null;
            try
            {
                var mesh = importer.ImportAsMesh(PATH);
                
                meshFilter.sharedMesh = mesh;

                stalling_frag = false;

                //Debug.Log("Open file path :" + PATH);
                //File.Delete(PATH);
            }
            catch(FileNotFoundException e)
            {
                //Debug.Log(e.Message);
            }
            catch(DirectoryNotFoundException e)
            {
                //Debug.Log(e.Message);
            }
            catch(EndOfStreamException e)
            {
                //Debug.Log(e.Message);
            }
            catch (System.Exception e)
            {
                //Debug.Log(PATH + "が見つかりません");
                Debug.LogError(e.Message);
            }
            finally
            {
                if (number == 0)
                {

                }
            }
        }

        private void SaveLogToFile()
        {
            string logFilePath = "Assets/Logs/sample.csv"; // ログファイルのパス
            FileInfo fi = new FileInfo(logFilePath);
            // ログファイルが存在しない場合は作成し、すでに存在する場合は上書きする
            StreamWriter writer = new StreamWriter(logFilePath);

            foreach (int row in stall_list)
            {
                writer.WriteLine(row);
            }

            writer.Flush();
            writer.Close();
        }


        void OnApplicationQuit()
        // 実行をやめるときの処理
        {
            cts?.Cancel();
            cancellationTokenSource?.Cancel();
            Debug.Log($"ストーリング回数：{stalling}");
            Debug.Log(string.Join(", ", stall_list));
            SaveLogToFile();
            //Directory.Delete("Assets/Download/", true);
            
            
        }
    }

}

