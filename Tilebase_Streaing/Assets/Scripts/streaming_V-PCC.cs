using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Globalization;
using System.Diagnostics;

namespace Pcx
{
    public class DownloadManager : MonoBehaviour
    {
        // レンダリング更新時に通知するイベント
        public static event Action OnRenderUpdated;

        int frameNumber = 0; // 初期化設定, Update関数内で受け取り、自動的に増える
        int endframeNumber = 30; // 再生するplyファイルの数
        string encoder = "jm"; // 小文字で指定(hm, jm，NVDec)
        int rate = 5; // 品質設定(R1~R5で指定)
        int frameCount = 1; // エンコード時の連続フレーム数(今は1で固定でしか実行できない)
        bool flag_bin = true; // binファイルを削除するかのフラグ
        bool flag_ASCII = true; // ASCIIファイルを削除するかのフラグ
        // bool flag_BinaryPly = true; // バイナリの点群ファイルを削除するかのフラグ

        string assetsPath = Application.dataPath; // Unityプロジェクト内のAssetsフォルダのパスを取得
        
        PlyImporter importer = new PlyImporter();
        MeshFilter meshFilter;
        MeshRenderer meshRenderer;

        int dl_idx = 0;
        int pl_idx = 0;

        void Start()
        {
            // ゲームオブジェクトにMeshFilter, MeshRendererを追加、マテリアルの設定
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = importer.GetDefaultMaterial(); 

            // テキストファイルの初期化
            InitializeTextFile();           
        }

        void Update()
        {
            if (frameNumber < endframeNumber)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start(); // 処理開始時間の計測開始

                if (dl_idx < endframeNumber && dl_idx <= pl_idx + 1){
                    DownloadBinFile(dl_idx);
                }

                if (pl_idx < dl_idx){
                    VPCCDecorder(pl_idx);
                    StartCoroutine(RenderingPly(pl_idx));
                    pl_idx++;
                }
                // DownloadBinFile(frameNumber); // binファイルダウンロード
                // VPCCDecorder(frameNumber);  // バッチファイル実行
                // StartCoroutine(RenderingPly(frameNumber));

                // レンダリング更新を通知
                OnRenderUpdated?.Invoke();

                stopwatch.Stop(); // 処理終了時間を記録

                // // 描画時間を秒単位でログ出力
                // float renderTimeInSeconds = stopwatch.ElapsedMilliseconds / 1000f;
                // UnityEngine.Debug.Log($"{frameNumber}をレンダリング. 描画時間: {renderTimeInSeconds:F3} s");

                // // テキストファイルに追記
                // AppendToTextFile(renderTimeInSeconds);

                frameNumber += 1;
                dl_idx++;
            }
        }

            // if (dl_idx - pl_idx > 1){
            //     StartCoroutine(RenderingPly(pl_idx));
            //     UnityEngine.Debug.Log($"{pl_idx}をレンダリング");
            //     pl_idx += 1;
            // } else {
            //     // UnityEngine.Debug.Log("初期バッファ待ち");
            // }
        

        void DownloadBinFile(int frameNumber)
        {
            // string server_url = $"http://172.16.51.65:5000/{encoder}/{frameCount}frame_r{rate}/";
            string server_url = $"http://172.16.51.12:8080/{encoder}/{frameCount}frame_r{rate}/";
            string server_binFileName = $"8i_vox10_loot_frame_{frameNumber}_to_{frameNumber}_r{rate}_{encoder}.bin";
            string url = server_url + server_binFileName;

            string Download_binFilepath = $"{assetsPath}/bin_Download/{encoder}_r{rate}_frame{frameNumber}_to_{frameNumber}.bin";

            // ダウンロード処理
            UnityWebRequest uwr = UnityWebRequest.Get(url);
            uwr.downloadHandler = new DownloadHandlerFile(Download_binFilepath, true);
            var operation = uwr.SendWebRequest(); // ダウンロード実行
        }

