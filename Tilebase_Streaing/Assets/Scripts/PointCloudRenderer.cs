using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pcx
{
    public class PointCloudRenderer : MonoBehaviour
    {
        public int cloudId; // このRendererの点群ID
        public MeshFilter meshFilter;
        PlyImporter importer = new PlyImporter();

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();

            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
            
            meshRenderer.sharedMaterial = importer.GetDefaultMaterial();
        }


        IEnumerator Start()
        {
            while (true)
            {
                if (Downloader.renderQueues[cloudId].TryDequeue(out var item))
                {
                    (byte[] data, int index) = item;
                    var mesh = importer.ImportAsMesh(data, index);
                    meshFilter.sharedMesh = mesh;
                    Debug.Log($"[Renderer {cloudId}] Frame {index} rendered.");
                }
                yield return new WaitForSeconds(0.033f); // 30fps相当
            }
        }
    }
    // Start is called before the first frame update
    // void Start()
    // {

    // }

    // Update is called once per frame
    // void Update()
    // {

    // }
}