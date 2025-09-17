using UnityEngine;
using TMPro;
using WebGLSupport;
using Unity.Netcode;
using Unity.Collections;
using System;
using System.Collections.Generic;

public class GameMaster : NetworkBehaviour
{
    // ���ꂪ���邱�ƂŁA���̃X�N���v�g����GameMaster�ɃA�N�Z�X�ł���i�V���O���g���j
    public static GameMaster Instance { get; private set; }

    // public�ϐ�
    public int forcusMonitorID = 0; // 0�͂ǂ̃��j�^�[�ɂ��t�H�[�J�X���Ă��Ȃ����

    // GameMaster�Ő��䂷��I�u�W�F�N�g
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera[] subCameras;
    [SerializeField] private GameObject canvasObject;
    [SerializeField] private TMP_InputField answerInputField;
    [SerializeField] private CountdownTimer[] timers;
    [SerializeField] private GameObject[] solvedPanels;
    [SerializeField] private GameObject[] puzzlePanels;
    [SerializeField] private TextAsset japaneseWordList; // �P�ꃊ�X�g(txt)��Inspector�Ŋ��蓖��
    [SerializeField] private TextMeshPro anagramText; // �q��TextMeshPro�����蓖��
    [SerializeField] private TextMeshPro dialText; // �q��TextMeshPro�����蓖��
    [SerializeField] private TextMeshPro SkeletonText1;
    [SerializeField] private TextMeshPro SkeletonText2;

    // private�ϐ�
    private string[] answers = new string[4];
    private string[] words;   // �ǂݍ��񂾒P�ꃊ�X�g

    // ���L�ϐ�
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
    // �P�ꃊ�X�g
    public NetworkList<FixedString128Bytes> wordList;
    // �Ֆʁi�s���Ƃ̕�����j
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
        // �ω�����������TextMeshPro�ɔ��f
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

        // �Q�����ɂ��ŐV�l�𔽉f
        if (anagramText != null)
            anagramText.text = sharedAnagram.Value.ToString();
        if (dialText != null)
            dialText.text = sharedDial.Value.ToString();

        if (IsServer)
        {
            // �����l�Ƃ��ă��j�^�[�����ǉ�
            for (int i = 0; i < 4; i++)
                solvedList.Add(new FixedString32Bytes("0"));
        }

        // �ω����Ď�
        solvedList.OnListChanged += (changeEvent) =>
        {
            Debug.Log($"Monitor solved state changed: {changeEvent.Index} = {solvedList[changeEvent.Index]}");
            puzzlePanels[changeEvent.Index].SetActive(solvedList[changeEvent.Index].ToString() == "0");
            solvedPanels[changeEvent.Index].SetActive(solvedList[changeEvent.Index].ToString() == "1");
        };
        wordList.OnListChanged += (changeEvent) =>
        {
            string text = "�y���X�g�z\n\n";
            // SkeletonText1.Text�ɁA���s��؂�ŏ����ʂ�
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
            // SkeletonText1.Text�ɁA���s��؂�ŏ����ʂ�
            for (int i = 0; i < gridRows.Count; i++)
            {
                text += gridRows[i].ToString();
                if (i < gridRows.Count - 1) text += "\n";
            }
            SkeletonText2.text = text;
        };

        if (IsServer)
        {
            Debug.Log("�T�[�o�[�o��!");
            // �T�[�o�[�݂̂��s������
            // �e�L�X�g�t�@�C�����s���Ƃɓǂݍ���
            if (japaneseWordList != null)
            {
                words = japaneseWordList.text
                    .Replace("\r", "")   // ���s�R�[�h�𐮗�
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
                Debug.LogWarning($"���Ή��̃��j�^�[��: {monitorName}");
                return -1; // �G���[
        }
    }

    // �v���C���[�����j�^�[���N���b�N�����Ƃ�
    public void OnMonitorClicked(GameObject clickedMonitor)
    {
        Debug.Log($"Cube {clickedMonitor.name} ���N���b�N����܂���");
        forcusMonitorID = MonitorNameToID(clickedMonitor.name);

        // �J������؂�ւ���
        subCameras[forcusMonitorID - 1].enabled = true;

        // Canvas��\������
        canvasObject.SetActive(true);
    }

