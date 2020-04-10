using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetParent : MonoBehaviour {

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "rightMapSide" || collision.tag == "leftMapSide")
        {
            transform.parent = collision.transform;
        }
    }
}
