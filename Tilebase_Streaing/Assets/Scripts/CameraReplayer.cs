using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraReplayer : MonoBehaviour
{
    public TextAsset cameraLogCSV;
    public Camera targetCamera;
    public float frameInterval = 1f / 30f;

    private List<(Vector3 pos, Vector3 dir)> cameraLogList = new List<(Vector3, Vector3)>();
    private int currentIndex = 0;

    void Start()
    {
        LoadCameraLog();
        StartCoroutine(ReplayLoop());
    }

    void LoadCameraLog()
    {
        if (cameraLogCSV == null)
        {
            Debug.LogError("cameraLogCSV が設定されていません");
            return;
        }

        var lines = cameraLogCSV.text.Split('\n');
        for (int i = 1; i < lines.Length; i++) // ヘッダーを除外
        {
            var parts = lines[i].Split(',');
            if (parts.Length < 8) continue;

            float px = float.Parse(parts[1]);
            float py = float.Parse(parts[2]);
            float pz = float.Parse(parts[3]);
            float rx = float.Parse(parts[4]);
            float ry = float.Parse(parts[5]);
            float rz = float.Parse(parts[6]);

            Vector3 pos = new Vector3(px, py, pz);
            Vector3 dir = Quaternion.Euler(rx, ry, rz) * Vector3.forward;
            cameraLogList.Add((pos, dir));
        }
    }

    IEnumerator ReplayLoop()
    {
        while (currentIndex < cameraLogList.Count)
        {
            var (pos, dir) = cameraLogList[currentIndex];

            targetCamera.transform.position = pos;
            targetCamera.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

            currentIndex++;
            yield return new WaitForSecondsRealtime(frameInterval);
        }

        Debug.Log("カメラ再生が完了しました");
    }
}
