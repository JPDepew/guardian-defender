using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmGlass : Enemy
{
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override bool DamageSelf(float damage, Vector2 hitPosition, Vector2? bulletDirection)
    {
        if (bulletDirection != null)
        {
            print("Damaging glass");
            //audioSources[].Play();
            Instantiate(hit, hitPosition, Quaternion.identity, transform);
        }
        return true;
    }
}
