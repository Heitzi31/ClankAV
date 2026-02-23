
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections.Generic;

public class s_Board : MonoBehaviour
{
    [Header("Spielfeld-Status")]
    private bool isGameStarted = false;
    private bool isProcessingMove = false;
    private bool playerOneActive;

    [Header("Spielfeldgeometrie")]
    public int boardWidth = 7;
    public int boardHeight = 6;
    public float chipSpacing;
    public float chipDropPosOffset;

    [Header("Zuweisungen")]
    [SerializeField] public Transform[] columnSpawnPositions;
    [SerializeField] private GameObject[] columnButtonsP1;
    [SerializeField] private GameObject[] columnButtonsP2;
    [SerializeField] private TMP_Text statusText;

    [Header("Spieler-Ressourcen")]
    public Material colorTeamOne;
    public Material colorTeamTwo;
    public Material colorNeutral;

    [Header("Roboter (werden dynamisch zugewiesen)")]
    public s_moveRobot roboterTeamOne;
    public s_moveRobot roboterTeamTwo;

    private int[,] board = new int[7, 6];
    private List<GameObject> chips = new List<GameObject>();
    private int activeColumn;
    private int currentRow;

    public GameObject[] pickerRegale;

    [Header("Chip Spawning")]
    public s_ChipSpawner chipSpawner;
    public Transform spawnPosTeamOne;
    public Transform spawnPosTeamTwo;
    public int chipsPerStack = 22;

    [Header("Spieler Namen")]
    public string nameP1 = "Spieler 1";
    public string nameP2 = "Spieler 2";
    public bool waitingForNames = false;

    private void Awake()
    {
        SetAllButtonsActive(false);
        if (statusText != null)
        {
            statusText.text = "Wählt eure Roboter am Regal!";
            statusText.color = colorNeutral.color;
        }
        Debug.Log("<color=lime>[s_Board] System bereit. Warte auf Roboter-Auswahl...</color>");
    }

   

    public void CheckIfBothReady()
    {
        Debug.Log($"<color=lime>[s_Board] CheckIfBothReady: P1={roboterTeamOne?.name ?? "FEHLT"}, P2={roboterTeamTwo?.name ?? "FEHLT"}</color>");
        if (roboterTeamOne != null && roboterTeamTwo != null && !isGameStarted && !waitingForNames)
        {
            waitingForNames = true;
            StartNameInputSequence();
        }
    }

    private void StartNameInputSequence()
    {
        // Deaktiviere das Regal, damit keiner mehr rumklickt
        if (pickerRegale != null)
            foreach (GameObject regal in pickerRegale) if (regal != null) regal.SetActive(false);

        statusText.text = "<color=red>Spieler 1: Name eingeben!</color>";

        // Ruft dein Keyboard-Skript auf
        FindObjectOfType<KeyboardTest>().OpenKeyboardForPlayer(1);
    }

    public void OnNameInputFinished(int playerNum, string enteredName)
    {
        // Falls leer, Standardnamen behalten
        string finalName = string.IsNullOrEmpty(enteredName) ? (playerNum == 1 ? "Spieler 1" : "Spieler 2") : enteredName;

        if (playerNum == 1)
        {
            nameP1 = finalName;
            statusText.text = "<color=yellow>Spieler 2: Name eingeben!</color>";
            FindObjectOfType<KeyboardTest>().OpenKeyboardForPlayer(2);
        }
        else
        {
            nameP2 = finalName;
            waitingForNames = false;
            StartGameSequence(); // JETZT erst geht es los!
        }
    }

    private void StartGameSequence()
    {
        isGameStarted = true;
        playerOneActive = UnityEngine.Random.value > 0.5f;
        Debug.Log($"<color=lime>[s_Board] Spiel gestartet! Erster Spieler: {(playerOneActive ? "Spieler 1 (Rot)" : "Spieler 2 (Gelb)")}</color>");

        UpdateStatusUI();
        RefreshVisibleButtons();
    }

    public void InputPlayerOne(int columnNumber) => MakeAMove(columnNumber, true);
    public void InputPlayerTwo(int columnNumber) => MakeAMove(columnNumber, false);

