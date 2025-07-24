using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.IO;

public static class XmlTileLoader
{
    public struct TileBounds
    {
        public int index; // 一意なタイル番号
        public Bounds bounds;
    }

    public static List<TileBounds> LoadTileBounds(int frame)
    {
        string filePath = Path.Combine(Application.dataPath, "XML", $"{frame:000}.xml");
        if (!File.Exists(filePath))
        {
            Debug.LogError($"XMLファイルが見つかりません: {filePath}");
            return null;
        }

        List<TileBounds> tileBoundsList = new List<TileBounds>();

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(filePath);

        XmlNodeList tileNodes = xmlDoc.SelectNodes("//Tile");
        foreach (XmlNode tileNode in tileNodes)
        {
            int x = int.Parse(tileNode.Attributes["x"].Value);
            int y = int.Parse(tileNode.Attributes["y"].Value);
            int z = int.Parse(tileNode.Attributes["z"].Value);

            XmlNode minNode = tileNode.SelectSingleNode("Min");
            XmlNode maxNode = tileNode.SelectSingleNode("Max");

            Vector3 min = new Vector3(
                float.Parse(minNode.Attributes["x"].Value),
                float.Parse(minNode.Attributes["y"].Value),
                float.Parse(minNode.Attributes["z"].Value)
            );

            Vector3 max = new Vector3(
                float.Parse(maxNode.Attributes["x"].Value),
                float.Parse(maxNode.Attributes["y"].Value),
                float.Parse(maxNode.Attributes["z"].Value)
            );

            Bounds bounds = new Bounds();
            bounds.SetMinMax(min, max);

            int index = x + y * Download.gridX + z * Download.gridX * Download.gridY;

            tileBoundsList.Add(new TileBounds { index = index, bounds = bounds });
        }

        return tileBoundsList;
    }
}
