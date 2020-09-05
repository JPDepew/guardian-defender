using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenWrappingObject : MonoBehaviour
{
    public float wrapDstMultiplier = 1;
    protected Constants constants;

    protected bool shouldWrap = true;

    private Transform mainCam;
    private float wrapDst = 100;

    protected virtual void Start()
    {
        constants = Constants.instance;
        mainCam = Camera.main.transform;
        wrapDst = constants.wrapDst * wrapDstMultiplier;
    }

    protected virtual void Update()
    {
        if (!shouldWrap) return;
        // The camera is too far to the right
        if (mainCam.position.x - transform.position.x > wrapDst)
        {
            transform.position = new Vector3(mainCam.position.x + wrapDst - 1, transform.position.y, transform.position.z);
        }
        // Cam is too far to left
        else if (mainCam.position.x - transform.position.x < -wrapDst)
        {
            transform.position = new Vector3(mainCam.position.x - wrapDst + 1, transform.position.y, transform.position.z);
        }
    }
}
