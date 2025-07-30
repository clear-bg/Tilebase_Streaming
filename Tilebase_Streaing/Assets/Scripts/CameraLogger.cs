using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CameraLogger : MonoBehaviour
{
    public Camera targetCamera;  // 記録対象のカメラ（Main Cameraを指定）
    public int logFrameRate = 30;

    private List<string> logLines = new List<string>();
    private int frameNumber = 0;

    // Download.cs などから代入する用（現在描画中の点群フレーム番号）
    public int currentViewFrame = 0;

    private float logInterval => 1f / logFrameRate;
    private Coroutine loggingCoroutine;

    void Start()
    {
        logLines.Add("FrameNumber,HMDPX,HMDPY,HMDPZ,HMDRX,HMDRY,HMDRZ,ViewFrame");

        loggingCoroutine = StartCoroutine(LogLoop());
        Debug.Log("Camera logging started.");
    }

    void OnApplicationQuit()
    {
        if (loggingCoroutine != null)
        {
            StopCoroutine(loggingCoroutine);
            if (Download.logEnabled) SaveCSV();
            Debug.Log("Camera logging stopped and saved.");
        }
    }

    IEnumerator LogLoop()
    {
        while (true)
        {
            Vector3 pos = targetCamera.transform.position;
            Vector3 rot = targetCamera.transform.eulerAngles;

            string line = string.Format(
                "{0},{1:F4},{2:F4},{3:F4},{4:F4},{5:F4},{6:F4},{7}",
                frameNumber,
                pos.x, pos.y, pos.z,
                rot.x, rot.y, rot.z,
                currentViewFrame
            );

            logLines.Add(line);
            frameNumber++;

            yield return new WaitForSeconds(logInterval);
        }
    }

    void SaveCSV()
    {
        string logDir = Path.Combine(Application.dataPath, "Log");
        if (!Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        string filePath = Path.Combine(logDir, "camera_log.csv");
        File.WriteAllLines(filePath, logLines);
        Debug.Log("Camera log saved to: " + filePath);
    }
}
