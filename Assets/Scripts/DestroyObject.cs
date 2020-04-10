using System.Collections;
using UnityEngine;

public class DestroyObject : ScreenWrappingObject
{
    public float destroyAfter = 1f;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(DestroyAfter());
    }

    IEnumerator DestroyAfter()
    {
        yield return new WaitForSeconds(destroyAfter);
        Destroy(gameObject);
    }
}