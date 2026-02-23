

using UnityEngine;

public class RoboInfos : MonoBehaviour
{
    [Header("UI Zuweisung")]
    [Tooltip("Zieh hier das World Space Canvas rein, das neben dem Robo schwebt.")]
    public GameObject infoCanvas;

    private void Awake()
    {
        // Sicherstellen, dass die Info beim Spielstart unsichtbar ist
        if (infoCanvas != null)
        {
            infoCanvas.SetActive(false);
            Debug.Log($"<color=lime>[RoboInfos - {gameObject.name}] Initialisiert: InfoCanvas wurde standardm‰ﬂig deaktiviert.</color>");
        }
        else
        {
            Debug.LogWarning($"<color=orange>[RoboInfos - {gameObject.name}] Warnung: Kein infoCanvas im Inspector zugewiesen!</color>");
        }
    }

    /// <summary>
    /// Wird vom Unity Event Wrapper aufgerufen (When Hover)
    /// </summary>
    public void ShowInfo()
    {
        if (infoCanvas != null)
        {
            infoCanvas.SetActive(true);
            Debug.Log($"<color=lime>[RoboInfos - {gameObject.name}] Event: ShowInfo aufgerufen. Anzeige ist jetzt SICHTBAR.</color>");
        }
    }

    /// <summary>
    /// Wird vom Unity Event Wrapper aufgerufen (When Unhover)
    /// </summary>
    public void HideInfo()
    {
        if (infoCanvas != null)
        {
            infoCanvas.SetActive(false);
            Debug.Log($"<color=lime>[RoboInfos - {gameObject.name}] Event: HideInfo aufgerufen. Anzeige ist jetzt VERSTECKT.</color>");
        }
    }
}