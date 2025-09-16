using UnityEngine;

public class ClickMonitor : MonoBehaviour
{
    void OnMouseDown()   // マウスクリック時に自動で呼ばれる
    {
        if (GameMaster.Instance != null && GameMaster.Instance.forcusMonitorIndex == -1)
        {
            GameMaster.Instance.OnMonitorClicked(gameObject);
        }
    }
}
