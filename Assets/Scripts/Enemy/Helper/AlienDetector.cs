using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienDetector : MonoBehaviour
{
    Alien alien;
    AlienShoot alienShoot;

    void Awake()
    {
        alien = GetComponentInParent<Alien>();
        alienShoot = GetComponentInParent<AlienShoot>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Human" && alien.curState == Alien.State.PATROLLING)
        {
            Human human = collision.GetComponent<Human>();

            if (human.curState == Human.State.GROUNDED || human.curState == Human.State.FALLING)
            {
                alien.ChaseHuman(human);
                alien.curState = Alien.State.CHASING;
            }
        }
        if(collision.tag == "Player" && alien.curState == Alien.State.PATROLLING)
        {
            alienShoot.StartAlienShooting(collision.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            alienShoot.StopAlienShooting();
        }
    }
}
