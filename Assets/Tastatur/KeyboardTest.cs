using UnityEngine;
using TMPro;

public class KeyboardTest : MonoBehaviour
{
    [Header("UI Zuweisung")]
    [SerializeField] private TMP_Text displayText; 
    [SerializeField] private GameObject inputCanvas; 

    private TouchScreenKeyboard keyboard;
    private s_Board board;
    private int currentPlayerNum = 1;

    void Start()
    {
        board = FindObjectOfType<s_Board>();
        if (inputCanvas != null) inputCanvas.SetActive(false); // Am Anfang unsichtbar
    }

    public void OpenKeyboardForPlayer(int playerNum)
    {
        currentPlayerNum = playerNum;
        if (inputCanvas != null) inputCanvas.SetActive(true); // Canvas anzeigen

        // Tastatur öffnen
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);

        Debug.Log($"[Keyboard] Gestartet für P{playerNum}.");
    }

    void Update()
    {
        if (board == null) return;

        // --- PC/EDITOR TEST LOGIK ---
#if UNITY_EDITOR
        if (board.waitingForNames && Input.GetKeyDown(KeyCode.Return))
        {
            FinishInput("PC_Spieler_" + currentPlayerNum);
            return;
        }
#endif

        // --- LIVE VORSCHAU & NATIVE LOGIK ---
        if (keyboard == null) return;

        // Text live im Canvas anzeigen (das ist dein gewünschtes Feature!)
        if (displayText != null)
        {
            displayText.text = keyboard.text;
        }

        // Wenn fertig
        if (keyboard.status == TouchScreenKeyboard.Status.Done)
        {
            FinishInput(keyboard.text);
        }
        else if (keyboard.status == TouchScreenKeyboard.Status.Canceled)
        {
            FinishInput(""); // Oder alten Namen behalten
        }
    }

    private void FinishInput(string result)
    {
        board.OnNameInputFinished(currentPlayerNum, result);
        keyboard = null;
        if (displayText != null) displayText.text = ""; // Textfeld leeren
        if (inputCanvas != null) inputCanvas.SetActive(false); // Canvas wieder verstecken
    }
}