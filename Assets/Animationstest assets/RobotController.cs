















//--------------------------------------------------SCRIPT VERALTET/WIRD NICHT BENUTZT--------------------------------------------------//

















//using Oculus.Interaction;
//using System;
//using System.Runtime.CompilerServices;
//using TMPro;
//using Unity.VisualScripting;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class RobotArmController : MonoBehaviour
//{
//    [Header("Animator Referenz")]
//    [SerializeField] private Animator animator; // Für Animationen im Inspector

//    [Header("Debug")]
//    [SerializeField] private bool showDebugMessages = true; // Debug an/aus

//    [Header("ChipSpawnZuweisung")]
//    [SerializeField] public Transform[] spaltenSpawnPositionen; // Array mit den Spawn-Positionen für jede Spalte

//    private int[,] spielfeld = new int[7, 6]; // 2D Array vom Spielfeld 
//    private bool spielereinsdran; // true = Spieler 1 (rot) dran, false = Spieler 2 (gelb) dran

//    [SerializeField] private TMP_Text startspielerText; // Textfeld Startspieleranzeige im Inspector

//    [Header("Buttons")]
//    [SerializeField] private GameObject[] spaltenButtons; // Array der Spalten Buttons

//    [Header("Spielstein Settings")]
//    [SerializeField] private GameObject spielsteinPrefab; // Chip im Inspector zuweisen

//    private void Awake()
//    {

//        spielereinsdran = UnityEngine.Random.value > 0.5f; //coin flip wer startet


//        string text = spielereinsdran ? "Spieler 1 startet" : "Spieler 2 startet"; // Textanzeige wer startet
//        if (startspielerText != null)
//        {
//            startspielerText.text = text;
//            startspielerText.color = spielereinsdran ? Color.red : Color.yellow;
//        }
//    }

//    private void Start()
//    {
//        if (animator == null)
//        {
//            animator = GetComponent<Animator>();
//            if (animator == null)
//            {
//                Debug.LogError("Kein Animator gefunden! Bitte Animator Component hinzufügen.");
//            }
//        }
//    }

//    public void SpieleSpaltenAnimation(int spaltenNummer)
//    {
//        if (spaltenNummer < 1 || spaltenNummer > 7)
//        {
//            Debug.LogError($"Ungültige Spaltennummer: {spaltenNummer}. Muss zwischen 1 und 7 sein.");
//            return;
//        }

//        if (animator != null)
//        {
//            string triggerName = $"Spalte{spaltenNummer}";
//            animator.SetTrigger(triggerName);
//        }

//        SpawneSpielstein(spaltenNummer);
//    }

//    // Einzelne Methoden für jede Spalte, über Inspector an den Buttons zugewiesen
//    public void SpieleSpalte1() => SpieleSpaltenAnimation(1);
//    public void SpieleSpalte2() => SpieleSpaltenAnimation(2);
//    public void SpieleSpalte3() => SpieleSpaltenAnimation(3);
//    public void SpieleSpalte4() => SpieleSpaltenAnimation(4);
//    public void SpieleSpalte5() => SpieleSpaltenAnimation(5);
//    public void SpieleSpalte6() => SpieleSpaltenAnimation(6);
//    public void SpieleSpalte7() => SpieleSpaltenAnimation(7);

//    private int GetNaechsteFreieReihe(int spalte) //unterste freier slot in der Spalte finden
//    {
//        for (int r = 0; r < 6; r++)
//        {
//            if (spielfeld[spalte, r] == 0)
//                return r;
//        }
//        return -1;
//    }

//    public void SpawneSpielstein(int spaltenNummer)
//    {
//        if (spielsteinPrefab == null)
//        {
//            Debug.LogError("Spielstein Prefab nicht zugewiesen!");
//            return;
//        }

//        if (spaltenSpawnPositionen == null || spaltenSpawnPositionen.Length != 7)
//        {
//            Debug.LogError("Das Array 'spaltenSpawnPositionen' muss genau 7 Elemente enthalten!");
//            return;
//        }

//        int spalte = spaltenNummer - 1; //Array startet bei 0
//        int freiereihe = GetNaechsteFreieReihe(spalte); 

//        if (freiereihe == -1)
//        {
//            Debug.Log($"Spalte {spalte} ist voll!");
//            spaltenButtons[spalte].SetActive(false);
//            return;
//        }

//        int spieler = spielereinsdran ? 1 : 2;  //coin flip ergebnis setzen
//        spielfeld[spalte, freiereihe] = spieler; //speicher stein je nach Spieler

//        Vector3 spawnPos = spaltenSpawnPositionen[spaltenNummer - 1].position;
//        GameObject chip = Instantiate(spielsteinPrefab, spawnPos, spielsteinPrefab.transform.rotation);

//        Renderer renderer = chip.GetComponent<Renderer>(); // Chip je nach Spieler einfärben
//        if (spielereinsdran)
//        {
//            renderer.material.color = Color.red;
//            spielereinsdran = false;
//        }
//        else
//        {
//            renderer.material.color = Color.yellow;
//            spielereinsdran = true;
//        }

//        if (GetNaechsteFreieReihe(spalte) == -1) //wenn spalte voll button ausblenden
//        {
//            spaltenButtons[spalte].SetActive(false);
//        }


//        if (PrüfeGewinner(spalte, freiereihe, spieler))
//        {
//            if (startspielerText != null)
//            {
//                startspielerText.text = (spieler == 1 ? "Spieler 1 gewinnt!" : "Spieler 2 gewinnt!");
//                startspielerText.color = (spieler == 1 ? Color.red : Color.yellow);
//            }

//            foreach (var btn in spaltenButtons) btn.SetActive(false); // alle buttons aus wenn fertig
//            return;
//        }

//        if (PrüfeUnentschieden())
//        {
//            if (startspielerText != null)
//            {
//                startspielerText.text = "Unentschieden!";
//                startspielerText.color = Color.green;
//            }
//            foreach (var btn in spaltenButtons) btn.SetActive(false);
//            return;
//        }

//        if (showDebugMessages)
//            Debug.Log($"Spielstein gespawnt bei {spawnPos} (Spalte {spaltenNummer})");
//    }


//    private bool PrüfeGewinner(int spalte, int reihe, int spieler)
//    {
//        int[][] richtungen = new int[][]
//        {
//            new int[]{1,0},   // horizontal
//            new int[]{0,1},   // vertikal
//            new int[]{1,1},   // diagonal /
//            new int[]{1,-1}   // diagonal \
//        };

//        foreach (var dir in richtungen)
//        {
//            int count = 1;
//            count += ZähleInRichtung(spalte, reihe, dir[0], dir[1], spieler);
//            count += ZähleInRichtung(spalte, reihe, -dir[0], -dir[1], spieler);
//            if (count >= 4) return true;
//        }
//        return false;
//    }

//    private int ZähleInRichtung(int startSpalte, int startReihe, int dx, int dy, int spieler)
//    {
//        int count = 0;
//        int x = startSpalte + dx;
//        int y = startReihe + dy;
//        while (x >= 0 && x < 7 && y >= 0 && y < 6 && spielfeld[x, y] == spieler)
//        {
//            count++;
//            x += dx;
//            y += dy;
//        }
//        return count;
//    }

//    private bool PrüfeUnentschieden()
//    {
//        for (int s = 0; s < 7; s++)
//            for (int r = 0; r < 6; r++)
//                if (spielfeld[s, r] == 0)
//                    return false;
//        return true;
//    }

//    public void Neustart()
//    {
//        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
//    }
//}
