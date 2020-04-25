using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyEnemyCollision : MonoBehaviour
{
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Alien")
        {
            Enemy alien = collision.GetComponent<Enemy>();
            if (alien.GetComponent<Renderer>().isVisible)
            {
                alien.DamageSelf(12, transform.position);
            }
        }
        if (collision.tag == "Human")
        {
            Human human = collision.GetComponent<Human>();
            if (human.curState != Human.State.GROUNDED)
            {
                human.DamageSelf(100, transform.position);
            }
        }
        if (collision.tag == "Watch")
        {
            Enemy alien = collision.transform.parent.GetComponent<Enemy>();
            if (collision.GetComponent<Renderer>().isVisible)
            {
                alien.DamageSelf(3, collision.transform.position);
            }
        }
    }
}
