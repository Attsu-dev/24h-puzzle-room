using UnityEngine;

public class ClickMonitor : MonoBehaviour
{
    public Camera mainCamera;   // �؂�ւ��O�̃J����
    public Camera subCamera;    // �؂�ւ���̃J����
    private bool isMainActive = true;

    void OnMouseDown()
    {
        subCamera.enabled = true;
        Debug.Log($"{name} ���N���b�N�B�J������؂�ւ��܂���");
    }
}
