using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Pcx
{
  public class Rendering : MonoBehaviour
  {
    public MeshFilter meshFilter; // レンダリング対象の MeshFilter
    PlyImporter importer = new PlyImporter();
    MeshRenderer meshRenderer;
    private bool canRender = false; // レンダリング可能フラグ

    private const float renderInterval = 0.01f; // レンダリング間隔 (秒)
    void Start()
    {
      QualitySettings.vSyncCount = 0; // VSyncを無効化
      Application.targetFrameRate = 120; // 最大フレームレートを120FPSに設定
      meshFilter = gameObject.AddComponent<MeshFilter>();
      meshRenderer = gameObject.AddComponent<MeshRenderer>();
      meshRenderer.sharedMaterial = importer.GetDefaultMaterial();

      // 初期バッファ充填完了を受け取る
      Download.OnBufferReady += EnableRendering;
      StartCoroutine(RenderLoop());
    }

    void EnableRendering()
    {
      canRender = true;
    }

    IEnumerator RenderLoop()
    {
      float nextRenderTime = Time.realtimeSinceStartup;
      while (true)
      {
        if (!canRender)
        {
          yield return null; // 初期バッファが満たされるまで待機
          continue;
        }

        if (!Download.renderQueue.TryDequeue(out var item))
        {
          yield break; // キューが空なら何もせず抜ける
        }

        (byte[] data, int downloadIndex) = item;

        if (downloadIndex < 0)
        {
          yield break; // 無効なインデックスはスキップ
        }

        yield return StartCoroutine(RenderFile(data, downloadIndex)); // レンダリング処理
        nextRenderTime += renderInterval;

        yield return new WaitForSeconds(renderInterval); // レンダリング間隔を設定
      }
    }

    IEnumerator RenderFile(byte[] data, int downloadIndex)
    {
      if (false) {
        yield break;
      }

      Debug.Log($"[Rendering Start] File: {downloadIndex}, Time: {Time.time}");
      var mesh = importer.ImportAsMesh(data, downloadIndex); // ファイルをメッシュに変換            
      meshFilter.sharedMesh = mesh;           
      Debug.Log($"[Rendering Complete] File: {downloadIndex}, Time: {Time.time}");

    }
  }
}