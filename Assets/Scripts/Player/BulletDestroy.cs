using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletDestroy : Bullet {

    protected override bool HitAction(Transform hitObject, Vector2 hitPoint)
    {
        return hitObject.GetComponent<Hittable>().DamageSelf(damage, hitPoint, Vector2.right * xDirection);
    }
}
