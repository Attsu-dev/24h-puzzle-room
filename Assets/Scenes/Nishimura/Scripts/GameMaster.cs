using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public static GameMaster Instance { get; private set; }
    public Camera mainCamera;
    public Camera subCamera;
    public int forcusMonitorIndex = -1; // -1はどのモニターもフォーカスしていない状態

    private string answer = "あんぱんまん";

    public GameObject canvasObject;


    void Awake()
    {
        Instance = this;
    }

    // プレイヤーがモニターをクリックしたとき
    public void OnMonitorClicked(GameObject clickedMonitor)
    {
        Debug.Log($"Cube {clickedMonitor.name} がクリックされました");

        // カメラを切り替える
        mainCamera.enabled = false;
        subCamera.enabled = true;

        // クリックされたモニターにフォーカスする
        forcusMonitorIndex = 0; // ここでは仮に0を設定。実際にはclickedMonitorに基づいて設定する必要があります。

        // Canvasを表示する
        canvasObject.SetActive(true);
    }

    // backボタンが押されたとき
    public void OnBackButtonClicked()
    {
        // カメラを元に戻す
        mainCamera.enabled = true;
        subCamera.enabled = false;
        // フォーカスを解除する
        forcusMonitorIndex = -1;
        // Canvasを非表示にする
        canvasObject.SetActive(false);
    }

    public void OnSubmitAnswer(string userAnswer)
    {
        if (userAnswer == answer)
        {
            Debug.Log("正解です！");
            // 正解時の処理をここに追加
        }
        else
        {
            Debug.Log("不正解です。もう一度試してください。");
            // 不正解時の処理をここに追加
        }
    }
}
