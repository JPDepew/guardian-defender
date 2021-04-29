using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crawler : Enemy
{
    public float maxXDstToPlayer = 3;
    public float moveSpeed = 4;
    public List<GameObject> legs;

    public GroundLineRenderer frontGroundLineRenderer;

    private float linePosY;
    private float verticalHalfSize;

    enum State { WALKING, SHOOTING };
    State state = State.WALKING;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(FindPlayer());
        StartCoroutine(SetDirectionToPlayerInterval());

        verticalHalfSize = Camera.main.orthographicSize;
        frontGroundLineRenderer = GameObject.FindGameObjectWithTag("Ground Line Renderer").GetComponent<GroundLineRenderer>();
        linePosY = frontGroundLineRenderer.GetWorldYPointRounded(transform.position.x) - constants.negativeHumanOffset;
    }

    protected override void Update()
    {
        base.Update();
        linePosY = frontGroundLineRenderer.GetWorldYPointRounded(transform.position.x) - 1;
        ControlState();
        if (!player && !findingPlayer)
        {
            StartCoroutine(FindPlayer());
        }
        HandleMovement();
    }

    void HandleMovement()
    {
        if (state == State.SHOOTING) return;
        print(linePosY);
        float targetYMovement = Mathf.Clamp(linePosY, -(verticalHalfSize - 0.5f), 100);
        print("target");
        print(targetYMovement);
        int moveDirection = (int)Mathf.Sign(dirToPlayer.x);
        Vector2 targetPos = new Vector2(
            transform.position.x + moveSpeed * moveDirection * Time.deltaTime,
            targetYMovement
        );

        transform.position = Vector2.Lerp(transform.position, targetPos, 0.5f);
    }

    void ControlState()
    {
        if (Mathf.Abs(dirToPlayer.x) < maxXDstToPlayer)
        {
            if (state != State.SHOOTING)
            {
                StopCoroutine("AnimateCrawling");
                AnimateBlank();
                state = State.SHOOTING;
            }
        }
        else
        {
            if (state != State.WALKING)
            {
                StartCoroutine("AnimateCrawling");
                state = State.WALKING;
            }
        }
    }

    IEnumerator SetDirectionToPlayerInterval()
    {
        while (true)
        {
            SetDirectionToPlayer();
            yield return new WaitForSeconds(0.1f);
        }
    }


    IEnumerator AnimateCrawling()
    {
        foreach (GameObject leg in legs)
        {
            string tagToUse = Mathf.Sign(dirToPlayer.x) > 0 ? "LeftLeg" : "RightLeg";
            PlayLegAnimation(tagToUse, leg);
            yield return new WaitForSeconds(0.1f);
        }
    }

    void AnimateBlank()
    {
        foreach (GameObject leg in legs)
        {
            Animator legAnim = leg.GetComponent<Animator>();
            legAnim.Play("Blank");
        }
    }

    void PlayLegAnimation(string legTag, GameObject leg)
    {
        Animator legAnim = leg.GetComponent<Animator>();
        if (leg.tag == legTag)
        {
            print("forward");
            legAnim.Play("CrawlerLeg_Forward");
        }
        else
        {
            print("back");
            legAnim.Play("CrawlerLeg_Back");
        }
    }
}