        void VPCCDecorder(int frameNumber)
        {
            string batchFilePath = string.Empty; // 初期化
            if (encoder == "hm")
            {
                batchFilePath = @"C:\Users\clear\mpeg-pcc-tmc2\decode_HMAPP.bat"; // アプリケーション呼び出しだから使えない
            }
            else if (encoder == "jm")
            {
                batchFilePath = @"C:\Users\clear\mpeg-pcc-tmc2\decode_JMAPP.bat";
            }
            else if (encoder == "NVDec")
            {
                batchFilePath = @"C:\Users\clear\mpeg-pcc-tmc2\decode_NVDec.bat";
            }

            string binFilePath = $"{assetsPath}/bin_Download/{encoder}_r{rate}_frame{frameNumber}_to_{frameNumber}.bin"; // 入力binファイルのパス
            string plyFilePath = $"{assetsPath}/Binary_Ply/binary_{encoder}_r{rate}_%04d.ply"; // 出力plyファイルのパス


            string arguments = $"{frameNumber:D4} \"{binFilePath}\" \"{plyFilePath}\"";

            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = batchFilePath,
                Arguments = arguments,
                CreateNoWindow = true, // コマンドプロンプトを表示しない
                WindowStyle = ProcessWindowStyle.Hidden, // 非表示で実行
                UseShellExecute = false, // シェルを使わずに実行
            };

            using (Process process = new Process { StartInfo = processInfo })
            {
                process.Start();
                process.WaitForExit();
            }

            // デコーダー実行後にテキストファイルなどを削除
            string basePath = $"{assetsPath}/bin_Download/{encoder}_r{rate}_frame{frameNumber}_to_{frameNumber}_dec_";
            string[] deleteTargets = new string[] {
                "bitstream_md5.txt",
                "hls_md5.txt",
                "atlas_log.txt",
                "pcframe_log.txt",
                "picture_log.txt",
                "rec_pcframe_log.txt",
                "tile_log.txt"
            };

            foreach (string target in deleteTargets)
            {
                string deletePath = basePath + target;
                if (File.Exists(deletePath)){
                    File.Delete(deletePath);
                    // UnityEngine.Debug.Log("ファイルを削除しました : " + deletePath);
                } else {
                    // UnityEngine.Debug.LogWarning("削除対象のファイルが存在しません : " + deletePath);
                }
            }

