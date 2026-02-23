using System.Collections;
using TMPro;
using UnityEngine;

public class LoadingText : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    private int status = 0;

    void Start()
    {
        StartCoroutine(Loading());
    }

    private IEnumerator Loading()
    {
        while (true)
        {
            switch (status)
            {
                case 0:
                    textMeshPro.text = "Loading";
                    break;
                case 1:
                    textMeshPro.text = "Loading.";
                    break;
                case 2:
                    textMeshPro.text = "Loading..";
                    break;
                case 3:
                    textMeshPro.text = "Loading...";
                    break;
            }

            status = (status + 1) % 4;
            yield return new WaitForSeconds(0.25f);
        }
    }
}
