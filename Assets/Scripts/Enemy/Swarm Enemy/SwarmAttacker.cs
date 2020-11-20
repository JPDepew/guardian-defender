using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class SwarmAttacker : Enemy
{
    public float lerpTime = 0.5f;
    public float acceleration = 0.1f;
    public float maxVelocity = 10;
    public float maxDstToPlayer = 3;
    public float offsetFromVerticalBounds = 3;
    public float directionMultiplier = 0.5f;
    public float shootWaitTime = 0.4f;
    public float raycastDst = 5;
    public float avoidEnemyTime = 0.5f;

    public GameObject bullet;
    public LayerMask layerMaskToAvoid;

    float verticalHalfSize;
    bool avoidingEnemy = false;
    Vector2 directionToUse;

    enum SwarmAttackState { CHASING, SHOOTING, INACTIVE };
    SwarmAttackState swarmAttackState = SwarmAttackState.INACTIVE;
    Rigidbody2D rb2D;
    CircleCollider2D circleCollider2D;
    ParticleSystem[] engineParticleSystems;
    ParticleSystem shootWarmup;

    protected override void Start()
    {
        base.Start();
        shouldWrap = false;
        rb2D = GetComponent<Rigidbody2D>();
        circleCollider2D = GetComponent<CircleCollider2D>();
        engineParticleSystems = GetComponentsInChildren<ParticleSystem>().Where(x => x.CompareTag("Engine")).ToArray();
        shootWarmup = GetComponentsInChildren<ParticleSystem>().FirstOrDefault(x => x.tag != "Engine");
        verticalHalfSize = Camera.main.orthographicSize;
    }

    protected override void Update()
    {
        base.Update();
    }

    public void Activate()
    {
        ActivateRigidbody();
        shouldWrap = true;
        transform.parent = null;
        circleCollider2D.enabled = true;
        swarmAttackState = SwarmAttackState.CHASING;
        StartCoroutine(StartCoroutinesDelay());
    }

    void ActivateRigidbody()
    {
        Vector2 directionToParent = (transform.position - transform.parent.position).normalized;
        float speedMultiplier = 0.5f;
        float torque = Random.Range(200, 800);
        int sign = Random.Range(0, 1) * 2 - 1;
        rb2D.bodyType = RigidbodyType2D.Dynamic;
        rb2D.simulated = true;
        rb2D.AddForce(directionToParent * Time.deltaTime * speedMultiplier);
        rb2D.AddTorque(torque * sign);
    }

    IEnumerator StartCoroutinesDelay()
    {
        yield return new WaitForSeconds(0.75f);
        rb2D.angularVelocity = 0;
        StartCoroutine(FindPlayer());
        StartCoroutine(GetDirectionToPlayer());
        StartCoroutine(ActionController());
    }

    IEnumerator ActionController()
    {
        while (true)
        {
            if (!player && !findingPlayer)
            {
                StartCoroutine(FindPlayer());
            }
            Vector2? evadeDirection = GetEvadeDirection();
            if (evadeDirection != null)
            {
                directionToUse = (Vector2)GetEvadeDirection();
            }
            else if (transform.position.y > verticalHalfSize - offsetFromVerticalBounds - 1)
            {
                directionToUse = direction.normalized + Vector2.down * Mathf.Abs(transform.position.y) * directionMultiplier;
            }
            else if (transform.position.y < -verticalHalfSize + offsetFromVerticalBounds)
            {
                directionToUse = direction.normalized + Vector2.up * Mathf.Abs(transform.position.y) * directionMultiplier;
            }
            else
            {
                float xDstToPlayer = Mathf.Abs(direction.x);
                if (xDstToPlayer > maxDstToPlayer || direction == Vector2.zero)
                {
                    directionToUse = new Vector2(direction.x, 0);
                }
                else
                {
                    if (swarmAttackState == SwarmAttackState.CHASING)
                    {
                        StartCoroutine(ShootAtPlayer());
                    }
                    directionToUse = direction;
                }
            }
            HandleRotation(directionToUse);
            HandleAcceleration();
            yield return null;
        }
    }

    void HandleRotation(Vector2 directionToUse)
    {
        float angle = Vector2.SignedAngle(transform.up, directionToUse);
        float angleToRotate = Mathf.Lerp(0, angle, lerpTime);
        transform.Rotate(Vector3.forward, angleToRotate);
    }

    /// <summary>
    /// If there is a hittable object in front, return a vector with a new direction
    /// to evade the object
    /// </summary>
    /// <returns>Null if no hit, otherwise forward new direction</returns>
    Vector2? GetEvadeDirection()
    {
        Vector3 offset = transform.up * circleCollider2D.radius * (transform.localScale.x + 0.01f);
        RaycastHit2D raycastHit2D = Physics2D.Raycast(transform.position + offset, transform.up, raycastDst, layerMaskToAvoid);
        Debug.DrawRay(transform.position + offset, transform.up * raycastDst, Color.blue);

        bool hit = raycastHit2D;
        float step = 15;
        int itrCount = 1;
        int maxRays = 4;
        Vector2 leftVectorDirection;
        Vector2 rightVectorDirection;
        RaycastHit2D raycastHit2DLeft;
        RaycastHit2D raycastHit2DRight;

        Vector2 evadeDirection = Vector2.zero;

        while (itrCount < maxRays)
        {
            leftVectorDirection = Quaternion.Euler(0, 0, step * itrCount) * transform.up;
            rightVectorDirection = Quaternion.Euler(0, 0, -step * itrCount) * transform.up;
            raycastHit2DLeft = Physics2D.Raycast(transform.position + offset, leftVectorDirection, raycastDst, layerMaskToAvoid);
            raycastHit2DRight = Physics2D.Raycast(transform.position + offset, rightVectorDirection, raycastDst, layerMaskToAvoid);

            Debug.DrawRay(transform.position + offset, leftVectorDirection * raycastDst, Color.red);
            Debug.DrawRay(transform.position + offset, rightVectorDirection * raycastDst, Color.red);

            if (raycastHit2DRight || raycastHit2DLeft)
            {
                hit = true;
            }

            // get closest
            if (!raycastHit2DRight)
            {
                evadeDirection = rightVectorDirection;
                break;
            }
            if (!raycastHit2DLeft)
            {
                evadeDirection = leftVectorDirection;
                break;
            }

            itrCount++;
        }

        Vector2? newDirection;
        newDirection = GetExtendedVector(evadeDirection, transform.up);
        Debug.DrawRay(transform.position + offset, (Vector2)newDirection * raycastDst, Color.green);

        return hit ? newDirection : null;
    }

    /// <summary>
    /// Get a vector that is the same direction from the compareVector as evadeDirection, but at a larger angle
    /// </summary>
    /// <param name="evadeDirection">The direction from the compare vector</param>
    /// <param name="compareVector">The base vector from which evadeDirection comes</param>
    /// <returns>Vector the same as evadeDirection but farther</returns>
    Vector2 GetExtendedVector(Vector2 evadeDirection, Vector2 compareVector)
    {
        float angleFromEvadeToCompare = Vector2.SignedAngle(evadeDirection, compareVector);
        Vector2 extendedVector = Quaternion.Euler(0, 0, -angleFromEvadeToCompare) * evadeDirection;

        return (extendedVector * 2 + evadeDirection).normalized;
    }

    void HandleAcceleration()
    {
        HandleEngineParticleSystems();
        float speedToUse = IsShooting() ? 0 : acceleration;
        rb2D.AddForce(transform.up * Time.deltaTime * speedToUse);
        rb2D.velocity = Vector2.ClampMagnitude(rb2D.velocity, maxVelocity);
    }

    void HandleEngineParticleSystems()
    {
        bool isShooting = IsShooting();
        if (!isShooting && !engineParticleSystems[0].isEmitting)
        {
            PlayAllParticleSystems();
        }
        if (isShooting && engineParticleSystems[0].isPlaying)
        {
            StopAllParticleSystems();
        }
    }

    void PlayAllParticleSystems()
    {
        for (int i = 0; i < engineParticleSystems.Length; i++)
        {
            engineParticleSystems[i].Play();
        }
    }

    void StopAllParticleSystems()
    {
        for (int i = 0; i < engineParticleSystems.Length; i++)
        {
            engineParticleSystems[i].Stop();
        }
    }

    bool IsShooting()
    {
        return swarmAttackState == SwarmAttackState.SHOOTING;
    }

    IEnumerator ShootAtPlayer()
    {
        swarmAttackState = SwarmAttackState.SHOOTING;
        shootWarmup.Play();
        yield return new WaitForSeconds(shootWarmup.main.duration);
        GameObject alienBullet = Instantiate(bullet, transform.position, Quaternion.identity);
        AlienBullet tempBullet = alienBullet.GetComponent<AlienBullet>();
        tempBullet.direction = transform.up;
        yield return new WaitForSeconds(shootWaitTime);
        swarmAttackState = SwarmAttackState.CHASING;
    }

    IEnumerator GetDirectionToPlayer()
    {
        while (true)
        {
            if (player)
            {
                direction = player.transform.position - transform.position;
            }
            else
            {
                direction = Vector2.zero;
            }
            float timeToWait = Random.Range(0.001f, 0.1f);
            yield return new WaitForSeconds(timeToWait);
        }
    }

    public override bool DamageSelf(float damage, Vector2 hitPosition, Vector2? bulletDirection = null)
    {
        return base.DamageSelf(damage, hitPosition, bulletDirection);
    }

    protected override void DestroySelf()
    {
        InvokeOnEnemyDestroyed();
        base.DestroySelf();
    }
}
