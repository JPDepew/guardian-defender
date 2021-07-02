using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crawler : Enemy
{
    public float maxXDstToPlayer = 3;
    public float moveSpeed = 4;
    public float maxTurnAngle = 21;
    public List<GameObject> legs;
    public GameObject cannon;
    public float rotationLerpTime = 0.05f;
    public float shootWaitSeconds = 3f;
    public GameObject bullet;

    public GroundLineRenderer frontGroundLineRenderer;

    private float linePosY;
    private float verticalHalfSize;

    enum State { WALKING, SHOOTING, DEFAULT };
    State state = State.DEFAULT;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(FindPlayer());
        StartCoroutine(SetDirectionToPlayerInterval());
        // initialize at a big number so it won't shoot at start
        dirToPlayer = new Vector2(100, 100);

        verticalHalfSize = Camera.main.orthographicSize;
        frontGroundLineRenderer = GameObject.FindGameObjectWithTag("Ground Line Renderer").GetComponent<GroundLineRenderer>();
    }

    protected override void Update()
    {
        base.Update();
        linePosY = frontGroundLineRenderer.GetWorldYPointRounded(transform.position.x);
        if (!player && !findingPlayer)
        {
            StartCoroutine(FindPlayer());
        }
        else
        {
            ControlState();
            HandleMovement();
        }
    }

    public override bool DamageSelf(float damage, Vector2 hitPosition, Vector2? bulletDirection = null)
    {
        soundPlayer.PlayRandomSoundFromRange(0, 5);
        return base.DamageSelf(damage, hitPosition, bulletDirection);
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
        if (!float.IsNaN(smoothedAngle))
        {
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, smoothedAngle));
        }

        transform.Translate(targetDir.normalized * Time.deltaTime);
    }

    void ControlState()
    {
        if (Mathf.Abs(dirToPlayer.x) < maxXDstToPlayer)
        {
            if (state != State.SHOOTING)
            {
                audioSources[6].Stop();
                StopCoroutine("AnimateCrawling");
                StartCoroutine("PointAtPlayer");
                StartCoroutine("ShootPlayer");
                AnimateBlank();
                state = State.SHOOTING;
            }
        }
        else
        {
            if (state != State.WALKING)
            {
                audioSources[6].Play();
                StartCoroutine("AnimateCrawling");
                StopCoroutine("PointAtPlayer");
                StopCoroutine("ShootPlayer");
                state = State.WALKING;
            }
        }
    }

    IEnumerator PointAtPlayer()
    {
        while (true)
        {
            float angle = Vector2.SignedAngle(cannon.transform.up, dirToPlayer);
            float angleToRotate = Mathf.Lerp(0, angle, rotationLerpTime);

            cannon.transform.Rotate(Vector3.forward, angleToRotate);
            float clampZAngle = cannon.transform.localEulerAngles.z;
            if (clampZAngle > 180)
            {
                clampZAngle -= 360;
            }
            clampZAngle = Mathf.Clamp(clampZAngle, -maxTurnAngle, maxTurnAngle);
            cannon.transform.localEulerAngles = new Vector3(
                cannon.transform.localEulerAngles.x,
                cannon.transform.localEulerAngles.y,
                clampZAngle
            );
            yield return null;
        }
    }
    IEnumerator ShootPlayer()
    {
        while (true)
        {
            Instantiate(bullet, cannon.transform.position, cannon.transform.rotation);
            yield return new WaitForSeconds(shootWaitSeconds);
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
            legAnim.Play("CrawlerLeg_Forward");
        }
        else
        {
            legAnim.Play("CrawlerLeg_Back");
        }
    }
}