    private void MakeAMove(int columnNumber, bool requesterIsPlayerOne)
    {
        if (!isGameStarted || isProcessingMove) return;

        if (requesterIsPlayerOne != playerOneActive)
        {
            Debug.Log("<color=orange>[s_Board] Input ignoriert: Nicht dein Zug!</color>");
            return;
        }

        int targetRow = GetLowestRow(columnNumber);
        if (targetRow == -1)
        {
            Debug.Log($"<color=orange>[s_Board] Spalte {columnNumber} ist voll!</color>");
            return;
        }

        isProcessingMove = true;
        activeColumn = columnNumber;
        int player = playerOneActive ? 1 : 2;
        board[columnNumber, targetRow] = player;

        Debug.Log($"<color=lime>[s_Board] ZUG AUSGEFÜHRT: Spieler {player} in Spalte {columnNumber}, Reihe {targetRow}</color>");

        RowAnimation(columnNumber);

        if (targetRow >= boardHeight - 1)
        {
            columnButtonsP1[columnNumber].SetActive(false);
            columnButtonsP2[columnNumber].SetActive(false);
        }

        if (CheckWin(columnNumber, targetRow, player))
        {
            Debug.Log($"<color=lime>[s_Board] SIEG GEFUNDEN für Spieler {player}!</color>");
            WinProcedure(player);
            return;
        }

        if (CheckDraw())
        {
            Debug.Log("<color=lime>[s_Board] UNENTSCHIEDEN! Das Brett ist voll.</color>");
            DrawProcedure();
            return;
        }

        playerOneActive = !playerOneActive;
        Debug.Log($"<color=lime>[s_Board] Zug beendet. Nächster Spieler: {(playerOneActive ? "P1" : "P2")}. Sperre für 6s (Robo-Fahrt)...</color>");
        Invoke("UnlockInput", 6f);
    }

    private void RowAnimation(int columnNumber)
    {
        Material currentMat = playerOneActive ? colorTeamOne : colorTeamTwo;
        s_moveRobot currentRobo = playerOneActive ? roboterTeamOne : roboterTeamTwo;

        if (currentRobo != null)
        {
            Debug.Log($"<color=lime>[s_Board] Befehl an {currentRobo.name}: Fahre zu Spalte {columnNumber}.</color>");
            currentRobo.moveObjectToPosition(columnSpawnPositions[columnNumber].position, currentMat);
        }
    }

    private void UnlockInput()
    {
        isProcessingMove = false;
        Debug.Log("<color=lime>[s_Board] Input wieder freigegeben.</color>");
        UpdateStatusUI();
        RefreshVisibleButtons();
    }

    private void RefreshVisibleButtons()
    {
        for (int i = 0; i < boardWidth; i++)
        {
            bool isColumnFree = GetLowestRow(i) != -1;
            columnButtonsP1[i].SetActive(playerOneActive && isColumnFree && isGameStarted);
            columnButtonsP2[i].SetActive(!playerOneActive && isColumnFree && isGameStarted);
        }
    }


    private void UpdateStatusUI()
    {
        if (statusText == null) return;

        // Wenn das Spiel nicht läuft UND wir nicht gerade Namen eingeben
        if (!isGameStarted && !waitingForNames)
        {
            statusText.text = "<color=green>Wählt eure Roboter am Regal!</color>";
            statusText.color = Color.white; // Oder colorNeutral.color
            return;
        }

        // Während der Namenseingabe (wird von OnNameInputFinished gesteuert, 
        // daher hier nur Sicherheits-Check oder leer lassen)
        if (waitingForNames) return;

        // Während des Spiels
        string activeName = playerOneActive ? nameP1 : nameP2;
        statusText.text = activeName + " am Zug";
        statusText.color = playerOneActive ? colorTeamOne.color : colorTeamTwo.color;
    }

    private void SetAllButtonsActive(bool state)
    {
        foreach (var btn in columnButtonsP1) btn.SetActive(state);
        foreach (var btn in columnButtonsP2) btn.SetActive(state);
    }

    private int GetLowestRow(int column)
    {
        for (int r = 0; r < boardHeight; r++)
        {
            if (board[column, r] == 0)
            {
                currentRow = r;
                return r;
            }
        }
        return -1;
    }

