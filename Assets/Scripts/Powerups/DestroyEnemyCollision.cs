using System.Collections;
using UnityEngine;

public class DestroyEnemyCollision : MonoBehaviour
{
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Alien")
        {
            if (collision.transform.parent != transform.parent)
            {
                Enemy enemy = collision.GetComponent<Enemy>();
                enemy.DamageSelf(12, new Vector2(enemy.transform.position.x, transform.position.y));
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
            alien.DamageSelf(3, collision.transform.position);
        }
    }
}
