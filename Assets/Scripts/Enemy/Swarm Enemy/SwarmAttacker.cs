using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmAttacker : Enemy
{
    public float lerpTime = 0.5f;
    public float acceleration = 0.1f;
    public float maxVelocity = 10;
    public float maxDstToPlayer = 3;
    public float offsetFromVerticalBounds = 3;
    public float directionMultiplier = 0.5f;
    public float shootWaitTime = 0.4f;

    public GameObject bullet; 

    float verticalHalfSize;

    enum SwarmAttackState { CHASING, SHOOTING };
    SwarmAttackState swarmAttackState = SwarmAttackState.CHASING;
    Rigidbody2D rb2D;

    protected override void Start()
    {
        base.Start();
        rb2D = GetComponent<Rigidbody2D>();
        StartCoroutine(FindPlayer());
        StartCoroutine(GetDirectionToPlayer());
        verticalHalfSize = Camera.main.orthographicSize;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (player)
        {
            Vector2 directionToUse;
            float xDstToPlayer = Mathf.Abs(direction.x);
            if (transform.position.y > verticalHalfSize - offsetFromVerticalBounds)
            {
                directionToUse = direction.normalized + Vector2.down * Mathf.Abs(transform.position.y) * directionMultiplier;
            }
            else if (transform.position.y < -verticalHalfSize + offsetFromVerticalBounds)
            {
                directionToUse = direction.normalized + Vector2.up * Mathf.Abs(transform.position.y) * directionMultiplier;
            }
            else
            {
                if (xDstToPlayer > maxDstToPlayer)
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
        }
        else
        {
            StartCoroutine(FindPlayer());
        }
    }

    void HandleRotation(Vector2 directionToUse)
    {
        float angle = Vector2.SignedAngle(transform.up, directionToUse);
        float angleToRotate = Mathf.Lerp(0, angle, lerpTime);
        transform.Rotate(Vector3.forward, angleToRotate);
    }

    void HandleAcceleration()
    {
        float speedToUse = swarmAttackState == SwarmAttackState.SHOOTING ? 0 : acceleration;
        rb2D.AddForce(transform.up * Time.deltaTime * speedToUse);
        rb2D.velocity = Vector2.ClampMagnitude(rb2D.velocity, maxVelocity);
    }

    IEnumerator ShootAtPlayer()
    {
        swarmAttackState = SwarmAttackState.SHOOTING;
        GameObject alienBullet = Instantiate(bullet, transform.position, Quaternion.identity);
        AlienBullet tempBullet = alienBullet.GetComponent<AlienBullet>();
        print(transform.up);
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
}
