using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    public void StartHost()
    {
        Unity.Netcode.NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        Unity.Netcode.NetworkManager.Singleton.StartClient();
    }
}
