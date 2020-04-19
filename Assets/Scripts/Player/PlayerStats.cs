using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;

    public int bombsCount = 0;
    private int lives;
    public int score;
    public Dictionary<PowerupObj, bool> powerups = new Dictionary<PowerupObj, bool>();

    private int scoreTracker;

    private void Awake()
    {
        instance = this;

        lives = 3;
        PowerupObj.onGetPowerup += OnPowerupActivate;
    }

    private void Start()
    {
        foreach (PowerupObj powerup in Constants.instance.powerups)
        {
            powerups.Add(powerup, false);
        }
    }

    public int GetLives()
    {
        return lives;
    }

    public void IncreaseScoreBy(int amount)
    {
        score += amount;
        scoreTracker += amount;
        if (scoreTracker > 50000)
        {
            IncrementLives();
            scoreTracker = 0;
        }
    }

    public bool IsPowerupActive(Powerup powerupEnum)
    {
        return powerups[PowerupKeyByEnum(powerupEnum)];
    }

    void OnPowerupActivate(Powerup powerupEnum)
    {
        PowerupObj powerup = PowerupKeyByEnum(powerupEnum);
        powerups[powerup] = true;
        if (powerupEnum == Powerup.Bomb)
        {
            bombsCount++;
        }
    }

    PowerupObj PowerupKeyByEnum(Powerup powerupEnum)
    {
        return powerups.FirstOrDefault(x => x.Key.powerupEnum == powerupEnum).Key;
    }

    public int GetScore()
    {
        return score;
    }
    public void DecrementLives()
    {
        lives--;
    }

    public void IncrementLives()
    {
        lives++;
        ShipController s = FindObjectOfType<ShipController>();
        if (s != null)
        {
            s.InitializeHealthIndicators();
        }
    }

    public void ResetAllPowerups()
    {
        List<PowerupObj> keys = new List<PowerupObj>(powerups.Keys);
        foreach (var key in keys)
        {
            powerups[key] = false;
        }
    }
}
