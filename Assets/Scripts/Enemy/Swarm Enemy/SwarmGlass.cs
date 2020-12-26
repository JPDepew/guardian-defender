using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmGlass : Enemy
{
    SwarmContainer swarmContainer;

    protected override void Start()
    {
        shouldWrap = false;
        swarmContainer = GetComponentInParent<SwarmContainer>();
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

    protected override void DestroySelf()
    {
        // Tell Parent glass to destroy self
        InvokeOnEnemyDestroyed(destroyPoints);
        swarmContainer.StartDestroy();
        base.DestroySelf();
    }
}
