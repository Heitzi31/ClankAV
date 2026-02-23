

using System;
using System.Collections.Generic;
using UnityEngine;

// IK_toolkit: Inverse Kinematics Toolkit for UR16e robot arm
[ExecuteInEditMode]
public class IK_toolkit : MonoBehaviour
{
    public Transform ik;
    public int solutionID;
    private List<string> IK_Solutions = new List<string>();
    public List<double> goodSolution = new List<double>();
    public List<Transform> robot = new List<Transform>();

    // UR16e robot arm Denavit-Hartenberg parameters matrix
    public static double[,] DH_matrix_UR16e = new double[6, 3] {
        { 0, Mathf.PI / 2.0, 0.1807 },
        { -0.4784, 0, 0 },
        { -0.36, 0, 0 },
        { 0, Mathf.PI / 2.0, 0.17415 },
        { 0, -Mathf.PI / 2.0, 0.11985},
        { 0, 0, 0.11655}};

    void Update()
    {
        if (ik == null) return;

        // Calculate the transformation matrix for the IK
        Matrix4x4 transform_matrix = GetTransformMatrix(ik);

        // Reflect the matrix along the Y-axis (Standard transformation for this IK setup)
        Matrix4x4 mt = Matrix4x4.identity;
        mt.m11 = -1;
        Matrix4x4 mt_inverse = mt.inverse;
        Matrix4x4 result = mt * transform_matrix * mt_inverse;

        // Compute the inverse kinematics solutions
        double[,] solutions = Inverse_kinematic_solutions(result);
        IK_Solutions.Clear();
        IK_Solutions = DisplaySolutions(solutions);

        // Set the robot arm joints based on the selected solution
        if (robot.Count >= 6)
        {
            ApplyJointSolution(IK_Solutions, solutions, solutionID, robot);
        }

        // Store active solution for external access
        goodSolution.Clear();
        for (int i = 0; i < 6; i++)
        {
            goodSolution.Add(solutions[i, solutionID % 8]);
        }
    }

    public static Matrix4x4 GetTransformMatrix(Transform controller)
    {
        return Matrix4x4.TRS(controller.localPosition, Quaternion.Euler(controller.localEulerAngles), Vector3.one);
    }

    public static Matrix4x4 ComputeTransformMatrix(int jointIndex, double[,] jointAngles)
    {
        jointIndex--;

        var rotationZ = Matrix4x4.identity;
        rotationZ.m00 = Mathf.Cos((float)jointAngles[0, jointIndex]);
        rotationZ.m01 = -Mathf.Sin((float)jointAngles[0, jointIndex]);
        rotationZ.m10 = Mathf.Sin((float)jointAngles[0, jointIndex]);
        rotationZ.m11 = Mathf.Cos((float)jointAngles[0, jointIndex]);

        var translationZ = Matrix4x4.identity;
        translationZ.m23 = (float)DH_matrix_UR16e[jointIndex, 2];

        var translationX = Matrix4x4.identity;
        translationX.m03 = (float)DH_matrix_UR16e[jointIndex, 0];

        var rotationX = Matrix4x4.identity;
        rotationX.m11 = Mathf.Cos((float)DH_matrix_UR16e[jointIndex, 1]);
        rotationX.m12 = -Mathf.Sin((float)DH_matrix_UR16e[jointIndex, 1]);
        rotationX.m21 = Mathf.Sin((float)DH_matrix_UR16e[jointIndex, 1]);
        rotationX.m22 = Mathf.Cos((float)DH_matrix_UR16e[jointIndex, 1]);

        return rotationZ * translationZ * translationX * rotationX;
    }

    public static void ApplyJointSolution(List<string> solutionStatus, double[,] jointSolutions, int solutionIndex, List<Transform> robotJoints)
    {
        // Clamp solution index to valid range
        int idx = Mathf.Clamp(solutionIndex, 0, solutionStatus.Count - 1);

        if (solutionStatus[idx] != "NON DISPONIBLE")
        {
            for (int i = 0; i < robotJoints.Count; i++)
            {
                if (robotJoints[i] != null)
                    robotJoints[i].localEulerAngles = ConvertJointAngles(jointSolutions[i, idx], i);
            }
        }
        else
        {
            // Red warning if target is out of reach
            if (Time.frameCount % 100 == 0) // Don't spam every frame
                Debug.LogWarning($"<color=red>[IK_toolkit] Zielposition außerhalb der Reichweite! (Lösung {idx} nicht verfügbar)</color>");
        }
    }

    private static Vector3 ConvertJointAngles(double angleRad, int jointIndex)
    {
        float angleDeg = -(float)(Mathf.Rad2Deg * angleRad);

        switch (jointIndex)
        {
            case 1: return new Vector3(-90, 0, angleDeg);
            case 4: return new Vector3(-90, 0, angleDeg);
            case 5: return new Vector3(90, 0, angleDeg);
            default: return new Vector3(0, 0, angleDeg);
        }
    }

