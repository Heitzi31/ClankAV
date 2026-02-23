using System.Text;
using UnityEngine;
using Photon.Pun;

public class WorldDebugMonitor : MonoBehaviour
{
    [Header("Setup")]
    public string networkRootName = "NetworkedObjectsRoot";
    public Transform playerRig;   // dein CameraRig / XR Rig
    public OVRInput.Controller controller = OVRInput.Controller.RTouch;

    private Transform networkRoot;

    private void Start()
    {
        DebugStorage.Log("Debugger", "Started");

        networkRoot = GameObject.Find(networkRootName)?.transform;
        DebugStorage.Log("NetworkRootFound", (networkRoot != null).ToString());
    }

    private void Update()
    {
        DebugStorage.Log("Time", System.DateTime.Now.ToString("HH:mm:ss"));

        LogPlayerInfo();
        LogControllerOrientation();
        LogNetworkObjects();
    }

    private void LogPlayerInfo()
    {
        if (playerRig == null)
        {
            DebugStorage.Log("PlayerRig", "NOT SET");
            return;
        }

        DebugStorage.Log("Player_Pos", VectorToStr(playerRig.position));
        DebugStorage.Log("Player_RotY", playerRig.rotation.eulerAngles.y.ToString("F1"));
    }

    private void LogControllerOrientation()
    {
        Quaternion localRot = OVRInput.GetLocalControllerRotation(controller);

        // Stabile Y-Achse extrahieren
        Vector3 forwardProjected =
            Vector3.ProjectOnPlane(localRot * Vector3.forward, Vector3.up);
        Quaternion yawOnly = Quaternion.LookRotation(forwardProjected, Vector3.up);

        DebugStorage.Log("CtrlRot_Y", yawOnly.eulerAngles.y.ToString("F1"));
    }

    private void LogNetworkObjects()
    {
        if (networkRoot == null)
        {
            DebugStorage.Log("NetworkRoot", "NULL");
            return;
        }

        int childCount = networkRoot.childCount;
        DebugStorage.Log("NetworkObj_Count", childCount.ToString());

        for (int i = 0; i < childCount; i++)
        {
            Transform t = networkRoot.GetChild(i);
            PhotonView pv = t.GetComponent<PhotonView>();

            string prefix = $"Obj[{i}]_";

            DebugStorage.Log(prefix + "Name", t.name);
            DebugStorage.Log(prefix + "Pos", VectorToStr(t.position));
            DebugStorage.Log(prefix + "RotY", t.rotation.eulerAngles.y.ToString("F1"));

            if (pv != null)
            {
                DebugStorage.Log(prefix + "ViewID", pv.ViewID);
                DebugStorage.Log(prefix + "IsMine", pv.IsMine);
            }
            else
            {
                DebugStorage.Log(prefix + "ViewID", "NO PV");
            }

            Collider col = t.GetComponent<Collider>();
            DebugStorage.Log(prefix + "Collider", (col != null).ToString());

            Rigidbody rb = t.GetComponent<Rigidbody>();
            DebugStorage.Log(prefix + "Rigidbody", (rb != null).ToString());
        }
    }

    private string VectorToStr(Vector3 v)
    {
        return $"{v.x:F2},{v.y:F2},{v.z:F2}";
    }
}
