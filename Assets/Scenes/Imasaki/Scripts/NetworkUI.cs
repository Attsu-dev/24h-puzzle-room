using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
// RelayのAllocationモデルに 'RelayAllocation' という別名を付ける
using RelayAllocation = Unity.Services.Relay.Models.Allocation;

// RelayのJoinAllocationモデルにも別名を付けておくと便利
using RelayJoinAllocation = Unity.Services.Relay.Models.JoinAllocation;

public class NetworkUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TMP_InputField joinCodeInputField; // TextMeshProの場合
    // [SerializeField] private InputField joinCodeInputField; // 通常のInput Fieldの場合
    [SerializeField] private TMP_Text joinCodeText; // ホストがJoinコードを表示する用

void Start()
    {
        // --- ボタンに関数を割り当てる ---
        hostButton.onClick.AddListener(() => StartHost());
        clientButton.onClick.AddListener(() => StartClient());
        
        // --- 元のStart関数の処理 ---
        InitializeAndSignIn();
    }

    async void InitializeAndSignIn()
    {
        // Unity Gaming Servicesの初期化と匿名認証
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        Debug.Log($"Player ID: {AuthenticationService.Instance.PlayerId}");
    }

    // ホストとしてRelayサーバーを作成する
    public async void StartHost() // Taskからvoidに変更してボタンから呼びやすくする
    {
        try
        {
            // (中身は変更なし)
            RelayAllocation allocation = await RelayService.Instance.CreateAllocationAsync(5);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            // UIにJoinコードを表示
            joinCodeText.text = $"Join Code: {joinCode}";
            Debug.Log($"Join Code: {joinCode}");

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                false
            );
            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay start host failed: {e.Message}");
        }
    }

    // クライアントとしてRelayサーバーに参加する
    public async void StartClient() // Taskからvoidに変更、引数も削除
    {
        try
        {
            // InputFieldからJoinコードを取得
            string joinCode = joinCodeInputField.text.Trim();
            
            RelayJoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetClientRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData,
                false // <- この引数を追加
            );
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay start client failed: {e.Message}");
        }
    }
}