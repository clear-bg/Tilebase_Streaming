using System.Diagnostics;
using UnityEngine;

public class TestFrustumPerf : MonoBehaviour
{
    public int testFrame = 0;   // 測りたいフレーム番号(XMLが必要)
    public int iterations = 1000; // ループ回数（平均をとる）

    void Start()
    {
        // 視点を仮定（Main Camera の位置・回転を利用）
        var cam = Camera.main;
        Vector3 origin = cam.transform.position;
        Quaternion rotation = cam.transform.rotation;

        // 計測開始
        Stopwatch sw = new Stopwatch();
        sw.Start();

        for (int i = 0; i < iterations; i++)
        {
            var visible = TileSelector.GetVisibleTilesFromXML(testFrame, origin, rotation);
        }

        sw.Stop();
        double ms = sw.Elapsed.TotalMilliseconds;
        UnityEngine.Debug.Log(
            $"[PerfTest] Frame={testFrame} Iterations={iterations} " +
            $"Total={ms:F2} ms, Avg={ms/iterations:F4} ms/iter"
        );
    }
}
