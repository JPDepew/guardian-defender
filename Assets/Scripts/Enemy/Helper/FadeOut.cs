using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOut : MonoBehaviour {

    TextMesh textMesh;

    public float fadeSpeed = 0.05f;
    public float initialWaitTime = 0.5f;

    private void Start()
    {
        textMesh = GetComponent<TextMesh>();
        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(initialWaitTime);
        while(textMesh.color.a > 0.01f)
        {
            textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, textMesh.color.a - fadeSpeed);
            yield return new WaitForSeconds(0.01f);
        }
        Destroy(gameObject);
    }
}
