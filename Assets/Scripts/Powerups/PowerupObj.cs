using System.Linq;
using UnityEngine;

public class PowerupObj : ScreenWrappingObject
{
    public GameObject explosion;
    public Powerup powerupEnum;
    public string powerupName;
    public KeyCode keyCode;
    public int minWave = 0;
    public bool enableable = true;

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
            Activate();
        }
    }

    public void Activate()
    {
        // Invoke method to set active on the player
        InvokePowerup();
        Instantiate(explosion, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    public void InvokePowerup()
    {
        onGetPowerup?.Invoke(powerupEnum);
    }
}
