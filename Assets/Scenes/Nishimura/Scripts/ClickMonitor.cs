using UnityEngine;

public class ClickMonitor : MonoBehaviour
{
    void OnMouseDown()   // �}�E�X�N���b�N���Ɏ����ŌĂ΂��
    {
        if (GameMaster.Instance != null && GameMaster.Instance.forcusMonitorIndex == -1)
        {
            GameMaster.Instance.OnMonitorClicked(gameObject);
        }
    }
}
