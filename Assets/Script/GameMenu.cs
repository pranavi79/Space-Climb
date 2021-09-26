using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Transports.UNET;

public class GameMenu : NetworkBehaviour
{

    // TODO: Check approval code
    // TODO: Improve player prefab

    private string ipAddress = "127.0.0.1";
    private string name = "Player";
    private string password = "";
    private UNetTransport transport;

    public Camera lobbyCamera;
    public GameObject menuPanel;
    public GameObject crossHair;

    public InputField nameField;
    public InputField hostPassField;
    public InputField roomIdField;
    public InputField clientPassField;

    public void Start()
    {
        //FindObjectOfType<AudioManager>().Play("main");
        crossHair.SetActive(false);
        lobbyCamera.enabled = true;
        menuPanel.SetActive(true);
    }

    public void HostGame()
    {
        if (nameField.text.Length != 0)
        {
            name = nameField.text;
        }

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost(GetSpawnLocation(), Quaternion.identity);

        PlayerDetails.name = name;
        PlayerDetails.clientId = NetworkManager.Singleton.LocalClientId;

        lobbyCamera.enabled = false;
        crossHair.SetActive(true);
        menuPanel.SetActive(false);
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientID, NetworkManager.ConnectionApprovedDelegate callback)
    {
        if (hostPassField.text.Length != 0)
        {
            password = hostPassField.text;
        }

        bool approve = System.Text.Encoding.ASCII.GetString(connectionData) == password;
        callback(true, null, approve, GetSpawnLocation(), Quaternion.identity);
    }

    Vector3 GetSpawnLocation()
    {
        float x = -46;
        float y = -80;
        float z = 140;
        return new Vector3(x, y, z);
    }
    public void IPAddressChanged(string newAddress)
    {
        this.ipAddress = newAddress;
    }

    public void JoinGame()
    {
        if (nameField.text.Length != 0)
        {
            name = nameField.text;
        }

        if (clientPassField.text.Length != 0)
        {
            password = clientPassField.text;
        }

        if (roomIdField.text.Length != 0)
        {
            ipAddress = roomIdField.text;
        }

        transport = NetworkManager.Singleton.GetComponent<UNetTransport>();
        transport.ConnectAddress = ipAddress;

        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(password);
        NetworkManager.Singleton.StartClient();

        PlayerDetails.name = name;
        PlayerDetails.clientId = NetworkManager.Singleton.LocalClientId;

        lobbyCamera.enabled = false;
        crossHair.SetActive(true);
        menuPanel.SetActive(false);
    }
}

