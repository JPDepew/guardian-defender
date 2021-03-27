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
    public bool hardLimit = false;
    public int defaultCount = 0;
    public int increaseAmt = 1;
    public int maxCount = 1;

    public delegate void OnGetPowerup(Powerup powerup);
    public static event OnGetPowerup onGetPowerup;

    protected override void Start()
    {
        base.Start();
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
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

    /// <summary>
    /// Should the saucer drop this
    /// </summary>
    public virtual bool CanBeDropped()
    {
        return PlayerStats.instance.PowerupValueByEnum(powerupEnum) < maxCount;
    }
}
