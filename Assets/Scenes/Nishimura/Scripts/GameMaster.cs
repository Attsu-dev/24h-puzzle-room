using UnityEngine;
using TMPro;
using WebGLSupport;
using Unity.Netcode;
using Unity.Collections;

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

    // private�ϐ�
    private string[] answers = new string[4];
    private string[] words;   // �ǂݍ��񂾒P�ꃊ�X�g

    // ���L�ϐ�
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
        // �ω�����������TextMeshPro�ɔ��f
        sharedAnagram.OnValueChanged += (oldVal, newVal) =>
        {
            if (anagramText != null)
                anagramText.text = sharedAnagram.Value.ToString();
        };

        // �Q�����ɂ��ŐV�l�𔽉f
        if (anagramText != null)
            anagramText.text = sharedAnagram.Value.ToString();

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
        subCameras[forcusMonitorID-1].enabled = true;

        // Canvas��\������
        canvasObject.SetActive(true);
    }

    // back�{�^���������ꂽ�Ƃ�
    public void OnBackButtonClicked()
    {
        // �J���������ɖ߂�
        subCameras[forcusMonitorID-1].enabled = false;
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
        solvedList[monitorID-1] = new FixedString32Bytes("0");
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
        
        timers[timerID-1].ResetTimer();
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
        if (userAnswer == answers[MonitorID-1])
        {
            Debug.Log("�����ł��I�i�T�[�o�[����j");
            solvedList[0] = new FixedString32Bytes("1");
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
        string answer = words[Random.Range(0, words.Length)].Trim();
        if (string.IsNullOrEmpty(answer) || answer.Length <= 2) { CreateAnagram(); return; }

        // �������V���b�t��
        char[] chars = answer.ToCharArray();

        while (new string(chars) == answer) // ���̕�����Ɠ����ꍇ�͍ăV���b�t��
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
