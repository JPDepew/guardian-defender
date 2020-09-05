using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletDisinfect : Bullet {

    protected override bool HitAction(Transform hitObject, Vector2 hitPoint)
    {
        return hitObject.GetComponent<Enemy>().DisinfectEnemy(hitPoint);
    }
}
