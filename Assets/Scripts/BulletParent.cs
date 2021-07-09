using System.Collections;
using UnityEngine;

public class BulletParent : MonoBehaviour
{
    public float destroyAfter = 10f;
    public float speed = 1;
    public int shrinkCounter = 30;
    public bool shouldOnlyDestroyCollider = false;
    public Vector2 direction;

    protected Utilities utilities;

    float shrinkSpeed = 0.98f;

    protected virtual void Start()
    {
        utilities = Utilities.instance;
        StartCoroutine(DelayedDestroy());
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (utilities.gameState == Utilities.GameState.STOPPED) return;
        transform.Translate(direction * speed * Time.deltaTime);
    }

    protected IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(destroyAfter);
        while (shrinkCounter > 0)
        {
            transform.localScale *= shrinkSpeed;
            shrinkCounter--;
            yield return null;
        }
        Destroy(gameObject);
    }

    public void DestroySelf()
    {
        if (shouldOnlyDestroyCollider)
        {
            GetComponent<Collider2D>().enabled = false;
            ParticleSystem particleSystem = GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                particleSystem.Stop();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
