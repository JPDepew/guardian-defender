using System.Collections;
using UnityEngine;

public class FlyingSaucer : Enemy
{
    public float speed;
    public float shootWaitTime;
    public float actionDstToPlayer = 7f;
    public float easeToNewDirection = 0.3f;
    public Vector2 horizontalBounds;

    public GameObject bullet;

    PlayerStats playerStats;

    private float currentSpeed;
    float verticalHalfSize;
    bool goToTopOfPlayer = true;
    private bool shootingAtPlayer = false;

    protected override void Start()
    {
        base.Start();
        playerStats = PlayerStats.instance;

        StartCoroutine(StartEverything());

        currentSpeed = speed;
        direction = Vector2.zero;
        verticalHalfSize = Camera.main.orthographicSize;
    }

    protected override void Update()
    {
        verticalHalfSize = Camera.main.orthographicSize;
        SetDirectionToPlayer();
        HandleOffScreenDirection();
        direction = Vector2.Lerp(direction, newDirection, easeToNewDirection);
        transform.Translate(direction * currentSpeed * Time.deltaTime, Space.World);

        base.Update();
    }
    public override void KonamiAction()
    {
        base.KonamiAction();
        speed *= 1.5f;
        shootWaitTime /= 4;
    }

    IEnumerator StartEverything()
    {
        yield return FindPlayer();
        StartCoroutine(ChasePlayer());
    }

    public override bool DamageSelf(float damage, Vector2 hitPosition, Vector2? bulletDirection)
    {
        soundPlayer.PlayRandomSoundFromRange(0, 5);
        return base.DamageSelf(damage, hitPosition);
    }

    IEnumerator ChasePlayer()
    {
        StartCoroutine(ChangeDirectionTimer());
        while (true)
        {
            if (player == null)
            {
                newDirection = Vector2.left;
                yield return FindPlayer();
            }
            else
            {
                float dstToPlayer = dirToPlayer.magnitude;
                RaycastHit2D raycastHit2D = Physics2D.Raycast(
                    transform.position,
                    dirToPlayer,
                    forwardRaycastDst,
                    layerMaskToAvoid
                );
                Debug.DrawRay(transform.position, dirToPlayer.normalized * forwardRaycastDst, Color.red);
                if (raycastHit2D)
                {
                    newDirection = GetAvoidDirection(raycastHit2D.collider.transform.position);
                }
                else if (dstToPlayer < actionDstToPlayer)
                {

                    float placement = goToTopOfPlayer ? 2 : -2;
                    if (konami)
                    {
                        float rotation = goToTopOfPlayer ? 10 : -10;
                        transform.Rotate(new Vector3(0, 0, rotation));
                    }
                    newDirection = new Vector2(dirToPlayer.x, dirToPlayer.y + placement).normalized;
                    if (!shootingAtPlayer)
                    {
                        StartCoroutine("ShootAtPlayer");
                    }
                }
                else
                {
                    newDirection = dirToPlayer.normalized;
                    if (shootingAtPlayer)
                    {
                        StopCoroutine("ShootAtPlayer");
                        shootingAtPlayer = false;
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator ChangeDirectionTimer()
    {
        while (true)
        {
            if (player == null)
            {
                newDirection = Vector2.left;
                yield return FindPlayer();
            }
            else
            {
                float waitTime = konami ? 1 : 3;
                yield return new WaitForSeconds(waitTime);
                goToTopOfPlayer = !goToTopOfPlayer;
            }
        }
    }

    protected void HandleOffScreenDirection()
    {
        if (transform.position.y > verticalHalfSize - constants.topOffset && goToTopOfPlayer)
        {
            // Condition: saucer is above screen
            newDirection = new Vector2(newDirection.x, 0);
        }
        else if (transform.position.y < -verticalHalfSize + constants.bottomOffset && !goToTopOfPlayer)
        {
            // Condition: saucer is below screen
            newDirection = new Vector2(newDirection.x, 0);
        }
    }

    /// <summary>
    /// Avoiding the object that was hit
    /// </summary>
    /// <param name="raycastHit2D"></param>
    Vector2 GetAvoidDirection(Vector2 hitTransform)
    {
        float multiplier = 1.5f;
        Vector2 normalizedDirection = dirToPlayer.normalized;
        if (hitTransform.y > transform.position.y)
        {
            return (normalizedDirection + Vector2.down * multiplier).normalized;
        }
        else
        {
            return (normalizedDirection + Vector2.up * multiplier).normalized;
        }
    }

    protected override void DestroySelf()
    {
        // change this next
        currentSpeed = 0;
        audioSources[6].Stop();
        PowerupObj powerup = playerStats.GetRandomPowerup();
        Instantiate(powerup, transform.position, transform.rotation);
        base.DestroySelf(false);
    }

    IEnumerator ShootAtPlayer()
    {
        shootingAtPlayer = true;
        while (true)
        {
            if (player)
            {
                Vector2 direction = (player.transform.position - transform.position).normalized;
                GameObject alienBullet = Instantiate(bullet, transform.position, transform.rotation);
                AlienBullet tempBullet = alienBullet.GetComponent<AlienBullet>();
                tempBullet.speed *= 1.5f;
                tempBullet.direction = direction;
            }
            yield return new WaitForSeconds(shootWaitTime);
        }
    }
}
