using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float high = 1.5f;
    [SerializeField] private float distance = 0;
    [SerializeField] private Quaternion verticalRotation;
    [SerializeField] public Quaternion horizontalRotation;
    [SerializeField] private float mouseSensitivity = 100f;
    private float xRotation = 0f;
    private float yRotation = 0f;

    public Transform Player
    {
        get => player;
        set
        {
            player = value;
            Initialize();
        }
    }

    void Start()
    {
        if (player != null)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        verticalRotation = Quaternion.Euler(0, 0, 0);
        horizontalRotation = Quaternion.identity;

        //Cursor.lockState = CursorLockMode.Locked;
        UpdateRotationPosition();
    }


    void LateUpdate()
    {
        if (player == null) return;

        UpdateRotationPosition();
        // マウスの移動量を取得
        //float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        //float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Y軸の回転量を計算（左右の動き）
        //yRotation += mouseX;

        // X軸の回転量を計算（上下の動き）
        //xRotation -= mouseY;
        // X軸の回転範囲を-90度から90度に制限
        //xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // カメラの上下回転を適用
        //transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    
        // カメラの左右回転を適用
        //transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    private void UpdateRotationPosition()
    {
        var transform1 = transform;
        transform1.rotation = player.rotation * Quaternion.Euler(xRotation, yRotation, 0f);
        transform1.position = player.position + new Vector3(0, high, 0) - transform1.rotation * Vector3.forward * distance;
    }
}