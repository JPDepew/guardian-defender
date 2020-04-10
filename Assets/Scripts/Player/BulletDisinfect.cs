using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletDisinfect : Bullet {

    protected override bool HitAction(Transform enemy, Vector2 hitPoint)
    {
        return enemy.GetComponent<Enemy>().DisinfectEnemy(hitPoint);
    }
}
