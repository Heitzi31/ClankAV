using System.Collections;
using Photon.Pun;
using UnityEngine;

public class NetworkedParentAssigner : MonoBehaviourPunCallbacks
{
    public string networkRootName = "NetworkedObjectsRoot";
    public bool disableDebugLogs = false;

    private void Start()
    {
        StartCoroutine(AssignParentWhenReady());
    }

    private IEnumerator AssignParentWhenReady()
    {
        Transform root = null;

        while (root == null)
        {
            root = GameObject.Find(networkRootName)?.transform;
            if (!disableDebugLogs) DebugStorage.Log("ParentAssigner", "Warte auf Root...");
            yield return null;
        }

        transform.SetParent(root, true); // true = Weltposition bleibt erhalten
        if (!disableDebugLogs) DebugStorage.Log("ParentAssigner", $"Prefab '{name}' parented zu '{networkRootName}'");
    }
}
