using System.Collections.Generic;
using UnityEngine;

public static class TileSelector
{
    /// <summary>
    /// 視点と視線方向に基づいて、交差するタイルインデックスを返す
    /// </summary>
    public static List<int> GetVisibleTiles(Vector3 origin, Vector3 direction)
    {
        List<int> visibleTiles = new List<int>();

        // Download.cs 側のパラメータを使用
        int gridX = Download.gridX;
        int gridY = Download.gridY;
        int gridZ = Download.gridZ;

        Vector3 globalMin = Download.globalMin;
        Vector3 globalMax = Download.globalMax;

        Vector3 tileSize = new Vector3(
            (globalMax.x - globalMin.x) / gridX,
            (globalMax.y - globalMin.y) / gridY,
            (globalMax.z - globalMin.z) / gridZ
        );

        int index = 0;
        for (int xi = 0; xi < gridX; xi++)
        {
            for (int yi = 0; yi < gridY; yi++)
            {
                for (int zi = 0; zi < gridZ; zi++)
                {
                    Vector3 min = new Vector3(
                        globalMin.x + xi * tileSize.x,
                        globalMin.y + yi * tileSize.y,
                        globalMin.z + zi * tileSize.z
                    );

                    Vector3 max = min + tileSize;

                    Bounds bounds = new Bounds();
                    bounds.SetMinMax(min, max);

                    Ray ray = new Ray(origin, direction.normalized);
                    if (bounds.IntersectRay(ray))
                    {
                        visibleTiles.Add(index);
                    }

                    index++;
                }
            }
        }

        return visibleTiles;
    }

    public static List<int> GetVisibleTilesFromXML(int frame, Vector3 origin, Vector3 direction)
    {
        List<int> visible = new List<int>();
        var tiles = XmlTileLoader.LoadTileBounds(frame);

        if (tiles == null) return visible;

        Ray ray = new Ray(origin, direction.normalized);

        foreach (var tile in tiles)
        {
            if (tile.bounds.IntersectRay(ray))
            {
                visible.Add(tile.index);
            }
        }

        return visible;
    }
}
