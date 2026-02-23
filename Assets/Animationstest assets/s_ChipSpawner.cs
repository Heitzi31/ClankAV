
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class s_ChipSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject chipPrefab;

    [Header("Teams")]
    public Material colorTeamOne;
    public Material colorTeamTwo;
    public GameObject parentChipsTeamOne;
    public GameObject parentChipsTeamTwo;

    // Listen für die Start-Transformationen
    private List<Vector3> startPositionsT1 = new List<Vector3>();
    private List<Quaternion> startRotationsT1 = new List<Quaternion>();
    private List<Vector3> startPositionsT2 = new List<Vector3>();
    private List<Quaternion> startRotationsT2 = new List<Quaternion>();

    private void Awake()
    {
        // Speichert die exakte Lage aller Chips beim allerersten Start
        int countT1 = 0;
        int countT2 = 0;

        if (parentChipsTeamOne)
        {
            foreach (Transform t in parentChipsTeamOne.transform)
            {
                startPositionsT1.Add(t.position);
                startRotationsT1.Add(t.rotation);
                countT1++;
            }
        }
        if (parentChipsTeamTwo)
        {
            foreach (Transform t in parentChipsTeamTwo.transform)
            {
                startPositionsT2.Add(t.position);
                startRotationsT2.Add(t.rotation);
                countT2++;
            }
        }
        Debug.Log($"<color=lime>[s_ChipSpawner] Awake: Start-Positionen gespeichert. Team1: {countT1} Chips, Team2: {countT2} Chips.</color>");
    }

    public void RespawnAllChips()
    {
        Debug.Log("<color=lime>[s_ChipSpawner] RespawnAllChips aufgerufen. Räume Spielfeld auf...</color>");

        // 1. Alte Reste löschen
        int deletedCount = 0;
        foreach (Transform child in parentChipsTeamOne.transform) { Destroy(child.gameObject); deletedCount++; }
        foreach (Transform child in parentChipsTeamTwo.transform) { Destroy(child.gameObject); deletedCount++; }

        Debug.Log($"<color=lime>[s_ChipSpawner] {deletedCount} alte Chips zerstört. Beginne Neu-Spawnen...</color>");

        // 2. Neu spawnen und stabilisieren
        for (int i = 0; i < startPositionsT1.Count; i++)
        {
            SpawnAndStabilize(1, startPositionsT1[i], startRotationsT1[i]);
        }
        for (int i = 0; i < startPositionsT2.Count; i++)
        {
            SpawnAndStabilize(2, startPositionsT2[i], startRotationsT2[i]);
        }
    }

    private void SpawnAndStabilize(int team, Vector3 pos, Quaternion rot)
    {
        GameObject newChip = SpawnChip(team, pos, rot);

        Rigidbody rb = newChip.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Erstmal einfrieren, damit nichts umkippt
            rb.isKinematic = true;
            // Nach kurzer Verzögerung Physik einschalten
            StartCoroutine(EnablePhysics(rb));
        }
    }

    private IEnumerator EnablePhysics(Rigidbody rb)
    {
        // Warte einen Moment, bis alle Chips da sind
        yield return new WaitForSeconds(0.5f);
        if (rb != null)
        {
            rb.isKinematic = false;
            // Kleiner Log-Hinweis, dass die Physik jetzt läuft (nur einmal pro Stapel-Reset sinnvoll, sonst zu viel Spam)
        }
    }

    public GameObject SpawnChip(int team, Vector3 position, Quaternion rotation)
    {
        if (chipPrefab == null)
        {
            Debug.LogError("<color=red>[s_ChipSpawner] FEHLER: chipPrefab ist nicht im Inspector zugewiesen!</color>");
            return null;
        }

        GameObject newChip = Instantiate(chipPrefab, position, rotation);
        Renderer chipRenderer = newChip.GetComponent<Renderer>();

        if (team == 1)
        {
            if (chipRenderer) chipRenderer.material = colorTeamOne;
            newChip.tag = "ChipTeamOne";
            newChip.transform.SetParent(parentChipsTeamOne.transform);
        }
        else
        {
            if (chipRenderer) chipRenderer.material = colorTeamTwo;
            newChip.tag = "ChipTeamTwo";
            newChip.transform.SetParent(parentChipsTeamTwo.transform);
        }
        return newChip;
    }

    public void CalculateChipHeight()
    {
        Debug.Log("<color=lime>[s_ChipSpawner] CalculateChipHeight aufgerufen (Platzhalter für Board-Kompatibilität).</color>");
    }
}