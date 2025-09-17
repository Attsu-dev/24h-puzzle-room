using UnityEngine;
using TMPro;
using Unity.Netcode;

public class CountdownTimer : NetworkBehaviour
{
    [Header("�ݒ�")]
    public int timerID = 0;
    public TMP_Text timerText;   // Inspector �Ŋ��蓖��
    public float timeLimit = 10f; // �������ԁi�b�j

    // �T�[�o�[���������݁A�S�N���C�A���g���ǂݎ��
    private NetworkVariable<float> sharedTime = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool isRunning = false;

    public override void OnNetworkSpawn()
    {
        // �Q�������N���C�A���g�ɂ����݂̎��Ԃ𔽉f
        UpdateTimerText(sharedTime.Value);

        // �l���ς������UI���X�V
        sharedTime.OnValueChanged += (oldVal, newVal) =>
        {
            UpdateTimerText(newVal);
        };

        // �T�[�o�[�̂݃^�C�}�[�J�n
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

    // ===== �T�[�o�[��p���� =====

    /// <summary>
    /// �V�����^�C�}�[���J�n
    /// </summary>
    public void StartTimer(float duration = -1f)
    {
        if (!IsServer) return;

        sharedTime.Value = (duration > 0f) ? duration : timeLimit;
        isRunning = true;
    }

    /// <summary>
    /// �c�莞�Ԃ������l�ɖ߂�
    /// </summary>
    public void ResetTimer()
    {
        if (!IsServer) return;

        sharedTime.Value = timeLimit;
        isRunning = true;
    }

    /// <summary>
    /// �T�[�o�[����c�莞�Ԃ��Q��
    /// </summary>
    public float RemainingTime => sharedTime.Value;

    /// <summary>
    /// �T�[�o�[���瓮�쒆���ǂ����Q��
    /// </summary>
    public bool IsRunning => isRunning;
}
