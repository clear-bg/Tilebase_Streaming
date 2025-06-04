using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pcx
{
    public static class PointCloudImporter
    {
        public static Mesh ImportAsMesh(byte[] data, int frameIndex)
        {
            try
            {
                using var stream = new MemoryStream(data);
                using var sr = new StreamReader(stream);
                var header = ReadHeader(sr);
                var reader = new BinaryReader(stream);

                // 頂点データ読み取り
                var vertices = new List<Vector3>(header.vertexCount);
                var colors = new List<Color32>(header.vertexCount);

                for (int i = 0; i < header.vertexCount; i++)
                {
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    float z = reader.ReadSingle();

                    byte r = reader.ReadByte();
                    byte g = reader.ReadByte();
                    byte b = reader.ReadByte();

                    vertices.Add(new Vector3(x, y, z));
                    colors.Add(new Color32(r, g, b, 255));
                }

                var mesh = new Mesh { name = $"frame_{frameIndex}" };
                mesh.indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
                mesh.SetVertices(vertices);
                mesh.SetColors(colors);
                mesh.SetIndices(Enumerable.Range(0, vertices.Count).ToArray(), MeshTopology.Points, 0);
                mesh.UploadMeshData(true);

                return mesh;
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlyImporter] Failed to import mesh: {e.Message}");
                return null;
            }
        }

        private static int[] CreateSequentialIndices(int count)
        {
            var indices = new int[count];
            for (int i = 0; i < count; i++) indices[i] = i;
            return indices;
        }

        private struct PlyHeader
        {
            public int vertexCount;
            public bool isBinaryLittleEndian;
        }

        private static PlyHeader ReadHeader(StreamReader sr)
        {
            var header = new PlyHeader { vertexCount = 0, isBinaryLittleEndian = false };
            var lines = new List<string>();
            string line;

            while ((line = sr.ReadLine()) != null)
            {
                lines.Add(line);
                if (line.StartsWith("format binary_little_endian"))
                    header.isBinaryLittleEndian = true;
                else if (line.StartsWith("element vertex"))
                    header.vertexCount = int.Parse(line.Split()[2]);
                else if (line.StartsWith("end_header"))
                    break;
            }

            sr.BaseStream.Position = lines.Sum(l => l.Length + 1);
            return header;
        }
    }
}
