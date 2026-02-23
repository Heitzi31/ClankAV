using UnityEngine;

public class DebugOverlayDesktop : MonoBehaviour
{
    public Vector2 startPosition = new Vector2(120, 60);
    public float lineHeight = 20f;
    public int fontSize = 14;
    public Color textColor = Color.white;
    public bool background = true;
    public Color backgroundColor = new Color(0, 0, 0, 0.5f);

    private GUIStyle style;

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Awake()
    {
        style = new GUIStyle { fontSize = fontSize };
        style.normal.textColor = textColor;
    }

    void OnGUI()
    {
        if (DebugStorage.Entries.Count == 0) return;

        int i = 0;
        foreach (var kvp in DebugStorage.Entries)
        {
            float y = startPosition.y + i * lineHeight;

            string text = $"{kvp.Key}: {kvp.Value}";
            Vector2 size = style.CalcSize(new GUIContent(text));
            Rect bgRect = new Rect(startPosition.x - 5, y - 2, size.x + 10, lineHeight);

            if (background)
            {
                Color oldColor = GUI.color;
                GUI.color = backgroundColor;
                GUI.Box(bgRect, GUIContent.none);
                GUI.color = oldColor;
            }

            GUI.Label(new Rect(startPosition.x, y, size.x, lineHeight), text, style);
            i++;
        }
    }
}
