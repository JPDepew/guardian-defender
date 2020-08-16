using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingSaucer : Enemy
{
    public float speed;
    public float actionDstToPlayer = 7f;
    public float easeToNewDirection = 0.3f;
    public Vector2 horizontalBounds;

    public GameObject bullet;

    PlayerStats playerStats;

    float verticalHalfSize;
    bool goToTopOfPlayer = true;

    protected override void Start()
    {
        base.Start();
        playerStats = PlayerStats.instance;

        StartCoroutine(StartEverything());

        direction = Vector2.zero;
        verticalHalfSize = Camera.main.orthographicSize;
    }

    protected override void Update()
    {
        verticalHalfSize = Camera.main.orthographicSize;

        HandleOffScreenDirection();
        direction = Vector2.Lerp(direction, newDirection, easeToNewDirection);
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

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

    public override bool DamageSelf(float damage, Vector2 hitPosition)
    {
        int index = Random.Range(0, 5);
        audioSources[index].Play();
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
                }
                else
                {
                    newDirection = dirToPlayer.normalized;
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
                //player = FindObjectOfType<ShipController>();
            }
            else
            {
                ShootAtPlayer();
                yield return new WaitForSeconds(3f);
                goToTopOfPlayer = !goToTopOfPlayer;
            }
        }
    }

    protected override void DestroySelf()
    {
        // change this next
        speed = 0;
        audioSources[6].Stop();
        int randomNum = Random.Range(0, 1);
        if (randomNum == 0)
        {
            randomNum = Random.Range(0, constants.powerups.Count);
            while (playerStats.IsPowerupActive(constants.powerups[randomNum].powerupEnum) && playerStats.NumPowerupsActive() < playerStats.powerups.Count - 1)
            {
                randomNum = (randomNum + 1) % (playerStats.powerups.Count - 1);
                print(playerStats.powerups.Count - 1);
            }
            Instantiate(constants.powerups[randomNum], transform.position, transform.rotation);
        }
        scoreText = Instantiate(scoreText, new Vector3(transform.position.x, transform.position.y, -5), transform.rotation);
        scoreText.text = "300";
        playerStats.IncreaseScoreBy(300);
        base.DestroySelf();
    }

    void ShootAtPlayer()
    {
        Vector2 direction = (player.transform.position - transform.position).normalized;
        GameObject alienBullet = Instantiate(bullet, transform.position, transform.rotation);
        AlienBullet tempBullet = alienBullet.GetComponent<AlienBullet>();
        tempBullet.speed *= 1.5f;
        tempBullet.direction = direction;
    }
}
