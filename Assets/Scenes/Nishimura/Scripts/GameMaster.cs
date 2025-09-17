using UnityEngine;
using TMPro;
using WebGLSupport;

public class GameMaster : MonoBehaviour
{
    // ���ꂪ���邱�ƂŁA���̃X�N���v�g����GameMaster�ɃA�N�Z�X�ł���i�V���O���g���j
    public static GameMaster Instance { get; private set; }
    void Awake()
    {
        Instance = this;
    }

    // public�ϐ�
    public int forcusMonitorID = 0; // 0�͂ǂ̃��j�^�[�ɂ��t�H�[�J�X���Ă��Ȃ����

    // GameMaster�Ő��䂷��I�u�W�F�N�g
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera subCamera;
    [SerializeField] private GameObject canvasObject;
    [SerializeField] private Anagram anagram;
    [SerializeField] private TMP_InputField answerInputField;
    [SerializeField] private CountdownTimer timer1;

    // private�ϐ�
    private bool solved = false;
    private string answer = "";

    // �v���C���[�����j�^�[���N���b�N�����Ƃ�
    public void OnMonitorClicked(GameObject clickedMonitor)
    {
        Debug.Log($"Cube {clickedMonitor.name} ���N���b�N����܂���");

        // �J������؂�ւ���
        mainCamera.enabled = false;
        subCamera.enabled = true;

        // �N���b�N���ꂽ���j�^�[�Ƀt�H�[�J�X����
        forcusMonitorID = 1; // �����ł͉���1��ݒ�B���ۂɂ�clickedMonitor�Ɋ�Â��Đݒ肷��K�v������܂��B

        // Canvas��\������
        canvasObject.SetActive(true);

        CreateNewQuestion();
    }

    // back�{�^���������ꂽ�Ƃ�
    public void OnBackButtonClicked()
    {
        // �J���������ɖ߂�
        mainCamera.enabled = true;
        subCamera.enabled = false;
        // �t�H�[�J�X����������
        forcusMonitorID = 0;
        // Canvas���\���ɂ���
        canvasObject.SetActive(false);
        // ���̓t�B�[���h���N���A����
        answerInputField.text = "";
    }

    // �V���ɖ����쐬����
    public void CreateNewQuestion()
    {
        // �����ŐV�������Ɠ�����ݒ肷��
        answer = anagram.CreateNextQuestion();
    }

    public void OnSubmitAnswer(string userAnswer)
    {
        if (userAnswer == answer)
        {
            Debug.Log("�����ł��I");
            // �������̏����������ɒǉ�
            solved = true;
        }
        else
        {
            Debug.Log("�s�����ł��B������x�����Ă��������B");
            // �s�������̏����������ɒǉ�
        }
        // ���̓t�B�[���h���N���A����
        answerInputField.text = "";
    }

    public void OnTimerFinished(int timerID)
    {
        if (!solved)
        {
            Debug.Log("�Q�[���I�[�o�[");
        }
        CreateNewQuestion();

        solved = false;
        timer1.Reset();
    }

    
}
