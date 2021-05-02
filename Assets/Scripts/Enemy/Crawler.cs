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

    enum State { WALKING, SHOOTING, DEFAULT };
    State state = State.DEFAULT;

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
        linePosY = frontGroundLineRenderer.GetWorldYPointRounded(transform.position.x);
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
        float targetYMovement = Mathf.Clamp(linePosY - transform.position.y, -(verticalHalfSize - 0.5f), 100);
        int moveDirection = (int)Mathf.Sign(dirToPlayer.x);
        Vector2 targetDir = new Vector2(
            moveDirection,
            targetYMovement
        );
        float curRotation = transform.rotation.eulerAngles.z;
        if (curRotation > 180)
        {
            curRotation -= 360;
        }
        float slope = frontGroundLineRenderer.GetSlope(transform.position.x);
        float targetAngle = Mathf.Atan(slope) * Mathf.Rad2Deg;
        float smoothedAngle = Mathf.Lerp(curRotation, targetAngle, .25f);

        transform.rotation = Quaternion.Euler(new Vector3(0, 0, smoothedAngle));

        transform.Translate(targetDir.normalized * Time.deltaTime);
    }

    void ControlState()
    {
        if (Mathf.Abs(dirToPlayer.x) < maxXDstToPlayer)
        {
            if (state != State.SHOOTING)
            {
                audioSources[0].Stop();
                StopCoroutine("AnimateCrawling");
                AnimateBlank();
                state = State.SHOOTING;
            }
        }
        else
        {
            if (state != State.WALKING)
            {
                audioSources[0].Play();
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
