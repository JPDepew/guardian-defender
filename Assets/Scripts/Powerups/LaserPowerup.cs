using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserPowerup : ScreenWrappingObject
{
    public GameObject explosion;
    public enum Powerup { Laser, Shield }
    public Powerup powerups;

    public delegate void OnGetPowerup(Powerup powerup);
    public static event OnGetPowerup onGetPowerup;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            if (powerups == Powerup.Laser)
            {
                PlayerStats.instance.bigLaser = true;
            }
            if(powerups == Powerup.Shield)
            {
                PlayerStats.instance.shield = true;
                if(onGetPowerup != null)
                {
                    onGetPowerup(Powerup.Shield);
                }
            }
            Instantiate(explosion, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
