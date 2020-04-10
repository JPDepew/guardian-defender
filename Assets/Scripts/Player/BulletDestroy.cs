using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletDestroy : Bullet {

    protected override bool HitAction(Transform enemy, Vector2 hitPoint)
    {
        return enemy.GetComponent<Hittable>().DamageSelf(damage, hitPoint);
    }
}
