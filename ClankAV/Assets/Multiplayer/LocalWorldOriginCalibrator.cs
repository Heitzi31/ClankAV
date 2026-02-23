

using UnityEngine;
using Photon.Pun;

public class LocalWorldOriginCalibrator : MonoBehaviour
{
    [Header("World Root (Parent of entire local scene)")]
    public Transform worldRoot;

    [Header("Network Settings")]
    [Tooltip("Name des Prefabs in Resources/, das als Kind von NetworkedObjectsRoot gespawnt wird.")]
    public string networkObjectPrefabName = "NetworkedObjectsRootObject";

    [Tooltip("Nur aktiv, wenn Network-Objekt erzeugt/verschoben werden soll.")]
    public bool enableNetworkObject = true;

    private Transform networkRoot;
    private GameObject spawnedNetworkObject;

    [Header("Input")]
    public OVRInput.Button mrPresident = OVRInput.Button.One;
    public OVRInput.Controller controller = OVRInput.Controller.RTouch;

    [Header("Calibration Settings")]
    public float positionSnap = 0.05f;
    public float rotationSnap = 5f;

    [Header("Debug Settings")]
    public bool disableDebugLogs = false;

    private void Start()
    {
        networkRoot = GameObject.Find("NetworkedObjectsRoot")?.transform;

        if (networkRoot == null)
            DebugStorage.Log("CalibError", "NetworkedObjectsRoot wurde nicht in der Szene gefunden!");
    }

    private void Update()
    {
        if (worldRoot == null)
        {
            DebugStorage.Log("Calibration Error", "worldRoot is null!");
            return;
        }

        if (OVRInput.GetDown(mrPresident))
            ApplyCalibration();
    }

    private void ApplyCalibration()
    {
        // Controller lokal zu Welt
        Vector3 controllerWorldPos = OVRInput.GetLocalControllerPosition(controller);
        Quaternion controllerWorldRot = OVRInput.GetLocalControllerRotation(controller);

        // Yaw-only Rotation
        Vector3 forward = controllerWorldRot * Vector3.forward;
        forward.y = 0f;
        forward.Normalize();
        Quaternion yawOnly = Quaternion.LookRotation(forward, Vector3.up);

        // Rotation snappen
        Vector3 yawEuler = yawOnly.eulerAngles;
        yawEuler.y = SnapAngle(yawEuler.y);
        yawOnly = Quaternion.Euler(yawEuler);

        // Root rotieren
        worldRoot.rotation = Quaternion.Euler(0, -90, 0) * yawOnly;

        // Root Position = Controller-Position direkt
        worldRoot.position = controllerWorldPos + new Vector3(0, 1, 0);

        Log("WorldRoot Pos", worldRoot.position);
        Log("WorldRoot Rot", worldRoot.rotation.eulerAngles);

        // -------------------------------------------
        // NETWORK OBJEKT HANDLING
        // -------------------------------------------

        if (!enableNetworkObject)
            return;

        if (spawnedNetworkObject == null)
        {
            GameObject networkPrefab = Resources.Load<GameObject>(networkObjectPrefabName);

            if (networkPrefab == null)
            {
                Log("NetError", $"Prefab '{networkObjectPrefabName}' wurde NICHT in Resources/ gefunden!");
                return;
            }

            spawnedNetworkObject = PhotonNetwork.Instantiate(networkObjectPrefabName, worldRoot.position, worldRoot.rotation);

            if (networkRoot != null)
                spawnedNetworkObject.transform.SetParent(networkRoot, worldPositionStays: true);

            Log("NetworkSpawn", $"Instanziiert @ {spawnedNetworkObject.transform.position}");
        }
        else
        {
            spawnedNetworkObject.transform.position = worldRoot.position;
            spawnedNetworkObject.transform.rotation = worldRoot.rotation;

            Log("NetworkMoved", $"Verschoben @ {spawnedNetworkObject.transform.position}");
        }
    }

    private void Log(string key, object value)
    {
        if (!disableDebugLogs)
            DebugStorage.Log(key, value);
    }

    private Vector3 SnapPosition(Vector3 pos)
    {
        return new Vector3(
            Mathf.Round(pos.x / positionSnap) * positionSnap,
            Mathf.Round(pos.y / positionSnap) * positionSnap,
            Mathf.Round(pos.z / positionSnap) * positionSnap
        );
    }

    private float SnapAngle(float angle)
    {
        return Mathf.Round(angle / rotationSnap) * rotationSnap;
    }
}
