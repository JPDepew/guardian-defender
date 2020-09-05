using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Transform rayPos;
    public float speed = 1f;
    public float hitOffsetMultiplier = 2f;
    public float invisibleTime = 0.5f;
    public LayerMask layerMask;
    public float damage = 1;
    public float destroyAfter = 1f;

    Utilities utilities;

    protected RaycastHit2D hit;
    SpriteRenderer spriteRenderer;
    protected float direction;
    private bool shouldRaycast = true;

    protected virtual void Start()
    {
        utilities = Utilities.instance;

        direction = Mathf.Sign(transform.localScale.x);
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(DelayedDestroy());
    }

    protected virtual void Update()
    {
        if (utilities.gameState == Utilities.GameState.STOPPED) return;

        transform.Translate(Vector3.right * speed * direction * Time.deltaTime);

        if (shouldRaycast)
        {
            Raycasting();
        }
    }

    protected void Raycasting()
    {
        hit = Physics2D.Raycast(rayPos.position, Vector2.right * direction, speed * hitOffsetMultiplier * Time.deltaTime, layerMask);
        Debug.DrawRay(rayPos.position, Vector2.right * direction * speed * hitOffsetMultiplier * Time.deltaTime, Color.red);
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

    IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(destroyAfter);
        Destroy(gameObject);
    }
}
