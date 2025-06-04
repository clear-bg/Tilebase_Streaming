using System.Collections;
using UnityEngine;

namespace Pcx
{
    public class Rendering : MonoBehaviour
    {
        public int tileID; // この描画オブジェクトが担当するタイルID
        public MeshFilter meshFilter;

        void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();

            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();

            meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
        }

        IEnumerator Start()
        {
            while (true)
            {
                if (Download.renderQueues[tileID].TryDequeue(out var item))
                {
                    (byte[] data, int frameIndex) = item;

                    // ✅ PointCloudImporter を使用
                    var mesh = PointCloudImporter.ImportAsMesh(data, frameIndex);
                    if (mesh != null)
                    {
                        meshFilter.sharedMesh = mesh;
                        Debug.Log($"[Renderer {tileID}] Frame {frameIndex} rendered.");
                    }
                    else
                    {
                        Debug.LogWarning($"[Renderer {tileID}] Failed to parse frame {frameIndex}");
                    }
                }

                yield return new WaitForSeconds(0.033f); // 約30fps
            }
        }
    }
}
