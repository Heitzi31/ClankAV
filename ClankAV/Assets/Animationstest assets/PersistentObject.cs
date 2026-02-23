using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    // Statische Variable, um die existierende Instanz zu speichern
    public static PersistentObject Instance;

    private void Awake()
    {
        // Prüfen, ob es bereits eine Instanz dieser Klasse gibt
        if (Instance == null)
        {
            // Wenn nicht: Ich bin das Original!
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Wenn es schon eine gibt: Ich bin ein Duplikat (z.B. durch Szenen-Neustart)
            // Also vernichte ich mich selbst sofort.
            Destroy(gameObject);
        }
    }
}