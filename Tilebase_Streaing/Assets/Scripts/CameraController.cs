using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // WASD：前後左右の移動
    // Space：上昇、Shift：下降
    // 左ドラッグ：視点の移動（左右反転済み）

    // カメラの移動量
    [SerializeField, Range(100.0f, 600.0f)]
    private float _positionStep = 400.0f;

    // マウス感度
    [SerializeField, Range(30.0f, 300.0f)]
    private float _mouseSensitive = 75.0f;

    // カメラのTransform
    private Transform _camTransform;

    // マウスの始点
    private Vector3 _startMousePos;

    // カメラ回転の始点情報
    private Vector3 _presentCamRotation;

    void Start()
    {
        _camTransform = this.gameObject.transform;
    }

    void Update()
    {
        CameraRotationMouseControl(); // カメラの回転（マウス）
        CameraPositionKeyControl();   // カメラの移動（キー）
    }

    // カメラの回転（マウス）
    private void CameraRotationMouseControl()
    {
        // マウスがクリックされたとき
        if (Input.GetMouseButtonDown(0))
        {
            _startMousePos = Input.mousePosition;
            _presentCamRotation.x = _camTransform.eulerAngles.x;
            _presentCamRotation.y = _camTransform.eulerAngles.y;
        }

        // マウスがクリックされている間
        if (Input.GetMouseButton(0))
        {
            // 左右の変化量にマイナスをかけて反転
            float x = -(_startMousePos.x - Input.mousePosition.x) / Screen.width;
            float y = (_startMousePos.y - Input.mousePosition.y) / Screen.height;

            float eulerX = _presentCamRotation.x + y * _mouseSensitive;
            float eulerY = _presentCamRotation.y + x * _mouseSensitive;

            _camTransform.rotation = Quaternion.Euler(eulerX, eulerY, 0);
        }
    }

    // カメラの移動（キー）
    private void CameraPositionKeyControl()
    {
        Vector3 campos = _camTransform.position;

        if (Input.GetKey(KeyCode.D)) { campos += _camTransform.right * Time.deltaTime * _positionStep * 1.3f; }
        if (Input.GetKey(KeyCode.A)) { campos -= _camTransform.right * Time.deltaTime * _positionStep * 1.3f; }
        if (Input.GetKey(KeyCode.Space)) { campos += _camTransform.up * Time.deltaTime * _positionStep; }
        if (Input.GetKey(KeyCode.LeftShift)) { campos -= _camTransform.up * Time.deltaTime * _positionStep; }
        if (Input.GetKey(KeyCode.W)) { campos += _camTransform.forward * Time.deltaTime * _positionStep; }
        if (Input.GetKey(KeyCode.S)) { campos -= _camTransform.forward * Time.deltaTime * _positionStep; }

        _camTransform.position = campos;
    }
}
