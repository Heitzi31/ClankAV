using System;
using UnityEngine;

public class CubeTestScript : MonoBehaviour
{
    double value = 0;
    Vector3 currenPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currenPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = currenPos + new Vector3((float)((double)currenPos.x + Math.Sin(value)), (float)((double)currenPos.y + Math.Sin(value)), (float)((double)currenPos.z + Math.Sin(value)));
        value += Time.deltaTime;
    }
}
