using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;

    public int bombsCount = 3;
    private int lives;
    public int score;
    public int timeFreezeAmountRemaining = 0;
    public Dictionary<PowerupObj, bool> powerups = new Dictionary<PowerupObj, bool>();
    public bool completedTutorial = false;

    Constants constants;

    private int scoreTracker;

    private void Awake()
    {
        instance = this;

        lives = 3;
        PowerupObj.onGetPowerup += OnPowerupActivate;
    }

    private void Start()
    {
        constants = Constants.instance;
        foreach (PowerupObj powerup in Constants.instance.powerups)
        {
            powerups.Add(powerup, false);
        }
    }

    private void OnDestroy()
    {
        PowerupObj.onGetPowerup -= OnPowerupActivate;
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
        bool isActive = false;
        PowerupObj powerupObj = PowerupKeyByEnum(powerupEnum);
        if (powerupObj)
        {
            powerups.TryGetValue(powerupObj, out isActive);
        }
        return isActive;
    }

    public int NumPowerupsActive()
    {
        return powerups.Where(x => x.Value == true).Count();
    }

    void OnPowerupActivate(Powerup powerupEnum)
    {
        PowerupObj powerup = PowerupKeyByEnum(powerupEnum);
        if (powerup.enableable)
        {
            powerups[powerup] = true;
        }
        if (powerupEnum == Powerup.ExtraLife)
        {
            lives++;
        }
        if (powerupEnum == Powerup.Bomb)
        {
            bombsCount++;
        }
        if (powerupEnum == Powerup.TimeFreeze)
        {
            timeFreezeAmountRemaining += constants.timeFreezeAmount;
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
            if (key.powerupEnum == Powerup.TimeFreeze) continue;
            powerups[key] = false;
        }
    }

    /// <summary>
    /// Get a random powerup that is currently available
    /// Powerup must not be enabled and must be available for the current wave number
    /// </summary>
    /// <returns>Powerup to use</returns>
    public PowerupObj GetRandomPowerup()
    {
        int waveNumber = GameMaster.instance.waveCount;
        List<PowerupObj> availablePowerups = powerups.Where(
            x => x.Value == false && x.Key.minWave <= waveNumber
        ).Select(x => x.Key).ToList();

        int randomNum = Random.Range(0, availablePowerups.Count);
        return availablePowerups[randomNum];
    }
}
