using UnityEngine;
using TMPro;
using WebGLSupport;
using Unity.Netcode;
using Unity.Collections;

public class GameMaster : NetworkBehaviour
{
    // これがあることで、他のスクリプトからGameMasterにアクセスできる（シングルトン）
    public static GameMaster Instance { get; private set; }

    // public変数
    public int forcusMonitorID = 0; // 0はどのモニターにもフォーカスしていない状態

    // GameMasterで制御するオブジェクト
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera[] subCameras;
    [SerializeField] private GameObject canvasObject;
    [SerializeField] private TMP_InputField answerInputField;
    [SerializeField] private CountdownTimer[] timers;
    [SerializeField] private GameObject[] solvedPanels;
    [SerializeField] private GameObject[] puzzlePanels;
    [SerializeField] private TextAsset japaneseWordList; // 単語リスト(txt)をInspectorで割り当て
    [SerializeField] private TextMeshPro anagramText; // 子のTextMeshProを割り当て

    // private変数
    private string[] answers = new string[4];
    private string[] words;   // 読み込んだ単語リスト

    // 共有変数
    public NetworkVariable<FixedString128Bytes> sharedAnagram = new NetworkVariable<FixedString128Bytes>(
        new FixedString128Bytes(""),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public NetworkList<FixedString32Bytes> solvedList;

    void Awake()
    {
        Instance = this;

        solvedList = new NetworkList<FixedString32Bytes>(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );
    }
    


    public override void OnNetworkSpawn()
    {
        // 変化があったらTextMeshProに反映
        sharedAnagram.OnValueChanged += (oldVal, newVal) =>
        {
            if (anagramText != null)
                anagramText.text = sharedAnagram.Value.ToString();
        };

        // 参加時にも最新値を反映
        if (anagramText != null)
            anagramText.text = sharedAnagram.Value.ToString();

        if (IsServer)
        {
            // 初期値としてモニター数分追加
            for (int i = 0; i < 4; i++)
                solvedList.Add(new FixedString32Bytes("0"));
        }

        // 変化を監視
        solvedList.OnListChanged += (changeEvent) =>
        {
            Debug.Log($"Monitor solved state changed: {changeEvent.Index} = {solvedList[changeEvent.Index]}");
            puzzlePanels[changeEvent.Index].SetActive(solvedList[changeEvent.Index].ToString() == "0");
            solvedPanels[changeEvent.Index].SetActive(solvedList[changeEvent.Index].ToString() == "1");
        };

        if (IsServer)
        {
            Debug.Log("サーバー登場!");
            // サーバーのみが行う処理
            // テキストファイルを行ごとに読み込む
            if (japaneseWordList != null)
            {
                words = japaneseWordList.text
                    .Replace("\r", "")   // 改行コードを整理
                    .Split('\n');
            }
            CreateNewQuestion(1);
        }
    }

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
        subCameras[forcusMonitorID-1].enabled = true;

        // Canvasを表示する
        canvasObject.SetActive(true);
    }

    // backボタンが押されたとき
    public void OnBackButtonClicked()
    {
        // カメラを元に戻す
        subCameras[forcusMonitorID-1].enabled = false;
        // フォーカスを解除する
        forcusMonitorID = 0;
        // Canvasを非表示にする
        canvasObject.SetActive(false);
        // 入力フィールドをクリアする
        answerInputField.text = "";
    }

    // 新たに問題を作成する
    public void CreateNewQuestion(int monitorID)
    {
        // ここで新しい問題と答えを設定する
        if (monitorID == 1)
        {
            CreateAnagram();
        }
        solvedList[monitorID-1] = new FixedString32Bytes("0");
    }

    public void OnSubmitAnswer(string userAnswer)
    {
        // 入力フィールドをクリアする
        answerInputField.text = "";
        // クライアント側で呼ぶ
        if (!IsServer)
        {
            SubmitAnswerServerRpc(userAnswer, forcusMonitorID);  // サーバーへ送信
            return;
        }

        CheckAnswer(userAnswer, forcusMonitorID);  // サーバーなら直接判定
    }

    public void OnTimerFinished(int timerID)
    {
        if (!IsServer) return;
        
        timers[timerID-1].ResetTimer();
        CreateNewQuestion(timerID);
    }
    // ---- 追加 ----
    [ServerRpc(RequireOwnership = false)]
    private void SubmitAnswerServerRpc(string userAnswer, int MonitorID)
    {
        CheckAnswer(userAnswer, MonitorID);
    }

    // ---- 共通判定処理 ----
    private void CheckAnswer(string userAnswer, int MonitorID)
    {
        if (userAnswer == answers[MonitorID-1])
        {
            Debug.Log("正解です！（サーバー判定）");
            solvedList[0] = new FixedString32Bytes("1");
        }
        else
        {
            Debug.Log($"不正解（サーバー判定）: {userAnswer} / 正解: {answers}");
        }
    }

    // アナグラムを作成する
    void CreateAnagram()
    {
        if (words == null || words.Length == 0) return;

        // ランダムに1つ選ぶ
        string answer = words[Random.Range(0, words.Length)].Trim();
        if (string.IsNullOrEmpty(answer) || answer.Length <= 2) { CreateAnagram(); return; }

        // 文字をシャッフル
        char[] chars = answer.ToCharArray();

        while (new string(chars) == answer) // 元の文字列と同じ場合は再シャッフル
        {
            for (int i = 0; i < chars.Length; i++)
            {
                int rnd = Random.Range(i, chars.Length);
                (chars[i], chars[rnd]) = (chars[rnd], chars[i]);
            }
        }
        sharedAnagram.Value = new FixedString128Bytes(new string(chars));
        answers[0] = answer;
    }

}
