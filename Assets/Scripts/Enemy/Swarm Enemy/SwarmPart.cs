using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SoundPlayer))]
public class SwarmPart : Hittable
{
    public float rotationAmountMax = 45;
    public float speed = 10;

    private float rotateAmount;
    private SoundPlayer soundPlayer;
    private SpriteRenderer spriteRenderer;

    protected override void Start()
    {
        base.Start();
        soundPlayer = GetComponent<SoundPlayer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        shouldWrap = false;
    }

    public void FlyOffDestroy(Vector2 parentPosition)
    {
        GameObject parent = transform.parent.gameObject;
        rotateAmount = Random.Range(-rotationAmountMax, rotationAmountMax);
        transform.parent = null;
        Destroy(parent);
        StartCoroutine(Explode(parentPosition));
    }

    public override bool DamageSelf(float damage, Vector2 hitPosition, Vector2? bulletDirection = null)
    {
        soundPlayer.PlayRandomSoundFromRange(0, 1);
        return base.DamageSelf(damage, hitPosition, bulletDirection);
    }

    IEnumerator Explode(Vector2 parentPosition)
    {
        Vector2 direction = (Vector2)transform.position - parentPosition;
        while (spriteRenderer.isVisible)
        {
            transform.Rotate(Vector3.back, rotateAmount * Time.deltaTime);
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
            yield return null;
        }
        Destroy(gameObject);
    }
}
