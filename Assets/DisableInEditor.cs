using UnityEngine;

public class DisableInEditor : MonoBehaviour
{
    void Awake()
    {
#if UNITY_EDITOR
        gameObject.SetActive(false);
#endif
    }
}
