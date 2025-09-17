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
    [SerializeField] private Camera[] subCameras;
    [SerializeField] private GameObject canvasObject;
    [SerializeField] private Puzzle anagram;
    [SerializeField] private TMP_InputField answerInputField;
    [SerializeField] private CountdownTimer timer1;
    [SerializeField] private GameObject solvedPanel;
    [SerializeField] private GameObject PuzzlePanel;

    // private変数
    private bool solved = false;
    private string answer = "";

    int MonitorNameToID(string monitorName)
    {
        switch (monitorName)
        {
            case "Monitor_A": return 1;
            case "Monitor_B": return 2;
            case "Monitor_C": return 3;
            case "Monitor_D": return 4;
            default:
                Debug.LogWarning($"未対応のモニター名: {monitorName}");
                return -1; // エラー
        }
    }

    // プレイヤーがモニターをクリックしたとき
    public void OnMonitorClicked(GameObject clickedMonitor)
    {
        Debug.Log($"Cube {clickedMonitor.name} がクリックされました");
        forcusMonitorID = MonitorNameToID(clickedMonitor.name);

        // カメラを切り替える
        //mainCamera.enabled = false;
        subCameras[forcusMonitorID-1].enabled = true;

        // Canvasを表示する
        canvasObject.SetActive(true);

        CreateNewQuestion();
    }

    // backボタンが押されたとき
    public void OnBackButtonClicked()
    {
        // カメラを元に戻す
        //mainCamera.enabled = true;
        subCameras[forcusMonitorID-1].enabled = false;
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
        solvedPanel.SetActive(false);
        PuzzlePanel.SetActive(true);
    }

    public void OnSubmitAnswer(string userAnswer)
    {
        if (userAnswer == answer)
        {
            Debug.Log("正解です！");
            // 正解時の処理をここに追加
            solved = true;
            PuzzlePanel.SetActive(false);
            solvedPanel.SetActive(true);
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
