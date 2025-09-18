using UnityEngine;
using TMPro;
using Unity.Netcode;

public class CountdownTimer : NetworkBehaviour
{
    [Header("設定")]
    public int timerID = 0;
    public TMP_Text timerText;   // Inspector で割り当て
    public float timeLimit = 10f; // 制限時間（秒）

    // サーバーが書き込み、全クライアントが読み取り
    private NetworkVariable<float> sharedTime = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool isRunning = false;

    public override void OnNetworkSpawn()
    {
        // 参加したクライアントにも現在の時間を反映
        UpdateTimerText(sharedTime.Value);

        // 値が変わったらUIを更新
        sharedTime.OnValueChanged += (oldVal, newVal) =>
        {
            UpdateTimerText(newVal);
        };

        // サーバーのみタイマー開始
        if (IsServer)
        {
            StartTimer();
        }
    }

    void Update()
    {
        if (!IsServer || !isRunning) return;

        sharedTime.Value -= Time.deltaTime;

        if (sharedTime.Value <= 0f)
        {
            sharedTime.Value = 0f;
            isRunning = false;
            GameMaster.Instance.OnTimerFinished(timerID);
        }
    }

    private void UpdateTimerText(float time)
    {
        int totalSeconds = Mathf.CeilToInt(time);
        System.TimeSpan t = System.TimeSpan.FromSeconds(totalSeconds);

        if (timerText != null)
            timerText.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
    }

    // ===== サーバー専用操作 =====

    /// <summary>
    /// 新しくタイマーを開始
    /// </summary>
    public void StartTimer(float duration = -1f)
    {
        if (!IsServer) return;

        sharedTime.Value = (duration > 0f) ? duration : timeLimit;
        isRunning = true;
    }

    /// <summary>
    /// 残り時間を初期値に戻す
    /// </summary>
    public void ResetTimer()
    {
        if (!IsServer) return;

        sharedTime.Value = timeLimit;
        isRunning = true;
    }

    /// <summary>
    /// サーバーから残り時間を参照
    /// </summary>
    public float RemainingTime => sharedTime.Value;

    /// <summary>
    /// サーバーから動作中かどうか参照
    /// </summary>
    public bool IsRunning => isRunning;
}
