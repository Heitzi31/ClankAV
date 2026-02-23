using Photon.Pun;
using UnityEngine;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public bool disableDebugLogs = false;
    public CreateAndJoin createAndJoin;

    private void Log(string key, object value)
    {
        if (!disableDebugLogs)
            DebugStorage.Log(key, value);
    }

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        Log("Photon", "AutomaticallySyncScene = true");
    }

    void Start()
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "eu";
        PhotonNetwork.ConnectUsingSettings();
        Log("Photon", "ConnectUsingSettings()");
    }

    void Update()
    {
        Log("Photon-State", PhotonNetwork.NetworkClientState.ToString());
    }

    public override void OnConnectedToMaster()
    {
        Log("Photon", "OnConnectedToMaster");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Log("Photon", "OnJoinedLobby");

        if (createAndJoin != null)
        {
            createAndJoin.AutoCreateOrJoin();
        }
        else
        {
            Log("AutoMatch", "CreateAndJoin Referenz fehlt");
        }
    }
}
