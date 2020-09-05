using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hittable : ScreenWrappingObject {
    /// <param name="damage">The damage amount to apply</param>
    /// <param name="hitPosition">The position a hit was received at</param>
    /// <param name="bulletDirection">The direction the hit was travelling ing</param>
    /// <returns>True if damage occurred</returns>
    public virtual bool DamageSelf(float damage, Vector2 hitPosition, Vector2? bulletDirection = null)
    {
        return true;
    }
}