    // back�{�^���������ꂽ�Ƃ�
    public void OnBackButtonClicked()
    {
        // �J���������ɖ߂�
        subCameras[forcusMonitorID - 1].enabled = false;
        // �t�H�[�J�X����������
        forcusMonitorID = 0;
        // Canvas���\���ɂ���
        canvasObject.SetActive(false);
        // ���̓t�B�[���h���N���A����
        answerInputField.text = "";
    }

    // �V���ɖ����쐬����
    public void CreateNewQuestion(int monitorID)
    {
        // �����ŐV�������Ɠ�����ݒ肷��
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
        // ���̓t�B�[���h���N���A����
        answerInputField.text = "";
        // �N���C�A���g���ŌĂ�
        if (!IsServer)
        {
            SubmitAnswerServerRpc(userAnswer, forcusMonitorID);  // �T�[�o�[�֑��M
            return;
        }

        CheckAnswer(userAnswer, forcusMonitorID);  // �T�[�o�[�Ȃ璼�ڔ���
    }

    public void OnTimerFinished(int timerID)
    {
        if (!IsServer) return;

        timers[timerID - 1].ResetTimer();
        CreateNewQuestion(timerID);
    }
    // ---- �ǉ� ----
    [ServerRpc(RequireOwnership = false)]
    private void SubmitAnswerServerRpc(string userAnswer, int MonitorID)
    {
        CheckAnswer(userAnswer, MonitorID);
    }

    // ---- ���ʔ��菈�� ----
    private void CheckAnswer(string userAnswer, int MonitorID)
    {
        if (userAnswer == answers[MonitorID - 1])
        {
            Debug.Log("�����ł��I�i�T�[�o�[����j");
            solvedList[MonitorID - 1] = new FixedString32Bytes("1");
        }
        else
        {
            Debug.Log($"�s�����i�T�[�o�[����j: {userAnswer} / ����: {answers}");
        }
    }

    // �A�i�O�������쐬����
    void CreateAnagram()
    {
        if (words == null || words.Length == 0) return;

        // �����_����1�I��
        string answer = words[UnityEngine.Random.Range(0, words.Length)].Trim();
        if (string.IsNullOrEmpty(answer) || answer.Length <= 3) { CreateAnagram(); return; }

        // �������V���b�t��
        char[] chars = answer.ToCharArray();

        while (new string(chars) == answer) // ���̕�����Ɠ����ꍇ�͍ăV���b�t��
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
        '��','��','��','��','��',
        '��','��','��','��','��',
        '��','��','��','��','��',
        '��','��','��','��','��',
        '��','��','��','��','��',
        '��','��','��','��','��',
        '��','��','��','��','��',
        '��','��','��',
        '��','��','��','��','��',
        '��','��','��',
        '��','��','��','��','��',
        '��','��','��','��','��',
        '��','��','��','��','��',
        '��','��','��','��','��',
        '��','��','��','��','��',
        '��','��','��',
        '��','��','��','��','��'
    };

    char ShiftHiragana(char c)
    {
        if (c == '��') return '��';
        if (c == '��') return '��';
        if (c == '��') return '��';
        if (c == '��') return '��';
        if (c == '��') return '��';
        if (c == '��') return '��';
        if (c == '��') return '��';
        if (c == '��') return '��';
        if (c == '��') return '��';
        if (c == '��') return '��';
        if (c == '��') return '��';
        if (c == '��') return '��';
        int index = System.Array.IndexOf(hiraganaList, c);
        if (index < 0) return c; // ���X�g�ɂȂ������͂��̂܂�

        // �O�����ɂ��炷
        int offset = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
        return hiraganaList[index + offset];
    }

