using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hittable : ScreenWrappingObject {
    public virtual bool DamageSelf(float damage, Vector2 hitPosition, Vector2? bulletDirection = null)
    {
        return true;
    }
}
