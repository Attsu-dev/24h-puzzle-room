using UnityEngine;
using UnityEngine.UI;

public class ClickMonitor : MonoBehaviour
{
    public Camera mainCamera;   // �؂�ւ��O�̃J����
    public Camera subCamera;    // �؂�ւ���̃J����
    private bool isMainActive = true;

    public InputField inputField;

    void OnMouseDown()
    {
        subCamera.enabled = true;
        Debug.Log($"{name} ���N���b�N�B�J������؂�ւ��܂���");
    }

    public void OnValueChanged(string text)
    {
        Debug.Log(text);
    }
}
