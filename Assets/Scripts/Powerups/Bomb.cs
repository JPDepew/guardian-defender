using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float minRotateSpeed = -120, maxRotateSpeed = 120;
    public float minAbsSpeed = 50;

    public float beepWait = 0.1f;
    public float explodeWait = 0.2f;
    public float waitBtwExplosions = 0.1f;

    float rotateSpeed;

    void Start()
    {
        rotateSpeed = Random.Range(minRotateSpeed, maxRotateSpeed);
        if (Mathf.Abs(rotateSpeed) < 50)
        {
            rotateSpeed *= 2;
        }
        StartCoroutine(ExplodeTimeout());
    }

    void Update()
    {
        transform.Rotate(new Vector3(0, 0, rotateSpeed * Time.deltaTime));
    }

    IEnumerator ExplodeTimeout()
    {
        yield return new WaitForSeconds(beepWait);
        // play beep noise & red flash
        yield return new WaitForSeconds(explodeWait);
        // play first explosion
        // raycasts to the side to destroy all enemies
        yield return new WaitForSeconds(waitBtwExplosions);
        // play 2nd explosion
        // destroy all on screen
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
