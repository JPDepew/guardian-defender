using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmContainer : Enemy
{
    float rotateAmountPerFrame = 0;

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        transform.Rotate(new Vector3(0, 0, rotateAmountPerFrame * Time.deltaTime));
    }

    public override bool DamageSelf(float damage, Vector2 hitPosition)
    {
        float dst = transform.position.y - hitPosition.y;
        print(dst);
        rotateAmountPerFrame += dst;
        return true;
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.tag == "Bullet")
    //    {
    //        float dst = collision.transform.position.y - transform.position.y;
    //        print(dst);
    //    }
    //}
}
