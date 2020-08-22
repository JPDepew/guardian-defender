using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Watch : Enemy
{
    public GameObject hitEnemy;

    public float normalSpeed = 5;
    public float chaseSpeed = 15;
    public float easeToNewDirection = 0.3f;
    public float initialWait = 0.5f;
    public float rotateShakeTime = 1f;
    public float waitToPlaySound = 6f;
    public Vector2 rotateRange;

    ShieldRotate[] rotators;
    float[] rotatorValues;
    private float speed = 0;
    private float verticalHalfSize = 0;

    public delegate void OnDestroyed();
    public static event OnDestroyed onWatchDestroyed;

    protected override void Start()
    {
        player = FindObjectOfType<ShipController>();
        verticalHalfSize = Camera.main.orthographicSize;
        base.Start();
        rotators = GetComponentsInChildren<ShieldRotate>();
        rotatorValues = new float[rotators.Length + 1];
        for (int i = 0; i < rotators.Length; i++)
        {
            rotatorValues[i] = rotators[i].rotationSpeed;
            rotators[i].rotationSpeed = 0;
        }
        StartCoroutine(PlaySound());
        StartCoroutine(Enter());
    }

    protected override void Update()
    {
        direction = Vector2.Lerp(direction, newDirection, easeToNewDirection);
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        base.Update();
    }

    IEnumerator PlaySound()
    {
        yield return new WaitForSeconds(waitToPlaySound);
        audioSources[1].Play();
        for (int i = 0; i < rotators.Length; i++)
        {
            rotators[i].rotationSpeed = rotatorValues[i];
        }
    }

    IEnumerator Enter()
    {
        yield return new WaitForSeconds(initialWait);
        transform.position = new Vector2(player.transform.position.x + 3, transform.position.y);
        newDirection = Vector2.down;
        speed = chaseSpeed;
        while (transform.position.y > 0)
        {
            yield return null;
        }
        speed = 0;
        for (int i = 0; i < rotators.Length; i++)
        {
            StartCoroutine(Rotate(rotators[i].transform, 6, rotateShakeTime));
        }
        yield return Rotate(transform, 1, rotateShakeTime);
        StartCoroutine(FindPlayer());
    }

    IEnumerator Rotate(Transform _transform, float rangeMultiplier, float time)
    {
        float timer = time + Time.time;
        while (Time.time < timer)
        {
            float rotateAmt = Random.Range(rotateRange.x * rangeMultiplier, rotateRange.y * rangeMultiplier);
            _transform.Rotate(new Vector3(0, 0, rotateAmt));
            yield return null;
        }
    }

    IEnumerator Behavior()
    {
        while (player != null)
        {
            speed = normalSpeed;
            int secondsToWait = Random.Range(15, 20);
            yield return new WaitForSeconds(secondsToWait);
            audioSources[4].Play();
            for (int i = 0; i < rotators.Length; i++)
            {
                StartCoroutine(Rotate(rotators[i].transform, 8,1));
            }
            yield return Rotate(transform, 2, 1);
            speed = chaseSpeed;
            for (int i = 0; i < rotators.Length; i++)
            {
                StartCoroutine(Rotate(rotators[i].transform, 8, 3));
            }
            yield return Rotate(transform, 2, 3);
        }
        speed = normalSpeed;
    }

    IEnumerator GetDirToPlayer()
    {
        while (player != null)
        {
            newDirection = (player.transform.position - transform.position).normalized;
            yield return new WaitForSeconds(0.3f);
        }
        StartCoroutine(FindPlayer());
    }

    protected override IEnumerator FindPlayer()
    {
        yield return base.FindPlayer();
        StartCoroutine(Behavior());
        StartCoroutine(GetDirToPlayer());
    }

    public override bool DamageSelf(float damage, Vector2 hitPosition, Vector2? bulletDirection)
    {
        audioSources[3].Play();
        return base.DamageSelf(damage, hitPosition);
    }

    protected override void DestroySelf()
    {
        TextMesh temp = Instantiate(scoreText, transform.position, transform.rotation);
        temp.text = "10000";
        PlayerStats.instance.IncreaseScoreBy(10000);
        base.DestroySelf();
    }

    private void OnDestroy()
    {
        if (onWatchDestroyed != null)
        {
            onWatchDestroyed();
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Alien")
        {
            Effect(collision.transform);

            collision.GetComponent<Enemy>().DamageSelf(100, transform.position);
        }
        if (collision.tag == "Player")
        {
            Effect(collision.transform);
            collision.GetComponent<ShipController>().DestroySelf();
            FindObjectOfType<GameMaster>().RespawnPlayer();
        }
    }

    public void Effect(Transform col)
    {
        Vector3 dirToHit = col.position - transform.position;
        Quaternion angleToPlayer = Quaternion.LookRotation(dirToHit);
        GameObject tempHit = Instantiate(hitEnemy, col.position, angleToPlayer);
        audioSources[2].Play();
    }
}
