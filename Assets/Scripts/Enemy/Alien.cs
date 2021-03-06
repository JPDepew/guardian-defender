﻿using System.Collections;
using UnityEngine;

public class Alien : Enemy
{
    public enum State { PATROLLING, CHASING, ABDUCTING, DEAD, DEMO }

    public State curState;

    public float speed;
    public float abductionSpeed;
    public float timeToChangeDirection = 3;
    public float easeToNewDirection = 0.3f;
    public float humanOffset = 0.8f;
    public Vector2 horizontalBounds;
    public bool isMiniatureAlien = false;

    public GameObject miniatureAlien;

    public SpriteRenderer windows;

    private Human human;
    float verticalHalfSize;

    public GameObject infectedAlien;
    private GameMaster gameMaster;

    protected override void Start()
    {
        base.Start();

        curState = State.PATROLLING;
        direction = Vector2.zero;
        verticalHalfSize = Camera.main.orthographicSize;
        gameMaster = GameMaster.instance;
        StartCoroutine("ChangeDirection");
        StartCoroutine("AvoidWalls");
    }

    protected override void Update()
    {
        float speedToUse = GetSpeed();
        verticalHalfSize = Camera.main.orthographicSize;

        direction = Vector2.Lerp(direction, newDirection, easeToNewDirection);
        transform.Translate(direction * speedToUse * Time.deltaTime, Space.World);

        base.Update();
    }

    public override void KonamiAction()
    {
        base.KonamiAction();
        if (!isMiniatureAlien)
        {
            health = 1;
            speed *= 1.5f;
        }
    }

    public void ChaseHuman(Human human)
    {
        curState = State.CHASING;
        StopCoroutine("ChangeDirection");
        StopCoroutine("AvoidWalls");
        StartCoroutine("ChasingHuman", human);
    }

    private float GetSpeed()
    {
        switch (curState)
        {
            case State.ABDUCTING:
                return abductionSpeed;
            case State.DEMO:
                return 0;
            default:
                return speed;
        }
    }

    IEnumerator ChangeDirection()
    {
        curState = State.PATROLLING;
        while (true)
        {
            newDirection = Random.insideUnitCircle.normalized;
            yield return new WaitForSeconds(timeToChangeDirection);
        }
    }

    IEnumerator AvoidWalls()
    {
        while (true)
        {
            if (transform.position.y > verticalHalfSize - constants.topOffset - 0.4f)
            {
                newDirection = new Vector2(newDirection.x, -Mathf.Abs(newDirection.y));
            }
            if (transform.position.y < -verticalHalfSize + constants.bottomOffset)
            {
                newDirection = new Vector2(newDirection.x, Mathf.Abs(newDirection.y));
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator ChasingHuman(Human _human)
    {
        while (true)
        {
            if (_human && (_human.curState == Human.State.GROUNDED || _human.curState == Human.State.FALLING))
            {
                newDirection = -(transform.position - _human.transform.position).normalized;
                yield return null;
            }
            else
            {
                StartCoroutine("ChangeDirection");
                StartCoroutine("AvoidWalls");
                curState = State.PATROLLING;
                break;
            }
        }
    }

    public override bool DamageSelf(float damage, Vector2 hitPosition, Vector2? bulletDirection = null)
    {
        soundPlayer.PlayRandomSoundFromRange(0, 5);
        return base.DamageSelf(damage, hitPosition);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (collision.tag == "Human" && !human)
        {
            human = collision.GetComponent<Human>();

            if (human.curState == Human.State.FALLING || human.curState == Human.State.GROUNDED)
            {
                human.SetToAbducted(transform, humanOffset);

                StopAllCoroutines();
                StartCoroutine("Abducting");
            }
            else
            {
                curState = State.PATROLLING;
                human = null;
            }
        }
    }

    // JOSIAH - LOOK HERE
    IEnumerator Abducting()
    {
        audioSources[7].Play();
        newDirection = Vector2.up;
        curState = curState == State.DEMO ? State.DEMO : State.ABDUCTING;
        bool infectedHuman = false;
        while (human && human.curState != Human.State.DEAD)
        {
            if (transform.position.y > verticalHalfSize - constants.topOffset)
            {
                Instantiate(infectedAlien, new Vector2(transform.position.x, transform.position.y - 0.3f), Quaternion.Euler(Vector2.zero));
                Destroy(gameObject);
                break;
            }

            yield return null;
        }

        if (!infectedHuman)
        {
            human = null;
            // This occurs if he loses the human
            StartCoroutine("ChangeDirection");
            StartCoroutine("AvoidWalls");
        }
    }

    protected override void DestroySelf()
    {
        speed = 0;
        Destroy(windows);
        audioSources[6].Stop();

        if (human)
        {
            human.SetToFalling();
        }
        if (konami && !isMiniatureAlien)
        {
            int alienCount = 3;
            gameMaster.IncreaseWaveEnemyCount(alienCount);
            for (int i = 0; i < alienCount; i++)
            {
                Vector2 offset = GetRandomVectorInRange(0.7f);
                Instantiate(
                    miniatureAlien,
                    new Vector3(transform.position.x + offset.x, transform.position.y + offset.y, transform.position.z),
                    Quaternion.identity
                );
            }
        }
        base.DestroySelf();
    }
}
