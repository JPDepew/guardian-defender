using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;

    private int lives;
    public int score;
    public bool bigLaser = false;
    public bool shield = false;
    public bool speedBoost = false;
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

    public bool IsPowerupActive(PowerupObj.Powerup powerupEnum)
    {
        return powerups[powerups.FirstOrDefault(x => x.Key.powerupEnum == powerupEnum).Key];
    }

    void OnPowerupActivate(PowerupObj.Powerup powerupEnum)
    {
        PowerupObj powerup = powerups.FirstOrDefault(x => x.Key.powerupEnum == powerupEnum).Key;
        powerups[powerup] = true;
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
        bigLaser = false;
        shield = false;
        List<PowerupObj> keys = new List<PowerupObj>(powerups.Keys);
        foreach (var key in keys)
        {
            powerups[key] = false;
        }
    }
}
