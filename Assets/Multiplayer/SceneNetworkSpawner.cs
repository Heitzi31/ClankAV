using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SceneNetworkSpawner : MonoBehaviourPunCallbacks
{
    [Header("Spawner Settings")]
    public string sceneObjectTag = "NetworkedSceneObject";
    public string networkRootName = "NetworkedObjectsRoot";
    public bool destroyLocalSceneObjectsOnMaster = true; // nur Master zerstört

    [Header("Debug Settings")]
    public bool disableDebugLogs = false;

    private void Start()
    {
        Log("Spawner", "Start – Warte auf Photon InRoom...");
        StartCoroutine(WaitForPhotonAndSpawn());
    }

    private IEnumerator WaitForPhotonAndSpawn()
    {
        while (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
            yield return null;

        Log("PhotonStatus", $"InRoom: {PhotonNetwork.CurrentRoom?.Name}, Master={PhotonNetwork.IsMasterClient}");

        // 1️⃣ Lokale Szeneobjekte nur zerstören, wenn Master
        if (destroyLocalSceneObjectsOnMaster && PhotonNetwork.IsMasterClient)
            DestroyLocalSceneObjects();

        // 2️⃣ MasterClient spawnt Objekte
        if (PhotonNetwork.IsMasterClient)
        {
            Log("Spawner", "MasterClient → sammle Szeneobjekte");

            List<SceneObjectInfo> list = CollectSceneObjectsToSpawn();

            yield return new WaitForSeconds(0.1f); // kleine Verzögerung

            foreach (var info in list)
            {
                GameObject prefab = LoadPrefab(info.prefabName);
                if (prefab == null)
                {
                    Log("SpawnError", $"Prefab '{info.prefabName}' nicht in Resources.");
                    continue;
                }

                try
                {
                    GameObject spawned = PhotonNetwork.Instantiate(info.prefabName, info.position, info.rotation, 0);
                    Log("Spawned_" + info.prefabName,
                        $"Pos={info.position} Rot={info.rotation.eulerAngles} ViewID={(spawned.GetComponent<PhotonView>()?.ViewID ?? -1)}");
                }
                catch (Exception e)
                {
                    Log("SpawnException", e.Message);
                }
            }
        }
        else
        {
            // Non-MasterClients: warten, Photon instanziert automatisch
            Log("Spawner", "Nicht MasterClient → warte auf Netzwerkinstanzen");
        }
    }

    private void DestroyLocalSceneObjects()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(sceneObjectTag);
        Log("SceneObjs_PreDestroy", objs.Length.ToString());

        foreach (GameObject obj in objs)
        {
            try { Destroy(obj); }
            catch { }
        }

        Log("SceneObjs_Destroyed", "true");
    }

    private List<SceneObjectInfo> CollectSceneObjectsToSpawn()
    {
        List<SceneObjectInfo> list = new List<SceneObjectInfo>();
        GameObject[] objs = GameObject.FindGameObjectsWithTag(sceneObjectTag);

        Log("SceneObjs_MasterCollect", objs.Length.ToString());

        foreach (GameObject obj in objs)
        {
            SceneObjectInfo info = new SceneObjectInfo
            {
                prefabName = obj.name.Replace("(Clone)", "").Trim(),
                position = obj.transform.position,
                rotation = obj.transform.rotation
            };
            list.Add(info);

            try { Destroy(obj); } catch { }
        }

        return list;
    }

    private GameObject LoadPrefab(string prefabName)
    {
        GameObject prefab = Resources.Load<GameObject>(prefabName);
        if (prefab != null) return prefab;

        GameObject[] all = Resources.LoadAll<GameObject>("");
        foreach (GameObject g in all)
        {
            if (string.Equals(g.name, prefabName, StringComparison.OrdinalIgnoreCase))
                return g;
        }

        return null;
    }

    private void Log(string key, object value)
    {
        if (!disableDebugLogs)
            DebugStorage.Log(key, value);
    }

    [Serializable]
    private class SceneObjectInfo
    {
        public string prefabName;
        public Vector3 position;
        public Quaternion rotation;
    }
}
