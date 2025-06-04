using UnityEngine;

public class CubeControl : MonoBehaviour
{
    private Renderer cubeRenderer;
    private bool isWhite = true;

    void Start()
    {
        // CubeのRendererを取得
        cubeRenderer = GetComponent<Renderer>();

        // 初期色を白に設定
        cubeRenderer.material.color = Color.white;

        // DownloadManagerのイベントを購読
        Pcx.DownloadManager.OnRenderUpdated += ToggleColor;
    }

    void OnDestroy()
    {
        // イベント購読を解除（メモリリーク防止）
        Pcx.DownloadManager.OnRenderUpdated -= ToggleColor;
    }

    private void ToggleColor()
    {
        // 現在の状態に応じて色を切り替え
        isWhite = !isWhite;
        cubeRenderer.material.color = isWhite ? Color.white : Color.red;
    }
}
