using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienBullet : MonoBehaviour
{

    public Vector2 direction;
    public float speed = 1;
    public float destroyAfter = 10f;
    float shrinkSpeed = 0.98f;
    public int shrinkCounter = 30;

    Utilities utilities;

    private void Start()
    {
        utilities = Utilities.instance;
        StartCoroutine(DelayedDestroy());
    }

    void Update()
    {
        if (utilities.gameState == Utilities.GameState.STOPPED) return;
        transform.Translate(direction * speed * Time.deltaTime);
    }

    IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(destroyAfter);
        while (shrinkCounter > 0)
        {
            transform.localScale *= shrinkSpeed;
            shrinkCounter--;
            yield return null;
        }
        Destroy(gameObject);
    }
}
