using UnityEngine;
using System.Collections.Generic;

public class StanchionGroup : MonoBehaviour
{
    [Header("Laser Settings")]
    public Material laserMaterial;
    [Range(0.01f, 0.2f)]
    public float laserThickness = 0.05f;
    public float laserHeightOffset = 1.0f;

    [Header("Pulse Settings")]
    public bool pulse = true;
    public float pulseSpeed = 2f;
    public float pulseIntensity = 0.5f;
    public float thicknessPulse = 0.01f;

    private readonly List<GameObject> lasers = new();
    private readonly List<Transform> stanchions = new();

    private MaterialPropertyBlock mpb;
    private Color baseEmission;

    // ---------------- UNITY ----------------

    void OnEnable()
    {
        if (!Application.isPlaying)
            return;

        mpb = new MaterialPropertyBlock();

        if (laserMaterial != null && laserMaterial.HasProperty("_EmissionColor"))
            baseEmission = laserMaterial.GetColor("_EmissionColor");

        Rebuild();
    }

    void OnDisable()
    {
        if (!Application.isPlaying)
            return;

        ClearLasers();
    }

    void LateUpdate()
    {
        if (!Application.isPlaying)
            return;

        UpdateLasers();
        UpdatePulse();
    }

    // ---------------- CORE ----------------

    public void Rebuild()
    {
        CollectStanchions();
        ClearLasers();

        if (laserMaterial == null || stanchions.Count < 2)
            return;

        for (int i = 0; i < stanchions.Count - 1; i++)
        {
            GameObject laser = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            laser.name = "Laser";

            laser.transform.SetParent(transform, false);
            Destroy(laser.GetComponent<Collider>());

            Renderer r = laser.GetComponent<Renderer>();
            r.sharedMaterial = laserMaterial;

            lasers.Add(laser);
        }

        UpdateLasers();
    }

    void CollectStanchions()
    {
        stanchions.Clear();

        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy)
                stanchions.Add(child);
        }
    }

    void UpdateLasers()
    {
        if (lasers.Count == 0 || stanchions.Count < 2)
            return;

        for (int i = 0; i < lasers.Count; i++)
        {
            Transform a = stanchions[i];
            Transform b = stanchions[i + 1];
            Transform laser = lasers[i].transform;

            Vector3 start = a.position + Vector3.up * laserHeightOffset;
            Vector3 end = b.position + Vector3.up * laserHeightOffset;

            Vector3 dir = end - start;
            Vector3 mid = start + dir * 0.5f;

            laser.position = mid;
            laser.up = dir.normalized;

            laser.localScale = new Vector3(
                laserThickness,
                dir.magnitude * 0.5f,
                laserThickness
            );
        }
    }

    // ---------------- PULSE ----------------

    void UpdatePulse()
    {
        if (!pulse || lasers.Count == 0 || mpb == null)
            return;

        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float emissionMul = 1f + t * pulseIntensity;
        float thicknessMul = 1f + t * thicknessPulse;

        Color emission = baseEmission * emissionMul;
        mpb.SetColor("_EmissionColor", emission);

        foreach (var laser in lasers)
        {
            if (laser == null) continue;

            Renderer r = laser.GetComponent<Renderer>();
            r.SetPropertyBlock(mpb);

            Vector3 s = laser.transform.localScale;
            s.x = laserThickness * thicknessMul;
            s.z = laserThickness * thicknessMul;
            laser.transform.localScale = s;
        }
    }

    // ---------------- CLEANUP ----------------

    void ClearLasers()
    {
        foreach (var l in lasers)
            if (l != null)
                Destroy(l);

        lasers.Clear();
    }

    // ---------------- EDITOR VISUAL ----------------

    void OnDrawGizmos()
    {
        stanchions.Clear();

        foreach (Transform child in transform)
            if (child.gameObject.activeInHierarchy)
                stanchions.Add(child);

        Gizmos.color = Color.red;

        for (int i = 0; i < stanchions.Count - 1; i++)
        {
            Vector3 a = stanchions[i].position + Vector3.up * laserHeightOffset;
            Vector3 b = stanchions[i + 1].position + Vector3.up * laserHeightOffset;
            Gizmos.DrawLine(a, b);
        }
    }
}
