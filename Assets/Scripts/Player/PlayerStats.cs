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
    public Dictionary<PowerupObj, int> powerups = new Dictionary<PowerupObj, int>();
    public bool completedTutorial = false;

    Constants constants;
    Data data;

    private int scoreTracker;

    public delegate void OnGatherAllPowerups();
    public static event OnGatherAllPowerups onGatherAllPowerups;

    private void Awake()
    {
        instance = this;

        lives = 3;
        PowerupObj.onGetPowerup += OnPowerupActivate;
    }

    private void Start()
    {
        constants = Constants.instance;
        data = Data.Instance;
        foreach (PowerupObj powerup in Constants.instance.powerups)
        {
            powerups.Add(powerup, powerup.defaultCount);
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
        int isActive = 0;
        PowerupObj powerupObj = PowerupKeyByEnum(powerupEnum);
        if (powerupObj)
        {
            powerups.TryGetValue(powerupObj, out isActive);
        }
        return isActive > 0;
    }

    public int NumPowerupsActive()
    {
        return powerups.Where(x => x.Value > 0).Count();
    }

    void OnPowerupActivate(Powerup powerupEnum)
    {
        PowerupObj powerup = PowerupKeyByEnum(powerupEnum);
        if (powerup.enableable)
        {
            powerups[powerup] = 1;
        }
        else
        {
            powerups[powerup] += powerup.increaseAmt;
        }
        if (powerupEnum == Powerup.ExtraLife)
        {
            IncrementLives();
        }
        if (powerupEnum == Powerup.Bomb)
        {
            bombsCount++;
        }
        if (powerupEnum == Powerup.TimeFreeze)
        {
            timeFreezeAmountRemaining += constants.timeFreezeAmount; // TODO - make this work with what's stored in the powerup. Maybe a decrement function on the player powerups. (probably??)
        }
    }

    PowerupObj PowerupKeyByEnum(Powerup powerupEnum)
    {
        return powerups.FirstOrDefault(x => x.Key.powerupEnum == powerupEnum).Key;
    }

    public int PowerupValueByEnum(Powerup powerupEnum)
    {
        return powerups.FirstOrDefault(x => x.Key.powerupEnum == powerupEnum).Value;
    }

    public void DecrementPowerupValue(Powerup powerupEnum)
    {
        PowerupObj powerupObj = PowerupKeyByEnum(powerupEnum);
        powerups[powerupObj] -= powerupObj.increaseAmt;
    }

    public int GetScore()
    {
        return score;
    }
    public void DecrementLives()
    {
        lives--;
        DecrementPowerupValue(Powerup.ExtraLife);
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
            if (key.powerupEnum == Powerup.TimeFreeze || key.powerupEnum == Powerup.ExtraLife) continue;
            powerups[key] = 0;
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
            onGatherAllPowerups?.Invoke();
        List<PowerupObj> availablePowerups = powerups.Where(
            x => x.Key.CanBeDropped()
            && (x.Key.minWave <= waveNumber || data.konamiEnabled)
        ).Select(x => x.Key).ToList();
        if (availablePowerups.Count > 0)
        {
            int randomNum = Random.Range(0, availablePowerups.Count);
            return availablePowerups[randomNum];
        }
        if (data.konamiEnabled)
        {
            onGatherAllPowerups?.Invoke();
        }
        availablePowerups = powerups.Where(x => !x.Key.hardLimit).Select(x => x.Key).ToList();
        int index = Random.Range(0, availablePowerups.Count);
        return availablePowerups[index];
    }
}
