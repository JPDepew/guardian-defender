using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public ShipController ship;
    Transform target;
    private void Start()
    {
        ship = FindObjectOfType<ShipController>();
        target = ship.transform;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject temp = GameObject.FindWithTag("Player");
            if (temp != null)
            {
                ship = temp.GetComponent<ShipController>();
                target = ship.transform;
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, new Vector3(target.position.x, transform.position.y, -10), 0.8f);
        }
    }
}
