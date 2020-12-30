using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingSaucer : Enemy
{
    public float speed;
    public float shootSpeed;
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

        HandleOffScreenDirection();
        direction = Vector2.Lerp(direction, newDirection, easeToNewDirection);
        transform.Translate(direction * currentSpeed * Time.deltaTime, Space.World);

        base.Update();
    }

    private void HandleOffScreenDirection()
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
                Vector2 dirToPlayer = player.transform.position - transform.position;
                float dstToPlayer = dirToPlayer.magnitude;
                if (dstToPlayer < actionDstToPlayer)
                {
                    float placement = goToTopOfPlayer ? 2 : -2;
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
                yield return new WaitForSeconds(3f);
                goToTopOfPlayer = !goToTopOfPlayer;
            }
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
            Vector2 direction = (player.transform.position - transform.position).normalized;
            GameObject alienBullet = Instantiate(bullet, transform.position, transform.rotation);
            AlienBullet tempBullet = alienBullet.GetComponent<AlienBullet>();
            tempBullet.speed *= 1.5f;
            tempBullet.direction = direction;
            yield return new WaitForSeconds(shootSpeed);
        }
    }
}
