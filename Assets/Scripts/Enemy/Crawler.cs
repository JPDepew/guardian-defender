using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crawler : Enemy
{
    Animator animator;
    public float maxXDstToPlayer = 3;
    public List<GameObject> legs;
    enum State { WALKING, SHOOTING };
    State state = State.WALKING;
    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        StartCoroutine(FindPlayer());
    }

    protected override void Update()
    {
        base.Update();
        ControlState();
        switch (state)
        {
            case State.WALKING:
                StartCoroutine("AnimateCrawling");
                break;
            case State.SHOOTING:
                AnimateBlank();
                break;
        }
    }

    void ControlState()
    {
        print(dirToPlayer);
        if (dirToPlayer.x < maxXDstToPlayer)
        {
            state = State.SHOOTING;
        }
        else
        {
            state = State.WALKING;
        }
    }

    protected override IEnumerator FindPlayer()
    {
        /// lolly
        return base.FindPlayer();
    }

    IEnumerator AnimateCrawling()
    {
        foreach (GameObject leg in legs)
        {
            string tagToUse = Mathf.Sign(dirToPlayer.x) > 0 ? "RightLeg" : "LeftLeg";
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
