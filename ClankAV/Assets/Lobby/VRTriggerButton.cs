using UnityEngine;

public class VRTriggerButton : MonoBehaviour
{
    [Header("Was soll passieren, wenn jemand durchläuft?")]
    public UnityEngine.Events.UnityEvent onTriggered;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[VRTriggerButton] Getriggert von: {other.name}");
        DebugStorage.Log("Created Room", "");
        onTriggered?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[VRTriggerButton] Verlassen von: {other.name}");
    }
}
