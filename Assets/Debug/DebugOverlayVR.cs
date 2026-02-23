using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Management;

public class DebugOverlayVR : MonoBehaviour
{
    [Header("VR Overlay Settings")]
    public float distance = 1.0f;
    public Vector3 offset = new Vector3(0, -0.15f, 0);
    public bool faceCamera = true;
    public float updateInterval = 0.2f;

    [Header("Canvas / Text Settings")]
    public Vector2 canvasSize = new Vector2(1600, 400); // Maximale Breite
    public float canvasScale = 0.002f;
    public TMP_FontAsset font;
    public int fontSize = 8;
    public Color textColor = Color.blue;

    private Transform targetCamera;
    private TextMeshProUGUI debugText;
    private StringBuilder sb = new StringBuilder();
    private float timeSinceUpdate;

    private static DebugOverlayVR instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (!IsVRActive()) return;

        Camera[] cams = Camera.allCameras;
        foreach (Camera cam in cams)
        {
            if (cam.stereoTargetEye != UnityEngine.StereoTargetEyeMask.None)
            {
                targetCamera = cam.transform;
                break;
            }
        }

        if (targetCamera == null && Camera.main != null)
            targetCamera = Camera.main.transform;

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = null;
        canvas.GetComponent<RectTransform>().sizeDelta = canvasSize;
        canvas.transform.localScale = Vector3.one * canvasScale;

        GameObject textObj = new GameObject("DebugText");
        textObj.transform.SetParent(canvas.transform, false);
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.sizeDelta = canvasSize;

        debugText = textObj.AddComponent<TextMeshProUGUI>();
        debugText.alignment = TextAlignmentOptions.TopLeft;
        debugText.enableWordWrapping = true; // Automatisches Umbrechen bei maximaler Breite
        debugText.overflowMode = TextOverflowModes.Overflow;
        debugText.fontSize = fontSize;
        debugText.color = textColor;
        if (font != null)
            debugText.font = font;
    }

    private void LateUpdate()
    {
        if (!IsVRActive()) return;

        if (targetCamera == null)
        {
            Camera[] cams = Camera.allCameras;
            foreach (Camera cam in cams)
            {
                if (cam.stereoTargetEye != UnityEngine.StereoTargetEyeMask.None)
                {
                    targetCamera = cam.transform;
                    break;
                }
            }
            if (targetCamera == null && Camera.main != null)
                targetCamera = Camera.main.transform;
        }

        if (targetCamera != null)
        {
            transform.position = targetCamera.position + targetCamera.rotation * (Vector3.forward * distance + offset);
            if (faceCamera)
                transform.rotation = targetCamera.rotation;
        }

        timeSinceUpdate += Time.deltaTime;
        if (timeSinceUpdate >= updateInterval)
        {
            timeSinceUpdate = 0f;
            UpdateText();
        }
    }

    private void UpdateText()
    {
        if (debugText == null) return;

        sb.Length = 0;
        foreach (var kvp in DebugStorage.Entries)
        {
            sb.Append(kvp.Key).Append(": ").AppendLine(kvp.Value);
        }
        debugText.text = sb.ToString();
    }

    private bool IsVRActive()
    {
        var xrManager = XRGeneralSettings.Instance?.Manager;
        return xrManager != null && xrManager.activeLoader != null;
    }

}
