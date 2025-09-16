using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public static GameMaster Instance { get; private set; }
    public Camera mainCamera;
    public Camera subCamera;
    public int forcusMonitorIndex = -1; // -1�͂ǂ̃��j�^�[���t�H�[�J�X���Ă��Ȃ����

    private string answer = "����ς�܂�";

    public GameObject canvasObject;


    void Awake()
    {
        Instance = this;
    }

    // �v���C���[�����j�^�[���N���b�N�����Ƃ�
    public void OnMonitorClicked(GameObject clickedMonitor)
    {
        Debug.Log($"Cube {clickedMonitor.name} ���N���b�N����܂���");

        // �J������؂�ւ���
        mainCamera.enabled = false;
        subCamera.enabled = true;

        // �N���b�N���ꂽ���j�^�[�Ƀt�H�[�J�X����
        forcusMonitorIndex = 0; // �����ł͉���0��ݒ�B���ۂɂ�clickedMonitor�Ɋ�Â��Đݒ肷��K�v������܂��B

        // Canvas��\������
        canvasObject.SetActive(true);
    }

    // back�{�^���������ꂽ�Ƃ�
    public void OnBackButtonClicked()
    {
        // �J���������ɖ߂�
        mainCamera.enabled = true;
        subCamera.enabled = false;
        // �t�H�[�J�X����������
        forcusMonitorIndex = -1;
        // Canvas���\���ɂ���
        canvasObject.SetActive(false);
    }

    public void OnSubmitAnswer(string userAnswer)
    {
        if (userAnswer == answer)
        {
            Debug.Log("�����ł��I");
            // �������̏����������ɒǉ�
        }
        else
        {
            Debug.Log("�s�����ł��B������x�����Ă��������B");
            // �s�������̏����������ɒǉ�
        }
    }
}
