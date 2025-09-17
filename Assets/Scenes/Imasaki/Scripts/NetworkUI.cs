using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = System.Random;
// RelayのAllocationモデルに 'RelayAllocation' という別名を付ける
using RelayAllocation = Unity.Services.Relay.Models.Allocation;

// RelayのJoinAllocationモデルにも別名を付けておくと便利
using RelayJoinAllocation = Unity.Services.Relay.Models.JoinAllocation;

public class NetworkUI : MonoBehaviour
{
    [Header("UI Elements")]
    private string _joinCode;
    private string joinCode;
    private string _playerName = "プレイヤー名" + (new Random().Next(100,1000)).ToString();

    void Start()
    {
        InitializeAndSignIn();
    }

    public string PlayerName
    {
        get => _playerName;
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 600));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    private void StartButtons()
    {
        GUILayout.Label("24h-puzzle-room\n");

        if (GUILayout.Button("Host"))
        {
            StartHost();
        }

        if (GUILayout.Button("Client"))
        {
            StartClient(_joinCode.Trim());
        }
        
        GUILayout.Label("JoinCode");
        _joinCode = GUILayout.TextField(_joinCode);
        
        GUILayout.Label("PlayerName");
        _playerName = GUILayout.TextField(_playerName);
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
            RelayAllocation allocation = await RelayService.Instance.CreateAllocationAsync(5);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            // UIにJoinコードを表示
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
    public async void StartClient(string joinCode) // Taskからvoidに変更、引数も削除
    {
        try
        {
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

    private void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ? "Host" :
            NetworkManager.Singleton.IsServer ? "Server" : "Client";
        if (joinCode == null)
        {
            joinCode = _joinCode;
        }

        GUILayout.Label("Transport: " +
                        NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
        GUILayout.Label("JoinCode: " + joinCode);
    }
}