using UnityEngine;

public class ClickMonitor : MonoBehaviour
{
    public Camera mainCamera;   // 切り替え前のカメラ
    public Camera subCamera;    // 切り替え後のカメラ
    private bool isMainActive = true;

    void OnMouseDown()
    {
        subCamera.enabled = true;
        Debug.Log($"{name} をクリック。カメラを切り替えました");
    }
}
