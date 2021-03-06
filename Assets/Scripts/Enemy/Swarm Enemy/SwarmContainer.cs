﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SwarmContainer : Enemy
{
    public float minDiffValue = 15;
    public float thrusterStrength = 3;
    public float thrusterLength = 30;
    public float topBound = 2;
    public float bottomBound = -2;
    public float minEngineWaitTime = 0.5f;
    public float maxEngineWaitTime = 3f;
    public float maxVelocityMagnitude = 1f;

    public Transform swarmAttackContainer;
    public GameObject body;

    List<Transform> engines;
    Rigidbody2D rigidbody2d;
    private SwarmPart[] metalChildren;
    private SwarmAttacker[] swarmAttackers;

    protected override void Start()
    {
        base.Start();
        rigidbody2d = GetComponent<Rigidbody2D>();
        engines = GetComponentsInChildren<Transform>().Where(t => t.tag == "SwarmEngine").ToList();
        metalChildren = GetComponentsInChildren<SwarmPart>();
        swarmAttackers = GetComponentsInChildren<SwarmAttacker>();

        StartCoroutine(DelayedStart());
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    IEnumerator DelayedStart()
    {
        body.SetActive(false);
        yield return new WaitForSeconds(1);
        body.SetActive(true);
        StartCoroutine("MoveShip");
    }

    public override bool DamageSelf(float damage, Vector2 hitPosition, Vector2? bulletDirection)
    {
        if (bulletDirection != null)
        {
            soundPlayer.PlayRandomSoundFromRange(0, 1, 0.9f, 1.1f);
            Instantiate(hit, hitPosition, Quaternion.identity);
            rigidbody2d.AddForceAtPosition((Vector2)bulletDirection * 5, hitPosition);
        }
        return true;
    }

    public override void KonamiAction()
    {
        base.KonamiAction();
        minEngineWaitTime = 0.1f;
        maxEngineWaitTime = 0.4f;
        thrusterLength = 10;
        thrusterStrength = 2f;
    }

    public void StartDestroy()
    {
        DestroySelf();
    }

    IEnumerator MoveShip()
    {
        while (true)
        {
            if (player == null)
            {
                while (player == null)
                {
                    yield return FindPlayer();
                }
            }
            Vector2 directionToMove = (player.transform.position - transform.position).normalized;
            List<Transform> enginesToUse;

            if (ShouldMoveUp())
            {
                directionToMove = Vector2.up + directionToMove;
                enginesToUse = GetNearestEngineDirections(directionToMove);
            }
            else if (ShouldMoveDown())
            {
                directionToMove = Vector2.down + directionToMove;
                enginesToUse = GetNearestEngineDirections(directionToMove);
            }
            else
            {
                enginesToUse = GetNearestEngineDirections(directionToMove);
            }

            for (int i = 0; i < enginesToUse.Count; i++)
            {
                audioSources[2].Play();
                enginesToUse[i].GetComponentInChildren<ParticleSystem>().Play();
            }
            int counter = 0;
            while (counter <= thrusterLength)
            {
                counter++;
                for (int i = 0; i < enginesToUse.Count; i++)
                {
                    rigidbody2d.AddForceAtPosition(enginesToUse[i].up * thrusterStrength, enginesToUse[i].position);
                    rigidbody2d.velocity = Vector2.ClampMagnitude(rigidbody2d.velocity, maxVelocityMagnitude);
                }
                yield return null;
            }
            for (int i = 0; i < enginesToUse.Count; i++)
            {
                enginesToUse[i].GetComponentInChildren<ParticleSystem>().Stop();
            }
            float waitTime = Random.Range(minEngineWaitTime, maxEngineWaitTime);
            yield return new WaitForSeconds(waitTime);
        }
    }

    /// <summary>
    /// Compares the angles to the compareVector and the angles that each engine is pointing at.
    /// Returns a list of stationary engines that point in the direction closest to the given vector.
    /// If there are 2 engines that both point close to the direction of the given vector it will return both
    /// </summary>
    /// <param name="compareVector">The vector to compare to</param>
    /// <returns>A list of engines that point in the direction closest to the given vector, max count of 2</returns>
    List<Transform> GetNearestEngineDirections(Vector2 compareVector)
    {
        float angleToPlayer = Mathf.Atan2(compareVector.y, compareVector.x) * Mathf.Rad2Deg;
        if (angleToPlayer < 0)
        {
            angleToPlayer += 360f;
        }

        float smallestDiff = float.MaxValue;
        Queue<Transform> enginesToUse = new Queue<Transform>();
        Queue<float> smallestDiffs = new Queue<float>();
        for (int i = 0; i < engines.Count; i++)
        {
            float engineAngle = Mathf.Atan2(engines[i].up.y, engines[i].up.x) * Mathf.Rad2Deg;
            if (engineAngle < 0)
            {
                engineAngle += 360f;
            }
            float angleDiff = Mathf.Abs(Mathf.Abs(engineAngle) - Mathf.Abs(angleToPlayer));
            if (angleDiff < smallestDiff)
            {
                enginesToUse.Enqueue(engines[i]);
                if (enginesToUse.Count > 2)
                {
                    enginesToUse.Dequeue();
                }
                smallestDiffs.Enqueue(angleDiff);
                if (smallestDiffs.Count > 2)
                {
                    smallestDiffs.Dequeue();
                }
                smallestDiff = angleDiff;
            }
        }
        if (smallestDiffs.Count > 1 && enginesToUse.Count > 1)
        {
            float first = smallestDiffs.Dequeue();
            float second = smallestDiffs.Dequeue();
            if (Mathf.Abs(first - second) > minDiffValue)
            {
                enginesToUse.Dequeue();
            }
        }
        return enginesToUse.ToList();
    }

    /// <summary>
    /// If the transform is too low or if it is moving downward too quickly and below a certain level
    /// </summary>
    /// <returns>True if the transform should move upward</returns>
    private bool ShouldMoveUp()
    {
        float maxDownwardYVelocity = -0.3f;
        return transform.position.y < bottomBound
            || (rigidbody2d.velocity.y < maxDownwardYVelocity && transform.position.y < bottomBound + 1);
    }

    private bool ShouldMoveDown()
    {
        float maxUpwardYVelocity = 0.3f;
        return transform.position.y > topBound
            || (rigidbody2d.velocity.y > maxUpwardYVelocity && transform.position.y > topBound - 1);
    }

    protected override void DestroySelf()
    {
        foreach (SwarmPart swarmPart in metalChildren)
        {
            StopCoroutine("MoveShip");
            swarmPart.FlyOffDestroy(transform.position);
        }
        // Activate all swarm attack children
        ActivateSwarmAttackers();
    }

    void ActivateSwarmAttackers()
    {
        foreach (SwarmAttacker swarmAttacker in swarmAttackers)
        {
            swarmAttacker.Activate();
        }
    }
}
