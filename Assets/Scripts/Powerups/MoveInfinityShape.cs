using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveInfinityShape : MonoBehaviour
{
    public float moveSpeed = 10;
    public float shapeScale = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float t = Time.time * moveSpeed;
        var scale = shapeScale / (3 - Mathf.Cos(2 * t));
        float x = scale * Mathf.Cos(t);
        float y = scale * Mathf.Sin(t * 2) / 2;
        transform.localPosition = new Vector2(x, y);
    }
}
