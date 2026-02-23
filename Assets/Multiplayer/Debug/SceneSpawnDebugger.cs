using System;
using System.Linq;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// Debugger: Prüft Szene-Objekte (Tag) auf Photon-Readiness:
/// - Name, Position, Rotation, Parent
/// - Existenz eines Prefabs mit gleichem Namen in Resources
/// - PhotonView vorhanden? NetworkSync vorhanden?
/// - Optional: Test-Spawn via PhotonNetwork.Instantiate (nur MasterClient)
/// Ergebnisse schreibt es in DebugStorage (dein Overlay zeigt das).
/// </summary>
public class SceneSpawnDebugger : MonoBehaviour
{
    [Tooltip("Tag aller Szene-Objekte, die du synchronisieren willst")]
    public string tagToCheck = "NetworkedSceneObject";

    [Tooltip("Wenn true, wird einmalig beim Start eine Ressourcen-Indexprüfung durchgeführt (kann langsam sein)")]
    public bool scanResourcesAtStart = true;

    [Tooltip("Wenn true, versucht der MasterClient einen Test-Spawn des ersten gefundenen Objekts")]
    public bool performTestSpawnAsMaster = false;

    // interner Cache der verfügbaren Prefab-Namen in Resources
    private HashSet<string> resourcesPrefabNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    private void Start()
    {
        DebugStorage.Log("SceneSpawnDebugger", "Start");

        // XR/Photon Status
        DebugStorage.Log("IsMasterClient", PhotonNetwork.IsMasterClient ? "yes" : "no");
        DebugStorage.Log("PhotonConnected", PhotonNetwork.IsConnected ? "yes" : "no");
        DebugStorage.Log("PhotonRoom", PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.Name : "none");

        if (scanResourcesAtStart)
            BuildResourcesIndex();

        InspectSceneObjects();

        if (performTestSpawnAsMaster)
            TryTestSpawnFirst();
    }

    private void BuildResourcesIndex()
    {
        try
        {
            // heavy operation but acceptable for debugging
            var all = Resources.LoadAll<GameObject>("");
            resourcesPrefabNames.Clear();
            foreach (var go in all)
            {
                if (go == null) continue;
                // strip clone suffix if any - but Resources returns raw prefabs
                resourcesPrefabNames.Add(go.name);
            }
            DebugStorage.Log("ResourcesIndexedCount", resourcesPrefabNames.Count.ToString());
        }
        catch (Exception e)
        {
            DebugStorage.Log("ResourcesIndexError", e.Message);
        }
    }

    private void InspectSceneObjects()
    {
        GameObject[] objs;
        try
        {
            objs = GameObject.FindGameObjectsWithTag(tagToCheck);
        }
        catch (Exception)
        {
            DebugStorage.Log("FindTagError", $"Tag '{tagToCheck}' not found or not used.");
            return;
        }

        DebugStorage.Log("SceneObjectsFound", objs.Length.ToString());

        if (objs.Length == 0)
            return;

        // collect a few human-readable lines
        int i = 0;
        foreach (var obj in objs)
        {
            i++;
            string baseName = obj.name.Replace("(Clone)", "").Trim();
            string keyPrefix = $"Obj{i}";

            DebugStorage.Log($"{keyPrefix}_Name", baseName);
            DebugStorage.Log($"{keyPrefix}_Active", obj.activeInHierarchy ? "true" : "false");
            DebugStorage.Log($"{keyPrefix}_Pos", FormatVec(obj.transform.position));
            DebugStorage.Log($"{keyPrefix}_Rot", FormatQuat(obj.transform.rotation));
            DebugStorage.Log($"{keyPrefix}_Parent", obj.transform.parent ? obj.transform.parent.name : "ROOT");

            // PhotonView?
            var pv = obj.GetComponent<PhotonView>();
            DebugStorage.Log($"{keyPrefix}_HasPhotonView", pv != null ? "yes" : "no");
            if (pv != null)
            {
                DebugStorage.Log($"{keyPrefix}_ViewID", pv.ViewID.ToString());
                DebugStorage.Log($"{keyPrefix}_IsMine", pv.IsMine ? "true" : "false");
            }

            // NetworkSync?
            var ns = obj.GetComponent("NetworkSync");
            DebugStorage.Log($"{keyPrefix}_HasNetworkSync", ns != null ? "yes" : "no");

            // Is there a prefab with that name in Resources?
            bool inResources = resourcesPrefabNames.Contains(baseName);
            DebugStorage.Log($"{keyPrefix}_PrefabInResources", inResources ? "yes" : "no");

            // If not found, try a fallback search through Resources names (case-insensitive substring)
            if (!inResources)
            {
                var match = resourcesPrefabNames.FirstOrDefault(n => string.Equals(n, baseName, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    DebugStorage.Log($"{keyPrefix}_PrefabInResources", $"yes (match:{match})");
                    inResources = true;
                }
            }

            // Also record Collider/Rigidbody presence (often needed for grabs)
            var col = obj.GetComponent<Collider>();
            var rb = obj.GetComponent<Rigidbody>();
            DebugStorage.Log($"{keyPrefix}_HasCollider", col != null ? "yes" : "no");
            DebugStorage.Log($"{keyPrefix}_HasRigidbody", rb != null ? "yes" : "no");
        }
    }

    private void TryTestSpawnFirst()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            DebugStorage.Log("TestSpawn", "Skipped - not MasterClient");
            return;
        }

        GameObject[] objs = GameObject.FindGameObjectsWithTag(tagToCheck);
        if (objs.Length == 0)
        {
            DebugStorage.Log("TestSpawn", "No objects with tag found");
            return;
        }

        var first = objs[0];
        string prefabName = first.name.Replace("(Clone)", "").Trim();

        // Try a direct Resources.Load first
        GameObject candidate = Resources.Load<GameObject>(prefabName);
        if (candidate == null)
        {
            // Attempt to find any resource whose name matches ignoring case
            candidate = Resources.LoadAll<GameObject>("").FirstOrDefault(g => string.Equals(g.name, prefabName, StringComparison.OrdinalIgnoreCase));
        }

        if (candidate == null)
        {
            DebugStorage.Log("TestSpawn", $"Prefab '{prefabName}' NOT found in Resources. Cannot instantiate.");
            return;
        }

        try
        {
            // Spawn via Photon
            GameObject inst = PhotonNetwork.Instantiate(prefabName, first.transform.position, first.transform.rotation);
            if (inst != null)
            {
                DebugStorage.Log("TestSpawn", $"Spawned '{prefabName}' OK");
                var pv = inst.GetComponent<PhotonView>();
                DebugStorage.Log("TestSpawn_ViewID", pv != null ? pv.ViewID.ToString() : "no PhotonView");
            }
            else
            {
                DebugStorage.Log("TestSpawn", $"PhotonNetwork.Instantiate returned null for '{prefabName}'");
            }
        }
        catch (Exception e)
        {
            DebugStorage.Log("TestSpawnException", e.Message);
        }
    }

    private string FormatVec(Vector3 v)
    {
        return $"{v.x:F3},{v.y:F3},{v.z:F3}";
    }

    private string FormatQuat(Quaternion q)
    {
        return $"{q.x:F3},{q.y:F3},{q.z:F3},{q.w:F3}";
    }

    // Optional helper: Call this from the Inspector to rebuild the resources index at runtime
    [ContextMenu("Rebuild Resources Index")]
    public void RebuildResourcesIndexContext()
    {
        BuildResourcesIndex();
        DebugStorage.Log("ResourcesIndexedCount", resourcesPrefabNames.Count.ToString());
    }
}
