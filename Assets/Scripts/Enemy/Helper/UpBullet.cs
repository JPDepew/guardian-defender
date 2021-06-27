using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpBullet : BulletParent
{

    protected override void Start()
    {
        base.Start();
        direction = transform.up;
    }

    protected override void Update()
    {
        if (utilities.gameState == Utilities.GameState.STOPPED) return;
        transform.position += transform.up * speed * Time.deltaTime;
    }
}