    private bool CheckWin(int column, int row, int player)
    {
        int[][] directions = { new[] { 1, 0 }, new[] { 0, 1 }, new[] { 1, 1 }, new[] { 1, -1 } };
        foreach (var dir in directions)
        {
            int count = 1 + CountInDirection(column, row, dir[0], dir[1], player)
                          + CountInDirection(column, row, -dir[0], -dir[1], player);
            if (count >= 4) return true;
        }
        return false;
    }

    private int CountInDirection(int startC, int startR, int dx, int dy, int player)
    {
        int count = 0;
        int x = startC + dx;
        int y = startR + dy;
        while (x >= 0 && x < boardWidth && y >= 0 && y < boardHeight && board[x, y] == player)
        {
            count++; x += dx; y += dy;
        }
        return count;
    }

    private bool CheckDraw()
    {
        for (int c = 0; c < boardWidth; c++)
            if (board[c, boardHeight - 1] == 0) return false;
        return true;
    }

    private void WinProcedure(int player)
    {
        isGameStarted = false;
        Debug.Log($"<color=lime>[s_Board] WinProcedure gestartet für Spieler {player}.</color>");

        if (statusText != null)
        {
            string winnerName = (player == 1) ? nameP1 : nameP2;
            statusText.text = winnerName + " gewinnt!";
            statusText.color = (player == 1) ? colorTeamOne.color : colorTeamTwo.color;
        }

        SetAllButtonsActive(false);
        Invoke("ResetBoard", 8f);
    }

    private void DrawProcedure()
    {
        isGameStarted = false;
        Debug.Log("<color=lime>[s_Board] DrawProcedure gestartet.</color>");
        if (statusText != null)
        {
            statusText.text = "Unentschieden!";
            statusText.color = colorNeutral.color;
        }
        SetAllButtonsActive(false);
        Invoke("ResetBoard", 8f);
    }

    public Tween InsertChip(GameObject chip)
    {
        int row = currentRow;
        Debug.Log($"<color=lime>[s_Board] Animation: Chip wird in Reihe {row} fallen gelassen.</color>");

        chips.Add(chip);
        Vector3 startPos = columnSpawnPositions[activeColumn].position;
        float goalPosY = startPos.y - (chipSpacing * (boardHeight - 1 - row)) - chipDropPosOffset;
        Vector3 goalPos = new Vector3(startPos.x, goalPosY, startPos.z);

        float distance = startPos.y - goalPos.y;
        float duration = Mathf.Sqrt((2 * distance) / 9.81f);

        return chip.transform.DOMove(goalPos, duration).SetEase(Ease.InQuad);
    }

    public void ResetBoard()
    {
        Debug.Log("<color=lime>[s_Board] RESET: Spielfeld wird vollständig zurückgesetzt...</color>");
        System.Array.Clear(board, 0, board.Length);
        foreach (GameObject chip in chips) { if (chip != null) Destroy(chip); }
        chips.Clear();

        if (chipSpawner != null) chipSpawner.RespawnAllChips();

        s_moveRobot[] allRobos = UnityEngine.Object.FindObjectsByType<s_moveRobot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (s_moveRobot robo in allRobos)
        {
            if (robo != null)
            {
                robo.Invoke("findGrabableObjects", 0.5f);
                if (robo.transform.parent != null && (robo.transform.parent.name.Contains("Panda") || robo.transform.parent.name.Contains("UR16")))
                    robo.transform.parent.gameObject.SetActive(false);
                else
                    robo.gameObject.SetActive(false);
            }
        }

        roboterTeamOne = null;
        roboterTeamTwo = null;
        if (pickerRegale != null)
        {
            foreach (GameObject regal in pickerRegale) if (regal != null) regal.SetActive(true);
        }

        isGameStarted = false;
        isProcessingMove = false;
        waitingForNames = false;
        nameP1 = "Spieler 1"; //eigentlich optional, sollten eh bei neuer Runde neu eingegeben werden, aber so ist besser bei Problem
        nameP2 = "Spieler 2"; //eigentlich optional, sollten eh bei neuer Runde neu eingegeben werden, aber so ist besser bei Problem
        UpdateStatusUI();
        SetAllButtonsActive(false);
        Debug.Log("<color=lime>[s_Board] RESET abgeschlossen. Bereit für neue Runde.</color>");
    }

    public bool GetActivePlayer() => playerOneActive;
}