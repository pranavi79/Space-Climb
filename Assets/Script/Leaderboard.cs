using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine.UI;

public class Leaderboard : NetworkBehaviour
{
    public Camera winnerCamera;
    public Text winnerText;
    public Camera lobbyCamera;
    public GameObject winnerPanel;
    public GameObject crossHair;
    public GameObject menuPanel;

    [ClientRpc]
    void broadcastWinnerClientRpc(string winnerName)
    {
        //FindObjectOfType<AudioManager>().Play("main");
        winnerPanel.SetActive(true);
        winnerCamera.enabled = true;
        crossHair.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        winnerText.text = "Winner is " + winnerName + "!";
    }


    void OnCollisionEnter(Collision collision)
    {
        ulong id = collision.collider.GetComponentInParent<NetworkObject>().OwnerClientId;

        if ((id == NetworkManager.Singleton.LocalClientId) && collision.collider.tag == "Player")
        {
            broadcastWinnerServerRpc(PlayerDetails.name);
        }

    }

    [ServerRpc(RequireOwnership = false)]
    void broadcastWinnerServerRpc(string winnerName)
    {
        broadcastWinnerClientRpc(winnerName);
    }

    public void BackToMenu()
    {
        if (IsHost)
        {
            NetworkManager.Singleton.StopHost();
        }
        else if (IsClient)
        {
            NetworkManager.Singleton.StopClient();
        }
        else if (IsServer)
        {
            NetworkManager.Singleton.StopServer();
        }

        winnerCamera.enabled = false;
        winnerPanel.SetActive(false);

        lobbyCamera.enabled = true;
        menuPanel.SetActive(true);
        
    }
}