    void CreateDial()
    {
        if (words == null || words.Length == 0) return;

        // �����_����1�I��
        string answer;
        do
        {
            answer = words[UnityEngine.Random.Range(0, words.Length)].Trim();
        } while (answer.Contains('�[') || answer.Contains('��'));
        if (string.IsNullOrEmpty(answer) || answer.Length <= 3) { CreateDial(); return; }

        // �e������O��ɂ��炷�i���_�͂��̂܂܁j
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
        // --- ������ ---
        List<string> wlist = new List<string>();
        char[,] grid = new char[6, 6];
        bool[,] rawBlock = new bool[6, 6];
        bool[,] colBlock = new bool[6, 6];

        int[] dr = { -1, 1, 0, 0 }; // ����
        int[] dc = { 0, 0, -1, 1 }; // ����
        for (int r = 0; r < 6; r++)
        {
            for (int c = 0; c < 6; c++)
            {
                grid[r, c] = '�@';   // �S�p�X�y�[�X
                rawBlock[r, c] = false;
                colBlock[r, c] = false;
            }
        }

        System.Random rnd = new System.Random();
        int tryCount = 0;

        // --- �P��������z�u ---
        while (tryCount < 200 && wlist.Count < 6) // 200��܂Ŏ��s
        {
            int Query = 1000;
            var pairs = new List<Tuple<string, int, int, bool, int>>();
            while (Query-- > 0 || pairs.Count <= 0)
            {
                string word = words[rnd.Next(words.Length)].Trim();
                if (word.Length < 2 || word.Length > 6) { tryCount++; continue; }

                bool horizontal = rnd.Next(2) == 0; // true=��
                int maxRow = horizontal ? 6 : 6 - word.Length + 1;
                int maxCol = horizontal ? 6 - word.Length + 1 : 6;
                int row = rnd.Next(maxRow);
                int col = rnd.Next(maxCol);
                int overlapCount = 0;
                if (wlist.Count != 0)
                {
                    // �Փ˔���
                    bool canPlace = true;
                    for (int i = 0; i < word.Length; i++)
                    {
                        int r = row + (horizontal ? 0 : i);
                        int c = col + (horizontal ? i : 0);
                        char g = grid[r, c];
                        if (g != '�@' && g != word[i] || horizontal && rawBlock[r, c] || !horizontal && colBlock[r, c]) { canPlace = false; break; }
                        if (g == word[i]) overlapCount++;
                        if (i == 0)
                        {
                            if (horizontal && c > 0 && grid[r, c - 1] != '�@') { canPlace = false; break; }
                            ;
                            if (!horizontal && r > 0 && grid[r - 1, c] != '�@') { canPlace = false; break; }
                            ;
                        }
                        else if (i + 1 == word.Length)
                        {
                            if (horizontal && c + 1 < 6 && grid[r, c + 1] != '�@') { canPlace = false; break; }
                            ;
                            if (!horizontal && r + 1 < 6 && grid[r + 1, c] != '�@') { canPlace = false; break; }
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
            
            // �z�u
            for (int i = 0; i < word0.Length; i++)
            {
                int r = row0 + (horizontal0 ? 0 : i);
                int c = col0 + (horizontal0 ? i : 0);
                grid[r, c] = word0[i];

                for (int dir = 0; dir < 4; dir++)
                {
                    int nr = r + dr[dir];
                    int nc = c + dc[dir];

                    // �͈͊O�`�F�b�N
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

        // --- �����v�[�����瓚������� ---
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
            if (canMake && word.Length >= 2 && word.Length <= 6) // �����t��
                candidates.Add(word);
        }

        if (candidates.Count > 0)
        {
            string answer = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            answers[2] = answer; // Skeleton�p�̓���
            Debug.Log("Skeleton�̓���: " + answer);
        }
        else
        {
            Debug.LogWarning("�����v�[���������P�ꂪ����܂���");
        }

        // --- NetworkList �֔��f ---
        wordList.Clear();
        gridRows.Clear();
        // �P�ꃊ�X�g�ǉ�
        foreach (var w in wlist)
            wordList.Add(new FixedString128Bytes(w));

        // �Ֆʏ�����
        for (int r = 0; r < 6; r++)
        {
            char[] line = new char[6];
            for (int c = 0; c < 6; c++)
            {
                line[c] = (grid[r, c] == '�@') ? '�@' : '��';
                for (int d = 0; d < answers[2].Length; d++)
                {
                    if (answers[2][d] == grid[r,c])
                    {
                        line[c] = "�@�A�B�C�D�E"[d];
                        break;
                    }
                }
            }
            gridRows.Add(new FixedString128Bytes(new string(line)));
        }
        
    }
}