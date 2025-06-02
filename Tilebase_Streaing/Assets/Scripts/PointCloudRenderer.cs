using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Pcx
{
    public class PointCloudRenderer : MonoBehaviour
    {
        public int tileID; // このRendererの点群ID
        public MeshFilter meshFilter;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();

            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();

            meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
        }

        IEnumerator Start()
        {
            while (true)
            {
                if (Download.renderQueues[tileID].TryDequeue(out var item))
                {
                    (byte[] data, int index) = item;
                    var mesh = ImportMeshFromData(data, index); // 修正
                    meshFilter.sharedMesh = mesh;
                    Debug.Log($"[Renderer {tileID}] Frame {index} rendered.");
                }
                yield return new WaitForSeconds(0.033f); // 30fps相当
            }
        }

        private Mesh ImportMeshFromData(byte[] data, int index)
        {
            var stream = new MemoryStream(data);
            var reader = new StreamReader(stream);
            var binReader = new BinaryReader(stream);
            
            // 簡略版の例として空のMeshを作成
            Mesh mesh = new Mesh { name = index.ToString() };
            // 実際にはここで点群データを読み込み、mesh.SetVerticesやSetColorsを設定します
            return mesh;
        }
    }
}
