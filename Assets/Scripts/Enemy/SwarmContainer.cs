using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmContainer : Enemy
{
    public float maxRotateAmountPerFrame = 45;
    public float rotateAmountPerFrame = 0;
    Rigidbody2D rb2d;

    protected override void Start()
    {
        base.Start();
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        //transform.Rotate(new Vector3(0, 0, rotateAmountPerFrame * Time.deltaTime));
    }

    public override bool DamageSelf(float damage, Vector2 hitPosition, Vector2? bulletDirection)
    {
        if (bulletDirection != null)
        {
            float dst = transform.position.y - hitPosition.y;
            rb2d.AddForceAtPosition((Vector2)bulletDirection * 5, hitPosition);
        }
        return true;
    }
}
