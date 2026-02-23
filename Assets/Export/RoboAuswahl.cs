

using UnityEngine;

public class s_RoboPicker : MonoBehaviour
{
    [Header("Zentrale Steuerung")]
    public s_Board boardScript;
    public int teamNumber;

    [Header("DIREKTE ZUWEISUNG (Zieh das 'IK-MOVE' Objekt hier rein)")]
    public s_moveRobot moverA;
    public s_moveRobot moverB;
    public s_moveRobot moverC;

    public void Select_A()
    {
        Debug.Log($"<color=lime>[s_RoboPicker - Team {teamNumber}] Button A geklickt. Versuche Mover A zu wählen.</color>");
        FinalizeSelection(moverA);
    }

    public void Select_B()
    {
        Debug.Log($"<color=lime>[s_RoboPicker - Team {teamNumber}] Button B geklickt. Versuche Mover B zu wählen.</color>");
        FinalizeSelection(moverB);
    }

    public void Select_C()
    {
        Debug.Log($"<color=lime>[s_RoboPicker - Team {teamNumber}] Button C geklickt. Versuche Mover C zu wählen.</color>");
        FinalizeSelection(moverC);
    }

    private void FinalizeSelection(s_moveRobot selectedMover)
    {
        if (selectedMover == null || boardScript == null)
        {
            Debug.LogError($"<color=red>[s_RoboPicker - Team {teamNumber}] FEHLER: Zuweisung fehlt im Inspector! Mover oder BoardScript ist NULL.</color>");
            return;
        }

        Debug.Log($"<color=lime>[s_RoboPicker] Auswahl bestätigt: {selectedMover.gameObject.name} wird Team {teamNumber} zugewiesen.</color>");

        // 1. Das Objekt aktivieren, auf dem der Mover sitzt
        selectedMover.gameObject.SetActive(true);
        Debug.Log($"<color=lime>[s_RoboPicker] Aktiviere Mover-Objekt: {selectedMover.gameObject.name}</color>");

        // Falls das Modell ein Parent hat (wie Panda_Base), aktiviere das auch:
        if (selectedMover.transform.parent != null)
        {
            selectedMover.transform.parent.gameObject.SetActive(true);
            Debug.Log($"<color=lime>[s_RoboPicker] Aktiviere Parent-Objekt: {selectedMover.transform.parent.name}</color>");
        }

        // 2. Daten ans Board
        if (teamNumber == 1)
        {
            boardScript.roboterTeamOne = selectedMover;
            Debug.Log("<color=lime>[s_RoboPicker] Board-Update: Roboter für Team 1 registriert.</color>");
        }
        else
        {
            boardScript.roboterTeamTwo = selectedMover;
            Debug.Log("<color=lime>[s_RoboPicker] Board-Update: Roboter für Team 2 registriert.</color>");
        }

        // 3. Board informieren und Regal aus
        Debug.Log("<color=lime>[s_RoboPicker] Rufe CheckIfBothReady() am Board auf und schalte Auswahl-Regal aus.</color>");
        boardScript.CheckIfBothReady();
        this.gameObject.SetActive(false);
    }
}