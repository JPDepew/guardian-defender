using System.Collections;
using UnityEngine;

public class Bullet : BulletParent
{
    public Transform rayPos;
    public float hitOffsetMultiplier = 2f;
    public float invisibleTime = 0.5f;
    public LayerMask layerMask;
    public float damage = 1;
    protected RaycastHit2D hit;
    SpriteRenderer spriteRenderer;
    protected float xDirection;
    private bool shouldRaycast = true;

    protected override void Start()
    {
        base.Start();

        xDirection = Mathf.Sign(transform.localScale.x);
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void Update()
    {
        if (utilities.gameState == Utilities.GameState.STOPPED) return;

        transform.Translate(Vector3.right * speed * xDirection * Time.deltaTime);

        if (shouldRaycast)
        {
            Raycasting();
        }
    }

    protected void Raycasting()
    {
        hit = Physics2D.Raycast(rayPos.position, Vector2.right * xDirection, speed * hitOffsetMultiplier * Time.deltaTime, layerMask);
        Debug.DrawRay(rayPos.position, Vector2.right * xDirection * speed * hitOffsetMultiplier * Time.deltaTime, Color.red);
        if (hit)
        {
            Transform hitObject = hit.transform;
            if (HitAction(hitObject, hit.point))
            {
                StartCoroutine(DestroyObject());
            }
        }
    }

    /// <param name="hitObject">Object that has been hit</param>
    /// <param name="hitPoint">Hit contact point</param>
    /// <returns>Returns true if should perform hit action</returns>
    protected virtual bool HitAction(Transform hitObject, Vector2 hitPoint)
    {
        return true;
    }

    protected IEnumerator DestroyObject()
    {
        shouldRaycast = false;
        speed = 0;
        while (spriteRenderer.color.a >= 0)
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, spriteRenderer.color.a - 0.1f);
            yield return null;
        }
        Destroy(gameObject);
    }
}
