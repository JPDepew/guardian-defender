using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupObj : ScreenWrappingObject
{
    public GameObject explosion;
    public Powerup powerupEnum;
    public string powerupName;
    public KeyCode keyCode;

    public delegate void OnGetPowerup(Powerup powerup);
    public static event OnGetPowerup onGetPowerup;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            // Set powerup to active
            PlayerStats.instance.powerups[this] = true;
            // Invoke method to set active on the player
            onGetPowerup?.Invoke(powerupEnum);
            Instantiate(explosion, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
