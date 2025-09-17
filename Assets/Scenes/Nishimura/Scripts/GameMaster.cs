using UnityEngine;
using TMPro;
using WebGLSupport;
using Unity.Netcode;
using Unity.Collections;
using System;
using System.Collections.Generic;

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
    [SerializeField] private TextMeshPro dialText; // 子のTextMeshProを割り当て
    [SerializeField] private TextMeshPro SkeletonText1;
    [SerializeField] private TextMeshPro SkeletonText2;

    // private変数
    private string[] answers = new string[4];
    private string[] words;   // 読み込んだ単語リスト

    // 共有変数
    public NetworkVariable<FixedString128Bytes> sharedAnagram = new NetworkVariable<FixedString128Bytes>(
        new FixedString128Bytes(""),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public NetworkVariable<FixedString128Bytes> sharedDial = new NetworkVariable<FixedString128Bytes>(
        new FixedString128Bytes(""),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    public NetworkList<FixedString32Bytes> solvedList;
    // 単語リスト
    public NetworkList<FixedString128Bytes> wordList;
    // 盤面（行ごとの文字列）
    public NetworkList<FixedString128Bytes> gridRows;

    void Awake()
    {
        Instance = this;

        solvedList = new NetworkList<FixedString32Bytes>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server
        );
        wordList = new NetworkList<FixedString128Bytes>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server
        );
        gridRows = new NetworkList<FixedString128Bytes>(
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
        sharedDial.OnValueChanged += (oldVal, newVal) =>
        {
            if (dialText != null)
                dialText.text = sharedDial.Value.ToString();
        };

        // 参加時にも最新値を反映
        if (anagramText != null)
            anagramText.text = sharedAnagram.Value.ToString();
        if (dialText != null)
            dialText.text = sharedDial.Value.ToString();

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
        wordList.OnListChanged += (changeEvent) =>
        {
            string text = "【リスト】\n\n";
            // SkeletonText1.Textに、改行区切りで書き写す
            for (int i = 0; i < wordList.Count; i++)
            {
                text += wordList[i].ToString();
                if (i < wordList.Count - 1) text += "\n";
            }
            SkeletonText1.text = text;
        };
        gridRows.OnListChanged += (changeEvent) =>
        {
            string text = "";
            // SkeletonText1.Textに、改行区切りで書き写す
            for (int i = 0; i < gridRows.Count; i++)
            {
                text += gridRows[i].ToString();
                if (i < gridRows.Count - 1) text += "\n";
            }
            SkeletonText2.text = text;
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
            for (int i = 1; i <= 3; i++)
            {
                CreateNewQuestion(i);
            }
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
        subCameras[forcusMonitorID - 1].enabled = true;

        // Canvasを表示する
        canvasObject.SetActive(true);
    }

    // backボタンが押されたとき
    public void OnBackButtonClicked()
    {
        // カメラを元に戻す
        subCameras[forcusMonitorID - 1].enabled = false;
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
        else if (monitorID == 2)
        {
            CreateDial();
        }
        else if (monitorID == 3)
        {
            CreateSkeleton();
        }
        solvedList[monitorID - 1] = new FixedString32Bytes("0");
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

        timers[timerID - 1].ResetTimer();
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
        if (userAnswer == answers[MonitorID - 1])
        {
            Debug.Log("正解です！（サーバー判定）");
            solvedList[MonitorID - 1] = new FixedString32Bytes("1");
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
        string answer = words[UnityEngine.Random.Range(0, words.Length)].Trim();
        if (string.IsNullOrEmpty(answer) || answer.Length <= 3) { CreateAnagram(); return; }

        // 文字をシャッフル
        char[] chars = answer.ToCharArray();

        while (new string(chars) == answer) // 元の文字列と同じ場合は再シャッフル
        {
            for (int i = 0; i < chars.Length; i++)
            {
                int rnd = UnityEngine.Random.Range(i, chars.Length);
                (chars[i], chars[rnd]) = (chars[rnd], chars[i]);
            }
        }
        sharedAnagram.Value = new FixedString128Bytes(new string(chars));
        answers[0] = answer;
    }

    char[] hiraganaList = new char[]
    {
        'あ','い','う','え','お',
        'か','き','く','け','こ',
        'さ','し','す','せ','そ',
        'た','ち','つ','て','と',
        'な','に','ぬ','ね','の',
        'は','ひ','ふ','へ','ほ',
        'ま','み','む','め','も',
        'や','ゆ','よ',
        'ら','り','る','れ','ろ',
        'わ','を','ん',
        'が','ぎ','ぐ','げ','ご',
        'ざ','じ','ず','ぜ','ぞ',
        'だ','ぢ','づ','で','ど',
        'ば','び','ぶ','べ','ぼ',
        'ぱ','ぴ','ぷ','ぺ','ぽ',
        'ゃ','ゅ','ょ',
        'ぁ','ぃ','ぅ','ぇ','ぉ'
    };

    char ShiftHiragana(char c)
    {
        if (c == 'あ') return 'い';
        if (c == 'ん') return 'を';
        if (c == 'が') return 'ぎ';
        if (c == 'ど') return 'で';
        if (c == 'ば') return 'び';
        if (c == 'ぼ') return 'べ';
        if (c == 'ぷ') return 'ぴ';
        if (c == 'ぽ') return 'ぺ';
        if (c == 'ゃ') return 'ゅ';
        if (c == 'ょ') return 'ゅ';
        if (c == 'ぁ') return 'ぃ';
        if (c == 'ぉ') return 'ぇ';
        int index = System.Array.IndexOf(hiraganaList, c);
        if (index < 0) return c; // リストにない文字はそのまま

        // 前か後ろにずらす
        int offset = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
        return hiraganaList[index + offset];
    }

    void CreateDial()
    {
        if (words == null || words.Length == 0) return;

        // ランダムに1つ選ぶ
        string answer;
        do
        {
            answer = words[UnityEngine.Random.Range(0, words.Length)].Trim();
        } while (answer.Contains('ー') || answer.Contains('っ'));
        if (string.IsNullOrEmpty(answer) || answer.Length <= 3) { CreateDial(); return; }

        // 各文字を前後にずらす（濁点はそのまま）
        char[] chars = answer.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = ShiftHiragana(chars[i]);
        }

        sharedDial.Value = new FixedString128Bytes(new string(chars));
        answers[1] = answer;
    }

    void CreateSkeleton()
    {
        // --- 初期化 ---
        List<string> wlist = new List<string>();
        char[,] grid = new char[6, 6];
        bool[,] rawBlock = new bool[6, 6];
        bool[,] colBlock = new bool[6, 6];

        int[] dr = { -1, 1, 0, 0 }; // ↑↓
        int[] dc = { 0, 0, -1, 1 }; // ←→
        for (int r = 0; r < 6; r++)
        {
            for (int c = 0; c < 6; c++)
            {
                grid[r, c] = '　';   // 全角スペース
                rawBlock[r, c] = false;
                colBlock[r, c] = false;
            }
        }

        System.Random rnd = new System.Random();
        int tryCount = 0;

        // --- 単語を順次配置 ---
        while (tryCount < 200 && wlist.Count < 6) // 200回まで試行
        {
            int Query = 1000;
            var pairs = new List<Tuple<string, int, int, bool, int>>();
            while (Query-- > 0 || pairs.Count <= 0)
            {
                string word = words[rnd.Next(words.Length)].Trim();
                if (word.Length < 2 || word.Length > 6) { tryCount++; continue; }

                bool horizontal = rnd.Next(2) == 0; // true=横
                int maxRow = horizontal ? 6 : 6 - word.Length + 1;
                int maxCol = horizontal ? 6 - word.Length + 1 : 6;
                int row = rnd.Next(maxRow);
                int col = rnd.Next(maxCol);
                int overlapCount = 0;
                if (wlist.Count != 0)
                {
                    // 衝突判定
                    bool canPlace = true;
                    for (int i = 0; i < word.Length; i++)
                    {
                        int r = row + (horizontal ? 0 : i);
                        int c = col + (horizontal ? i : 0);
                        char g = grid[r, c];
                        if (g != '　' && g != word[i] || horizontal && rawBlock[r, c] || !horizontal && colBlock[r, c]) { canPlace = false; break; }
                        if (g == word[i]) overlapCount++;
                        if (i == 0)
                        {
                            if (horizontal && c > 0 && grid[r, c - 1] != '　') { canPlace = false; break; }
                            ;
                            if (!horizontal && r > 0 && grid[r - 1, c] != '　') { canPlace = false; break; }
                            ;
                        }
                        else if (i + 1 == word.Length)
                        {
                            if (horizontal && c + 1 < 6 && grid[r, c + 1] != '　') { canPlace = false; break; }
                            ;
                            if (!horizontal && r + 1 < 6 && grid[r + 1, c] != '　') { canPlace = false; break; }
                            ;
                        }
                    }

                    if (!canPlace) { tryCount++; continue; }
                }
                pairs.Add(Tuple.Create(word, row, col, horizontal, overlapCount));
                
            }
            pairs.Sort((a, b) => {
                int cmp = b.Item5.CompareTo(a.Item5);
                return cmp != 0 ? cmp : b.Item1.Length.CompareTo(a.Item1.Length);
            });

            if (pairs.Count <= 0) continue;
            var top = pairs[0];
            string word0 = top.Item1;
            int row0 = top.Item2;
            int col0 = top.Item3;
            bool horizontal0 = top.Item4;
            int ocount = top.Item5;
            
            // 配置
            for (int i = 0; i < word0.Length; i++)
            {
                int r = row0 + (horizontal0 ? 0 : i);
                int c = col0 + (horizontal0 ? i : 0);
                grid[r, c] = word0[i];

                for (int dir = 0; dir < 4; dir++)
                {
                    int nr = r + dr[dir];
                    int nc = c + dc[dir];

                    // 範囲外チェック
                    if (nr < 0 || nr >= 6 || nc < 0 || nc >= 6) continue;

                    if (horizontal0) rawBlock[nr, nc] = true;
                    else colBlock[nr, nc] = true;
                }
                if (i == 0)
                {
                    if (horizontal0 && c > 0) colBlock[r, c - 1] = true;
                    if (!horizontal0 && r > 0) rawBlock[r-1, c] = true;
                } 
                else if (i + 1 == word0.Length)
                {

                    if (horizontal0 && c + 1 < 6) colBlock[r, c + 1] = true;
                    if (!horizontal0 && r + 1 < 6) rawBlock[r + 1, c] = true;
                }
            }
            wlist.Add(word0);
            tryCount = 0;   
        }

        // --- 文字プールから答えを作る ---
        List<char> pool = new List<char>();
        foreach (var w in wlist)
            foreach (var c in w)
                pool.Add(c);

        List<string> candidates = new List<string>();
        foreach (var word in words)
        {
            List<char> tempPool = new List<char>(pool);
            bool canMake = true;
            foreach (var c in word)
            {
                if (tempPool.Contains(c))
                    tempPool.Remove(c);
                else
                {
                    canMake = false;
                    break;
                }
            }
            if (canMake && word.Length >= 2 && word.Length <= 6) // 条件付き
                candidates.Add(word);
        }

        if (candidates.Count > 0)
        {
            string answer = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            answers[2] = answer; // Skeleton用の答え
            Debug.Log("Skeletonの答え: " + answer);
        }
        else
        {
            Debug.LogWarning("文字プールから作れる単語がありません");
        }

        // --- NetworkList へ反映 ---
        wordList.Clear();
        gridRows.Clear();
        // 単語リスト追加
        foreach (var w in wlist)
            wordList.Add(new FixedString128Bytes(w));

        // 盤面初期化
        for (int r = 0; r < 6; r++)
        {
            char[] line = new char[6];
            for (int c = 0; c < 6; c++)
            {
                line[c] = (grid[r, c] == '　') ? '　' : '◯';
                for (int d = 0; d < answers[2].Length; d++)
                {
                    if (answers[2][d] == grid[r,c])
                    {
                        line[c] = "①②③④⑤⑥"[d];
                        break;
                    }
                }
            }
            gridRows.Add(new FixedString128Bytes(new string(line)));
        }
        
    }
}