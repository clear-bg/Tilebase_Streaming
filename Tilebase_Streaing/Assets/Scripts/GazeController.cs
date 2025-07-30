using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GazeController : MonoBehaviour
{
    [Tooltip("Camera Log CSVファイル（Assets/ 以下の Resources に配置推奨）")]
    public TextAsset cameraLogCSV;

    private List<Vector3> positions = new List<Vector3>();
    private List<Vector3> rotations = new List<Vector3>();
    private int currentFrame = 0;

    public Vector3 CurrentPosition => positions.Count > currentFrame ? positions[currentFrame] : Vector3.zero;
    public Vector3 CurrentForward => positions.Count > currentFrame
        ? Quaternion.Euler(rotations[currentFrame]) * Vector3.forward
        : Vector3.forward;

    public Quaternion CurrentRotation => positions.Count > currentFrame
        ? Quaternion.Euler(rotations[currentFrame])
        : Quaternion.identity;
    void Awake()
    {
        LoadCSV();
    }

    void LoadCSV()
    {
        if (cameraLogCSV == null)
        {
            Debug.LogError("CSVファイルが割り当てられていません");
            return;
        }

        string[] lines = cameraLogCSV.text.Split('\n');
        for (int i = 1; i < lines.Length; i++) // 1行目はヘッダー
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] tokens = line.Split(',');
            if (tokens.Length < 8) continue;

            float px = float.Parse(tokens[1]);
            float py = float.Parse(tokens[2]);
            float pz = float.Parse(tokens[3]);

            float rx = float.Parse(tokens[4]);
            float ry = float.Parse(tokens[5]);
            float rz = float.Parse(tokens[6]);

            positions.Add(new Vector3(px, py, pz));
            rotations.Add(new Vector3(rx, ry, rz));
        }

        Debug.Log($"GazeController: {positions.Count} フレーム分のデータを読み込みました。");
    }

    public void SetFrame(int frame)
    {
        currentFrame = Mathf.Clamp(frame, 0, positions.Count - 1);
    }
}
