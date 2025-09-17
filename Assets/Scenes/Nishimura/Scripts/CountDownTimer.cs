using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    public int timerID = 0;
    public TMP_Text timerText;   // TimerText を Inspector で割り当て
    public float timeLimit = 10f; // 制限時間（秒）

    private float timeRemaining;
    private bool isRunning = false;

    void Start()
    {
        timeRemaining = timeLimit;
        isRunning = true; // スタート時に自動開始
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

            // 秒単位で表示（小数切り捨て）
            int totalSeconds = Mathf.CeilToInt(timeRemaining);
            System.TimeSpan t = System.TimeSpan.FromSeconds(totalSeconds);
            timerText.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);

        }
    }

    public void Reset()
    {
        timeRemaining = timeLimit;
    }
}
