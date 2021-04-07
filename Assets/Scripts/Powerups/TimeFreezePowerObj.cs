using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeFreezePowerObj : PowerupObj
{
    protected override void Start()
    {
        base.Start();
    }
    public override bool CanBeDropped()
    {
        return PlayerStats.instance.timeFreezeAmountRemaining < maxCount;
    }
}
