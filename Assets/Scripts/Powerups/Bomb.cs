using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : DestroyEnemyCollision
{
    public float minRotateSpeed = -120, maxRotateSpeed = 120;
    public float minAbsSpeed = 50;

    public float startWait = 0.1f;
    public float explodeWait = 0.2f;
    public float waitBtwExplosions = 0.1f;
    public float colliderExistTime = 0.1f;
    public float thenDestroyWait = 1f;
    public float damage = 100;
    public float raycastDst = 14;

    public Vector3 velocity;
    public float yAcceleration = -0.01f;

    public ParticleSystem ps1;
    public ParticleSystem ps2;

    public LayerMask hitLayerMask;

    AudioSource[] audioSources;
    CircleCollider2D circleCollider2D;

    float rotateSpeed;

    void Start()
    {
        audioSources = GetComponents<AudioSource>();
        circleCollider2D = GetComponent<CircleCollider2D>();

        rotateSpeed = UnityEngine.Random.Range(minRotateSpeed, maxRotateSpeed);
        if (Mathf.Abs(rotateSpeed) < 50)
        {
            rotateSpeed *= 2;
        }
        StartCoroutine(ExplodeTimeout());
    }

    void Update()
    {
        HandleVelocity();
        transform.Rotate(new Vector3(0, 0, rotateSpeed * Time.deltaTime));
        transform.position = transform.position + velocity * Time.deltaTime;
    }

    void HandleVelocity()
    {
        velocity = new Vector3(velocity.x, velocity.y + yAcceleration);
    }

    IEnumerator ExplodeTimeout()
    {
        // hatch open sound?
        yield return new WaitForSeconds(startWait);
        yield return new WaitForSeconds(explodeWait);
        StartCoroutine("Raycasting");
        ps1.Play();
        audioSources[1].Play();
        yield return new WaitForSeconds(waitBtwExplosions);
        ps2.Play();
        Destroy(gameObject.GetComponent<SpriteRenderer>());
        circleCollider2D.enabled = true;
        StopCoroutine("Raycasting");
        yield return new WaitForSeconds(colliderExistTime);
        circleCollider2D.enabled = false;
        yield return new WaitForSeconds(thenDestroyWait);
        Destroy(transform.parent.gameObject);
    }

    /// <summary>
    /// Raycasting to hit all enemies to the right and left
    /// </summary>
    /// <returns>null</returns>
    IEnumerator Raycasting()
    {
        RaycastHit2D[] hitsLeft;
        RaycastHit2D[] hitsRight;

        while (true)
        {
            hitsLeft = Physics2D.RaycastAll(transform.position, transform.right, raycastDst, hitLayerMask);
            hitsRight = Physics2D.RaycastAll(transform.position, -transform.right, raycastDst, hitLayerMask);
            Debug.DrawRay(transform.position, transform.right * raycastDst, Color.red);
            Debug.DrawRay(transform.position, -transform.right * raycastDst, Color.red);

            RaycastHit2D[] allHits = new RaycastHit2D[hitsLeft.Length + hitsRight.Length];
            Array.Copy(hitsLeft, allHits, hitsLeft.Length);
            Array.Copy(hitsRight, 0, allHits, hitsLeft.Length, hitsRight.Length);

            foreach (RaycastHit2D hit in allHits)
            {
                hit.collider.GetComponent<Hittable>().DamageSelf(damage, hit.point);
            }
            yield return null;
        }
    }
}
