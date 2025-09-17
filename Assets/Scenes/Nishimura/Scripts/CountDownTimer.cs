using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    public int timerID = 0;
    public TMP_Text timerText;   // TimerText �� Inspector �Ŋ��蓖��
    public float timeLimit = 10f; // �������ԁi�b�j

    private float timeRemaining;
    private bool isRunning = false;

    void Start()
    {
        timeRemaining = timeLimit;
        isRunning = true; // �X�^�[�g���Ɏ����J�n
    }

    void Update()
    {
        if (isRunning)
        {
            timeRemaining -= Time.deltaTime;

            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                GameMaster.Instance.OnTimerFinished(timerID);
            }

            // �b�P�ʂŕ\���i�����؂�̂āj
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
        }
    }

    public void Reset()
    {
        timeRemaining = timeLimit;
    }
}
