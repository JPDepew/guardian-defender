using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteMaskFadeIn : MonoBehaviour
{
    public float fadeSpeed = 0.05f;
    SpriteMask spriteMask;
    private void Start()
    {
        spriteMask = GetComponent<SpriteMask>();
    }
    void Update()
    {
        spriteMask.alphaCutoff -= fadeSpeed;
        if(spriteMask.alphaCutoff <= 0.01f)
        {
            Destroy(gameObject);
        }
    }
}
