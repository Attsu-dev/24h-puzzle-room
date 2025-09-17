using UnityEngine;
using TMPro;
using WebGLSupport;

public class GameMaster : MonoBehaviour
{
    // これがあることで、他のスクリプトからGameMasterにアクセスできる（シングルトン）
    public static GameMaster Instance { get; private set; }
    void Awake()
    {
        Instance = this;
    }

    // public変数
    public int forcusMonitorID = 0; // 0はどのモニターにもフォーカスしていない状態

    // GameMasterで制御するオブジェクト
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera subCamera;
    [SerializeField] private GameObject canvasObject;
    [SerializeField] private Anagram anagram;
    [SerializeField] private TMP_InputField answerInputField;
    [SerializeField] private CountdownTimer timer1;

    // private変数
    private bool solved = false;
    private string answer = "";

    // プレイヤーがモニターをクリックしたとき
    public void OnMonitorClicked(GameObject clickedMonitor)
    {
        Debug.Log($"Cube {clickedMonitor.name} がクリックされました");

        // カメラを切り替える
        mainCamera.enabled = false;
        subCamera.enabled = true;

        // クリックされたモニターにフォーカスする
        forcusMonitorID = 1; // ここでは仮に1を設定。実際にはclickedMonitorに基づいて設定する必要があります。

        // Canvasを表示する
        canvasObject.SetActive(true);

        CreateNewQuestion();
    }

    // backボタンが押されたとき
    public void OnBackButtonClicked()
    {
        // カメラを元に戻す
        mainCamera.enabled = true;
        subCamera.enabled = false;
        // フォーカスを解除する
        forcusMonitorID = 0;
        // Canvasを非表示にする
        canvasObject.SetActive(false);
        // 入力フィールドをクリアする
        answerInputField.text = "";
    }

    // 新たに問題を作成する
    public void CreateNewQuestion()
    {
        // ここで新しい問題と答えを設定する
        answer = anagram.CreateNextQuestion();
    }

    public void OnSubmitAnswer(string userAnswer)
    {
        if (userAnswer == answer)
        {
            Debug.Log("正解です！");
            // 正解時の処理をここに追加
            solved = true;
        }
        else
        {
            Debug.Log("不正解です。もう一度試してください。");
            // 不正解時の処理をここに追加
        }
        // 入力フィールドをクリアする
        answerInputField.text = "";
    }

    public void OnTimerFinished(int timerID)
    {
        if (!solved)
        {
            Debug.Log("ゲームオーバー");
        }
        CreateNewQuestion();

        solved = false;
        timer1.Reset();
    }

    
}
