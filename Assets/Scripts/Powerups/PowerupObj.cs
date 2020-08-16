using System.Linq;
using UnityEngine;

public class PowerupObj : ScreenWrappingObject
{
    public GameObject explosion;
    public Powerup powerupEnum;
    public string powerupName;
    public KeyCode keyCode;

    private PlayerStats playerStats;

    public delegate void OnGetPowerup(Powerup powerup);
    public static event OnGetPowerup onGetPowerup;

    protected override void Start()
    {
        base.Start();
        playerStats = PlayerStats.instance;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            // Set powerup to active
            PowerupObj powerupObj = playerStats.powerups.FirstOrDefault(x => x.Key.powerupEnum == powerupEnum).Key;
            playerStats.powerups[powerupObj] = true;
            
            // Invoke method to set active on the player
            onGetPowerup?.Invoke(powerupEnum);
            Instantiate(explosion, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
