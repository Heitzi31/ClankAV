using UnityEngine;
using Photon.Pun;

public class CreateAndJoin : MonoBehaviourPunCallbacks
{
    public bool disableDebugLogs = false;

    [Header("Auto Match Settings")]
    public string autoRoomName = "AutoRoom";

    private const string GAME_SCENE = "SampleScene";

    private bool requestSent = false;

    private void Log(string key, object value)
    {
        if (!disableDebugLogs)
            DebugStorage.Log(key, value);
    }

    public void AutoCreateOrJoin()
    {
        if (requestSent)
            return;

        requestSent = true;

        Log("AutoMatch", "Versuche CreateRoom: " + autoRoomName);
        PhotonNetwork.CreateRoom(autoRoomName);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Log("AutoMatch", "Create fehlgeschlagen → JoinRoom");
        PhotonNetwork.JoinRoom(autoRoomName);
    }

    public override void OnJoinedRoom()
    {
        Log("OnJoinedRoom", PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.LoadLevel(GAME_SCENE);
    }
}