            // flag_binがtrueの場合、.binファイルを削除
            if (flag_bin && File.Exists(binFilePath))
            {
                try
                {
                    File.Delete(binFilePath);
                    UnityEngine.Debug.Log("BINファイルを削除しました : " + binFilePath);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError("BINファイルの削除に失敗しました : " + ex.Message);
                }
            }
        }


        IEnumerator RenderingPly(int frameNumber)
        {
            string PlyPath = $"{assetsPath}/Binary_Ply/Binary_{encoder}_r{rate}_{frameNumber:D4}.ply";

            // Create MeshFilter/MeshRenderer
            var mesh = importer.ImportAsMesh(PlyPath);

            meshFilter.sharedMesh = mesh;

            UnityEngine.Debug.Log($"{frameNumber}.plyを描画");

            // コルーチンの終了 : ファイルの読み込みが完了すると即座に終了
            yield return null;

            // // 削除対象は描画したファイルではなく2つ前のファイル
            // int previousFrame = frameNumber - 5;
            // if (previousFrame >= 0) // 前のフレームが存在する場合のみ削除
            // {
            //     string previousPlyPath = $"{assetsPath}/Binary_Ply/Binary_{encoder}_r{rate}_{previousFrame:D4}.ply";

            //     if (flag_BinaryPly && File.Exists(previousPlyPath))
            //     {
            //         try
            //         {
            //             File.Delete(previousPlyPath);
            //             UnityEngine.Debug.Log("バイナリPlyファイルを削除しました : " + previousPlyPath);
            //         }
            //         catch (Exception ex)
            //         {
            //             UnityEngine.Debug.LogError("バイナリPlyファイルの削除に失敗しました : " + ex.Message);
            //         }
            //     }
            // }
        }


        void ConvertASCIItoBinary(int frameNumber) // 使わない
        {
            string inputPath = $"{assetsPath}/ASCII_Ply/ASCII_{frameNumber:D4}.ply"; // ASCII形式の点群ファイルパス
            string outputPath = $"{assetsPath}/Binary_Ply/Binary_{encoder}_r{rate}_{frameNumber:D4}.ply"; // 出力するバイナリ形式の点群ファイルパス

            using (StreamReader reader = new StreamReader(inputPath))
            using (BinaryWriter writer = new BinaryWriter(File.Open(outputPath, FileMode.Create)))
            {
                string line;
                bool headerEnded = false;
                int vertexCount = 0;

                // ヘッダーの書き込み（エンコーディングと改行コードを明示的に指定）
                writer.Write(System.Text.Encoding.ASCII.GetBytes("ply\n"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes("format binary_little_endian 1.0\n"));

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("element vertex"))
                    {
                        vertexCount = int.Parse(line.Split(' ')[2]);
                        writer.Write(System.Text.Encoding.ASCII.GetBytes($"element vertex {vertexCount}\n"));
                    }
                    else if (line.StartsWith("property") && !line.Contains("list uint8 int32 vertex_index"))
                    {
                        // "property list uint8 int32 vertex_index" 以外のプロパティ行のみ書き込み
                        writer.Write(System.Text.Encoding.ASCII.GetBytes(line + "\n"));
                    }
                    else if (line.StartsWith("end_header"))
                    {
                        writer.Write(System.Text.Encoding.ASCII.GetBytes("end_header\n"));
                        headerEnded = true;
                        break;
                    }
                }

                // 頂点データの変換と書き込み
                if (headerEnded)
                {
                    for (int i = 0; i < vertexCount; i++)
                    {
                        line = reader.ReadLine();
                        string[] values = line.Split(' ');

                        // 頂点のx, y, z座標をfloat型で書き込み
                        writer.Write(float.Parse(values[0], CultureInfo.InvariantCulture));
                        writer.Write(float.Parse(values[1], CultureInfo.InvariantCulture));
                        writer.Write(float.Parse(values[2], CultureInfo.InvariantCulture));

                        // 色のr, g, b値をuchar型で書き込み
                        writer.Write(Convert.ToByte(values[3]));
                        writer.Write(Convert.ToByte(values[4]));
                        writer.Write(Convert.ToByte(values[5]));
                    }
                }
            }

            // バイナリ変換終了後にASCIIのファイル削除
            if (flag_ASCII && File.Exists(inputPath))
            {
                File.Delete(inputPath);
                UnityEngine.Debug.Log("ASCIIファイルを削除しました : " + inputPath);
            }
            else if (!File.Exists(inputPath))
            {
                UnityEngine.Debug.LogWarning("指定されたASCIIファイルが存在しません : " + inputPath);
            }
        }

        string textFilePath;
        void InitializeTextFile()
        {
            textFilePath = $"C:\\Users\\clear\\github\\graduation_thesis_template\\thesis\\figures\\excel_file\\{encoder}_r{rate}_frames_{endframeNumber}.txt";
            // テキストファイルを新規作成し、空にする
            if (File.Exists(textFilePath))
            {
                File.Delete(textFilePath); // 既存ファイルがあれば削除
            }
            using (StreamWriter writer = new StreamWriter(textFilePath, false))
            {
                writer.WriteLine($"Rendering Times ({encoder}, Quality: R{rate}, Frames: {endframeNumber}, Unit: s)"); // ヘッダーを記載
            }
        }

        void AppendToTextFile(float renderTimeInSeconds)
        {
            // テキストファイルに描画時間を追記
            using (StreamWriter writer = new StreamWriter(textFilePath, true))
            {
                writer.WriteLine(renderTimeInSeconds.ToString("F3")); // 秒単位で小数第2位まで
            }
        }
    }
}
