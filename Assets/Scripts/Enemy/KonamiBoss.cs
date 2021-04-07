using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KonamiBoss : Enemy
{
    public float getPlayerDirectionInterval = 1;
    public float linearInterpolationTime = 0.5f;
    public float approachPlayerSpeed = 30;
    public float descendSpeed = 2;
    public float dstToChomp = 3;
    public float dstToSlowdown = 6;
    public float yTargetOffset = 1;
    public float speedLinearInterpolation = 0.2f;
    public float attackWaitTime = 10;

    float speed = 1;
    Vector2 directionToMove;
    Animator animator;
    Data data;
    enum State { APPROACHING, VISIBLE, ATTACK, PROPELLOR, LEAVING };
    State state = State.APPROACHING;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        data = Data.Instance;
        health = data.konamiBossHealth;
        animator = GetComponent<Animator>();
        StartChase();
    }

    public void StartChase()
    {
        gameObject.SetActive(true);
        StartCoroutine(FindPlayer());
        StartCoroutine(SetDirectionToMove());
        StartCoroutine(SetDirectionToPlayerEveryInterval());
        StartCoroutine(Move());
    }

    protected override void Update()
    {
        base.Update();
        animator.SetBool("isPlayerAlive", player != null);
        animator.SetFloat("dstToPlayer", dirToPlayer.magnitude);
        if (state == State.LEAVING && !spriteRenderer.isVisible)
        {
            state = State.APPROACHING;
            StopAllCoroutines();
            gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        data.konamiBossHealth = (int)health;
    }

    IEnumerator SetDirectionToPlayerEveryInterval()
    {
        while (true)
        {
            SetDirectionToPlayer();
            if (Mathf.Sign(dirToPlayer.x) > 0)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
            }
            else
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y);
            }
            yield return new WaitForSecondsRealtime(getPlayerDirectionInterval);
        }
    }

    IEnumerator SetDirectionToMove()
    {
        while (true)
        {
            if (player)
            {
                if (state == State.VISIBLE)
                {
                    float playerSpeed = player.GetPlayerSpeed();
                    directionToMove = Vector2.Lerp(
                        directionToMove,
                        new Vector2(Mathf.Sign(playerSpeed) * Mathf.Abs(dirToPlayer.x), dirToPlayer.y + yTargetOffset),
                        linearInterpolationTime
                    ).normalized;
                }
                else
                {
                    directionToMove = Vector2.Lerp(
                        directionToMove,
                        new Vector2(dirToPlayer.x, dirToPlayer.y + yTargetOffset),
                        linearInterpolationTime
                    ).normalized;
                }
            }
            else
            {
                directionToMove = Vector2.down;
                state = State.LEAVING;
                break;
            }
            yield return null;
        }
    }

    IEnumerator Move()
    {
        while (true)
        {
            speed = Mathf.Lerp(speed, GetSpeed(), GetSpeedLinearInterpolation());
            transform.Translate(directionToMove * speed * Time.unscaledDeltaTime);
            yield return null;
        }
    }

    float GetSpeed()
    {
        if (!player)
        {
            return descendSpeed;
        }
        float absPlayerSpeed = Mathf.Abs(player.GetPlayerSpeed());
        if (dirToPlayer.magnitude < dstToChomp)
        {
            return absPlayerSpeed + approachPlayerSpeed;
        }
        if (dirToPlayer.magnitude < dstToSlowdown)
        {
            if (state == State.APPROACHING)
            {
                state = State.VISIBLE;
                StartCoroutine(AttackWaitTimer());
            }
            if (state == State.VISIBLE)
            {
                print("visible");
                return absPlayerSpeed;
            }
            return absPlayerSpeed + 0.7f;
        }
        return absPlayerSpeed + approachPlayerSpeed;
    }

    IEnumerator AttackWaitTimer()
    {
        yield return new WaitForSecondsRealtime(attackWaitTime);
        state = State.ATTACK;
    }

    float GetSpeedLinearInterpolation()
    {
        if (!player)
        {
            return 1;
        }
        return speedLinearInterpolation;
    }

    public void PlayChomp()
    {
        audioSources[0].Play();
    }
}
