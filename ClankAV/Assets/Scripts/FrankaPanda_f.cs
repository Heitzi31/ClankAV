
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class FrankaPanda_f : MonoBehaviour
{
    [DllImport("FrankaPandaDLL", EntryPoint = "?franka_IK@@YAXPEAN0N0_N1@Z")]
    public static extern void franka_IK([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] double[] q, double[] TF, double q7, double[] q0, bool limit, bool flange);

    [DllImport("FrankaPandaDLL", EntryPoint = "?franka_IK_f@@YAXPEAM0M0_N1@Z")]
    public static extern void franka_IK_f([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] float[] q, float[] TF, float q7, float[] q0, bool limit, bool flange);

    public GameObject Base;
    public GameObject J1;
    public GameObject J2;
    public GameObject J3;
    public GameObject J4;
    public GameObject J5;
    public GameObject J6;
    public GameObject J7;
    public GameObject TCP;
    public GameObject finger1;
    public GameObject finger2;

    [Header("Ziel (IK Target)")]
    public GameObject target;

    float q1_start = 56.0f, q2_start = 89.8f, q3_start = -113.6f, q4_start = -179.6f, q5_start = 156.4f, q6_start = 56.3f, q7_start = 45.0f;

    [Header("Joint Angles (Pose)")]
    public float[] q;

    float[] qq = new float[7];
    float[] qMax;
    float[] qMin;
    float finger_pos;

    // Timer für gedrosseltes Logging
    private float nextLogTime = 0f;

    void Start()
    {
        if (J1 == null || target == null)
        {
            Debug.LogError($"<color=red>[IK-Visuals - {gameObject.name}] FEHLER: Gelenke oder Target fehlen!</color>");
        }
        else
        {
            Debug.Log($"<color=lime>[IK-Visuals - {gameObject.name}] Initialisierung erfolgreich. Verfolge Target: {target.name}</color>");
        }

        if (q == null || q.Length != 7)
        {
            q = new float[] { q1_start, q2_start, q3_start, q4_start, q5_start, q6_start, q7_start };
            Debug.Log($"<color=lime>[IK-Visuals - {gameObject.name}] Start-Pose auf Default-Werte gesetzt.</color>");
        }

        qMax = new float[] { 166.003062f, 101.0010001f, 166.003062f, -3.99925f, 166.0031f, 215.0024f, 166.0031f };
        qMin = new float[] { -166.003062f, -101.0010001f, -166.003062f, -176.0012f, -166.0031f, -1.00268f, -166.0031f };
        finger_pos = 0.03f;

        ApplyPose();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

        if (target == null || Base == null || J1 == null)
        {
            Debug.LogWarning($"<color=orange>[IK-Visuals - {gameObject.name}] Suche Komponenten erneut...</color>");
            Start();
            return;
        }

        float q7 = 45 * Mathf.Deg2Rad;
        float[] q0 = { q[0] * Mathf.Deg2Rad, q[1] * Mathf.Deg2Rad, q[2] * Mathf.Deg2Rad, q[3] * Mathf.Deg2Rad, q[4] * Mathf.Deg2Rad, q[5] * Mathf.Deg2Rad, q[6] * Mathf.Deg2Rad };

        Matrix4x4 lh_m = Base.transform.worldToLocalMatrix * target.transform.localToWorldMatrix;
        Quaternion lh_quat = lh_m.rotation;
        Quaternion rh_quat = new Quaternion(lh_quat.x, -lh_quat.y, lh_quat.z, -lh_quat.w);
        Matrix4x4 matrix = Matrix4x4.identity;
        matrix.SetTRS(new Vector3(lh_m[0, 3], -lh_m[1, 3], lh_m[2, 3]), rh_quat, Vector3.one);

        float[] TF = { matrix[0], matrix[1], matrix[2], matrix[3], matrix[4], matrix[5], matrix[6], matrix[7], matrix[8], matrix[9], matrix[10], matrix[11], matrix[12], matrix[13], matrix[14], matrix[15] };

        // DLL Aufruf
        franka_IK_f(qq, TF, q7, q0, false, false);

        bool hasNaN = false;
        for (int i = 0; i < 7; i++)
        {
            float val = qq[i] * Mathf.Rad2Deg;
            if (!float.IsNaN(val))
            {
                q[i] = val;
            }
            else
            {
                hasNaN = true;
            }
        }

        if (hasNaN && Time.time > nextLogTime)
        {
            Debug.LogError($"<color=red>[IK-Visuals - {gameObject.name}] DLL liefert NaN! Ziel-Rotation evtl. unmöglich.</color>");
            nextLogTime = Time.time + 1f; // Fehler-Spam verhindern
        }
        else if (Time.time > nextLogTime)
        {
            //Debug.Log($"<color=lime>[IK-Visuals - {gameObject.name}] IK-Status: OK. Aktueller J1 Winkel: {q[0]:F2}°</color>");
            nextLogTime = Time.time + 2f; // Alle 2 Sekunden ein Lebenszeichen
        }

        ApplyPose();
    }

    void ApplyPose()
    {
        if (J1 == null) return;

        J1.transform.localEulerAngles = new Vector3(0, 0, -q[0]);
        J2.transform.localEulerAngles = new Vector3(90, 0, -q[1]);
        J3.transform.localEulerAngles = new Vector3(-90, 0, -q[2]);
        J4.transform.localEulerAngles = new Vector3(-90, 0, -q[3]);
        J5.transform.localEulerAngles = new Vector3(90, 0, -q[4]);
        J6.transform.localEulerAngles = new Vector3(-90, 0, -q[5]);
        J7.transform.localEulerAngles = new Vector3(-90, 0, -q[6]);

        if (finger1 != null) finger1.transform.localPosition = new Vector3(0, finger_pos, -0.0448f);
        if (finger2 != null) finger2.transform.localPosition = new Vector3(0, -finger_pos, -0.0448f);
    }
}