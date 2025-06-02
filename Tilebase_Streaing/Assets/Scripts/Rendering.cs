using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;

namespace Pcx
{
    public class Rendering : MonoBehaviour
    {
        public int tileID; // このRendererが担当する点群ID（数値型）
        public MeshFilter meshFilter;
        private const float renderInterval = 0.033f; // 30fps相当

        void Start()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 120;
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

            StartCoroutine(RenderLoop());
        }

        IEnumerator RenderLoop()
        {
            float nextRenderTime = Time.realtimeSinceStartup;
            while (true)
            {
                if (Time.realtimeSinceStartup >= nextRenderTime)
                {
                    if (Download.renderQueues[tileID].TryDequeue(out var item))
                    {
                        (byte[] data, int frameIndex) = item;
                        yield return StartCoroutine(RenderFile(data, frameIndex));
                    }
                    nextRenderTime += renderInterval;
                }
                yield return new WaitForSeconds(renderInterval);
            }
        }

        IEnumerator RenderFile(byte[] data, int frameIndex)
        {
            Debug.Log($"[Rendering Start] Tile: {tileID}, Frame: {frameIndex}, Time: {Time.time}");
            var mesh = ImportMeshFromData(data, frameIndex);
            meshFilter.sharedMesh = mesh;
            Debug.Log($"[Rendering Complete] Tile: {tileID}, Frame: {frameIndex}, Time: {Time.time}");
            yield break;
        }

        private Mesh ImportMeshFromData(byte[] data, int index)
        {
            var stream = new MemoryStream(data);
            var reader = new StreamReader(stream);
            var binReader = new BinaryReader(stream);
            
            Mesh mesh = new Mesh { name = index.ToString() };
            return mesh;
        }
    }
}
