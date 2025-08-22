using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class TileRequestLogger
{
    // 必要に応じて手動で有効/無効化（Download.logEnabledと併用可）
    public static bool Enabled = true;

    // CSVのファイルパス（Assets/Log/requests_{timestamp}.csv）
    private static string _csvPath = null;
    private static bool _headerWritten = false;

    /// <summary>
    /// 最初に一度だけ呼べばOK（呼ばなくても最初のLogで自動初期化されます）
    /// </summary>
    public static void Begin(string sessionTag = null)
    {
        if (_csvPath != null) return;

        var logDir = Path.Combine(Application.dataPath, "Log");
        if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);

        // セッション識別子を付けて上書き衝突を避ける
        var tag = string.IsNullOrEmpty(sessionTag)
            ? DateTime.Now.ToString("yyyyMMdd_HHmmss")
            : sessionTag;

        _csvPath = Path.Combine(logDir, $"requests_{tag}.csv");
        _headerWritten = false;
    }

    /// <summary>
    /// 1フレーム分の「フレーム番号・タイル集合」をCSVへ追記
    /// 形式: Frame,TileCount,Tiles
    /// Tiles列は "0,6,1,7,..." のように引用符で囲って出力（カンマのエスケープ対策）
    /// </summary>
    public static void LogTiles(int frame, List<int> tileIndices)
    {
        if (!Enabled) return;

        if (_csvPath == null) Begin();

        try
        {
            using (var sw = new StreamWriter(_csvPath, append: true, Encoding.UTF8))
            {
                if (!_headerWritten || new FileInfo(_csvPath).Length == 0)
                {
                    sw.WriteLine("Frame,TileCount,Tiles");
                    _headerWritten = true;
                }

                string joined = (tileIndices != null && tileIndices.Count > 0)
                    ? string.Join(",", tileIndices)
                    : "";

                // Tiles列はダブルクォートで囲む（カンマをセル内データとして扱うため）
                sw.WriteLine($"{frame},{tileIndices?.Count ?? 0},\"{joined}\"");
            }
        }
        catch (Exception e)
        {
            Debug.unityLogger.LogError("[TileRequestLogger]", $"CSV書き込みに失敗: {_csvPath}\n{e}");
        }
    }
}
