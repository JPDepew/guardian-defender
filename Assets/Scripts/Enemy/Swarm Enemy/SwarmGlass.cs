using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmGlass : Enemy
{
    protected override void Start()
    {
        shouldWrap = false;
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override bool DamageSelf(float damage, Vector2 hitPosition, Vector2? bulletDirection)
    {
        base.DamageSelf(damage, hitPosition, bulletDirection);
        audioSources[0].Play();
        return true;
    }

    protected override void StartRotation(float directionToHitY) { }
}
