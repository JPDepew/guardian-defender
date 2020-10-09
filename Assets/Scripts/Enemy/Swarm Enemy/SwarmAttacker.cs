using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmAttacker : Enemy
{
    public float lerpTime = 0.5f;
    public float speed = 10;
    public float maxVelocity = 10;
    public float dstToPlayer = 3;

    Vector3 velocity;
    Rigidbody2D rb2D;

    protected override void Start()
    {
        base.Start();
        rb2D = GetComponent<Rigidbody2D>();
        StartCoroutine(FindPlayer());
        StartCoroutine(GetDirectionToPlayer());
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (player)
        {
            if (Mathf.Abs(direction.x) < dstToPlayer)
            {
                float angle = Vector2.SignedAngle(transform.up, direction);
                float angleToRotate = Mathf.Lerp(0, angle, lerpTime);
                transform.Rotate(Vector3.forward, angleToRotate);
            }
            else
            {
                // check closeness to bounds to move up or down
                float angle = Vector2.SignedAngle(transform.up, new Vector2(direction.x, 0));
                float angleToRotate = Mathf.Lerp(0, angle, lerpTime);
                transform.Rotate(Vector3.forward, angleToRotate);
                if (rb2D.velocity.magnitude < maxVelocity)
                {
                    rb2D.AddForce(transform.up * Time.deltaTime * .1f);
                }
            }

            print(rb2D.velocity);
            print(rb2D.velocity.normalized);
            //velocity += transform.up * Time.deltaTime * speed;
            //transform.position = transform.position + velocity;
        }
        else
        {
            StartCoroutine(FindPlayer());
        }
    }

    IEnumerator GetDirectionToPlayer()
    {
        while (true)
        {
            if (player)
            {
                direction = player.transform.position - transform.position;
            }
            else
            {
                direction = Vector2.zero;
            }
            yield return new WaitForSeconds(0.2f);
        }
    }
}
