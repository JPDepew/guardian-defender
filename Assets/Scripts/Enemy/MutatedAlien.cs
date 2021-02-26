using System.Collections;
using UnityEngine;

public class MutatedAlien : Enemy
{
    public GameObject human;
    public GameObject hitMask;
    public GameObject disinfectMask;
    public GameObject disinfectHit;
    public float speedMax = 10, speedMin = 6;
    public float offsetMax = 2, offsetMin = -2;
    public float changeTimeMin = 0.2f, changeTimeMax = 1f;
    public float easeToNewDirection = 0.2f;
    public float disinfectHumanOffset = 0.3f;
    public float destroyDelay = 0.4f;
    public float disinfectHealth = 3;
    public float dstToAttack = 3f;

    float newSpeed = 8;
    float speed = 8;
    float randomYOffset = 0;
    float verticalHalfSize;
    Vector2 currentScale;

    protected override void Start()
    {
        StartCoroutine(ChasePlayer());
        StartCoroutine(ChangeSpeed());
        StartCoroutine(ChangeOffset());
        randomYOffset = Random.Range(offsetMin, offsetMax);
        speed = Random.Range(speedMin, speedMax);
        newSpeed = speed;
        currentScale = transform.localScale;
        base.Start();
    }

    public override void KonamiAction()
    {
        base.KonamiAction();
        speedMin = speedMin / 2;
        speedMax = speedMax * 1.7f;
        if (GetRandomChance(2))
        {
            StartCoroutine("GrowCycle");
        }
    }

    protected override void Update()
    {
        verticalHalfSize = Camera.main.orthographicSize;

        speed = Mathf.Lerp(speed, newSpeed, 0.1f);

        HandleOffScreenDirection();
        direction = Vector2.Lerp(direction, newDirection, easeToNewDirection);
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        base.Update();
    }

    public override bool DamageSelf(float damage, Vector2 hitPosition, Vector2? bulletDirection)
    {
        soundPlayer.PlayRandomSoundFromRange(0, 1);
        GameObject tempMask = Instantiate(hitMask, hitPosition, transform.rotation);
        tempMask.transform.parent = transform;
        return base.DamageSelf(damage, hitPosition);
    }

    public override bool DisinfectEnemy(Vector2 hitPoint)
    {
        disinfectHealth--;
        GameObject tempMask = Instantiate(hitMask, hitPoint, transform.rotation);
        tempMask.transform.localScale *= 2;
        tempMask.transform.parent = transform;
        Instantiate(disinfectHit, transform.position, transform.rotation);
        soundPlayer.PlayRandomSoundFromRange(0, 1);
        if (disinfectHealth <= 0)
        {
            speed = speed / 2;
            // refactor all this
            GameObject temp = Instantiate(disinfectMask, new Vector2(transform.position.x, transform.position.y + 0.3f), transform.rotation);
            temp.transform.parent = transform;
            StartCoroutine(DestroyAfterDelay());
        }
        return true;
    }

    IEnumerator ChangeSpeed()
    {
        while (true)
        {
            float waitTime = Random.Range(changeTimeMin, changeTimeMax);
            yield return new WaitForSeconds(waitTime);
            newSpeed = Random.Range(speedMin, speedMax);
        }
    }

    IEnumerator ChangeOffset()
    {
        while (true)
        {
            float waitTime = Random.Range(changeTimeMin, changeTimeMax);
            yield return new WaitForSeconds(waitTime);
            randomYOffset = Random.Range(offsetMin, offsetMax);
        }
    }

    /// <summary>
    /// Destroy used in the case of disinfection, delayed so that the sprite mask on the 
    /// mutated alien fade in
    /// </summary>
    IEnumerator DestroyAfterDelay()
    {
        InvokeOnEnemyDestroyed(destroyPoints);
        GetComponent<PolygonCollider2D>().enabled = false;
        yield return new WaitForSeconds(destroyDelay);
        InstantiateHuman();
        Destroy(gameObject);
    }

    /// <summary>
    /// Only used in konami mode. Randomly grows and shrinks the mutated alien
    /// </summary>
    IEnumerator GrowCycle()
    {
        while (true)
        {
            Vector2 targetScale = GetRandomVector2();
            StopCoroutine("Scale");
            StartCoroutine("Scale", targetScale);
            yield return new WaitForSeconds(3);
        }
    }

    IEnumerator Scale(Vector2 targetScale)
    {
        while (true)
        {
            transform.localScale = Vector2.Lerp(transform.localScale, targetScale, 0.1f);
            yield return null;
        }
    }

    void InstantiateHuman()
    {
        if (konami)
        {
            int maxHumanCount = 3;
            for (int i = 0; i < maxHumanCount; i++)
            {
                Vector2 offset = GetRandomVectorInRange(0.3f);
                GameObject newHuman = Instantiate(
                    human,
                    new Vector2(transform.position.x + offset.x, transform.position.y - disinfectHumanOffset + offset.y),
                    Quaternion.Euler(Vector2.zero)
                );
                newHuman.GetComponent<Human>().SetToFalling();
            }
        }
        else
        {
            GameObject newHuman = Instantiate(human, new Vector2(transform.position.x, transform.position.y - disinfectHumanOffset), Quaternion.Euler(Vector2.zero));
            newHuman.GetComponent<Human>().SetToFalling();
        }
    }

    protected override void DestroySelf()
    {
        base.DestroySelf();
    }

    IEnumerator ChasePlayer()
    {
        float actualXOffset = randomYOffset;
        while (true)
        {
            if (player == null)
            {
                newDirection = Vector2.left;
                yield return FindPlayer();
            }
            else
            {
                if ((transform.position - player.transform.position).magnitude < dstToAttack)
                {
                    actualXOffset = 0;
                }
                else
                {
                    actualXOffset = randomYOffset;
                }
                newDirection = new Vector2(player.transform.position.x - transform.position.x, player.transform.position.y - transform.position.y + actualXOffset).normalized;
            }
            yield return new WaitForSeconds(0.25f);
        }
    }

    private void HandleOffScreenDirection()
    {
        if (transform.position.y > verticalHalfSize - constants.topOffset && newDirection.y > 0)
        {
            // Condition: alien is above screen
            newDirection = new Vector2(newDirection.x, 0);
        }
        else if (transform.position.y < -verticalHalfSize + constants.bottomOffset && newDirection.y < 0)
        {
            // Condition: alien is below screen
            newDirection = new Vector2(newDirection.x, 0);
        }
    }

    public void SetDirection(Vector2 _direction)
    {
        newDirection = _direction;
    }

    public void SetDemo()
    {
        StopAllCoroutines();
        newSpeed = 0;
    }

    /// <summary>
    /// Decide if the 
    /// </summary>
    bool GetRandomChance(int maxRandomRange)
    {
        int growChance = Random.Range(0, maxRandomRange);
        if (growChance == 0)
        {
            return true;
        }
        return false;
    }

    Vector2 GetRandomVector2()
    {
        float maxScale = 2;
        float minScale = -1;
        float scale = Random.Range(minScale, maxScale);
        return new Vector2(currentScale.x + scale, currentScale.y + scale);
    }
}