    public static double[,] Inverse_kinematic_solutions(Matrix4x4 transform_matrix_unity)
    {
        double[,] theta = new double[6, 8];

        // Kinematics math...
        Vector4 P05 = transform_matrix_unity * new Vector4(0, 0, -(float)DH_matrix_UR16e[5, 2], 1);
        float psi = Mathf.Atan2(P05[1], P05[0]);
        float denom = Mathf.Sqrt(Mathf.Pow(P05[0], 2) + Mathf.Pow(P05[1], 2));
        float arg = (float)((DH_matrix_UR16e[1, 2] + DH_matrix_UR16e[3, 2] + DH_matrix_UR16e[2, 2]) / denom);

        float phi = (arg <= 1 && arg >= -1) ? Mathf.Acos(arg) : 0;

        for (int i = 0; i < 8; i++) theta[0, i] = (i < 4) ? psi + phi + Mathf.PI / 2 : psi - phi + Mathf.PI / 2;

        for (int i = 0; i < 8; i += 4)
        {
            double t5 = (transform_matrix_unity[0, 3] * Mathf.Sin((float)theta[0, i]) - transform_matrix_unity[1, 3] * Mathf.Cos((float)theta[0, i]) - (DH_matrix_UR16e[1, 2] + DH_matrix_UR16e[3, 2] + DH_matrix_UR16e[2, 2])) / DH_matrix_UR16e[5, 2];
            float th5 = (t5 <= 1 && t5 >= -1) ? Mathf.Acos((float)t5) : 0;

            theta[4, i] = th5; theta[4, i + 1] = th5;
            theta[4, i + 2] = -th5; theta[4, i + 3] = -th5;
        }

        Matrix4x4 tmu_inverse = transform_matrix_unity.inverse;
        for (int i = 0; i < 8; i += 2)
        {
            float th_val = Mathf.Atan2((-tmu_inverse[1, 0] * Mathf.Sin((float)theta[0, i]) + tmu_inverse[1, 1] * Mathf.Cos((float)theta[0, i])), (tmu_inverse[0, 0] * Mathf.Sin((float)theta[0, i]) - tmu_inverse[0, 1] * Mathf.Cos((float)theta[0, i])));
            theta[5, i] = th_val; theta[5, i + 1] = th_val;
        }

        for (int i = 0; i < 8; i++)
        {
            double[,] temp = new double[1, 6];
            for (int j = 0; j < 6; j++) temp[0, j] = theta[j, i];

            Matrix4x4 T01 = ComputeTransformMatrix(1, temp);
            Matrix4x4 T45 = ComputeTransformMatrix(5, temp);
            Matrix4x4 T56 = ComputeTransformMatrix(6, temp);
            Matrix4x4 T14 = T01.inverse * transform_matrix_unity * (T45 * T56).inverse;

            Vector4 P13 = T14 * new Vector4(0, (float)-DH_matrix_UR16e[3, 2], 0, 1);
            double t3 = (Mathf.Pow(P13[0], 2) + Mathf.Pow(P13[1], 2) - Mathf.Pow((float)DH_matrix_UR16e[1, 0], 2) - Mathf.Pow((float)DH_matrix_UR16e[2, 0], 2)) / (2 * DH_matrix_UR16e[1, 0] * DH_matrix_UR16e[2, 0]);

            theta[2, i] = (t3 <= 1 && t3 >= -1) ? (i % 2 == 0 ? Mathf.Acos((float)t3) : -Mathf.Acos((float)t3)) : 0;

            theta[1, i] = Mathf.Atan2(-P13[1], -P13[0]) - Mathf.Asin((float)(-DH_matrix_UR16e[2, 0] * Mathf.Sin((float)theta[2, i]) / Mathf.Sqrt(Mathf.Pow(P13[0], 2) + Mathf.Pow(P13[1], 2))));

            temp[0, 1] = theta[1, i]; temp[0, 2] = theta[2, i];
            Matrix4x4 T32 = ComputeTransformMatrix(3, temp).inverse;
            Matrix4x4 T21 = ComputeTransformMatrix(2, temp).inverse;
            Matrix4x4 T34 = T32 * T21 * T14;
            theta[3, i] = Mathf.Atan2(T34[1, 0], T34[0, 0]);
        }
        return theta;
    }

    public static List<string> DisplaySolutions(double[,] solutions)
    {
        List<string> info = new List<string>();
        for (int col = 0; col < 8; col++)
        {
            bool isValid = true;
            for (int row = 0; row < 6; row++) if (double.IsNaN(solutions[row, col])) isValid = false;

            if (isValid)
            {
                string s = "";
                for (int row = 0; row < 6; row++) s += $"{Math.Round(Mathf.Rad2Deg * solutions[row, col], 1)}" + (row < 5 ? " | " : "");
                info.Add(s);
            }
            else info.Add("NON DISPONIBLE");
        }
        return info;
    }
}