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

    Constants constants;

    private int scoreTracker;

    private void Awake()
    {
        instance = this;

        lives = 3;
        PowerupObj.onGetPowerup += OnPowerupActivate;
        PlayerPowerups.onBomb += OnBomb;
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
        PlayerPowerups.onBomb -= OnBomb;
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
            print(powerupEnum == Powerup.Bomb);
            bombsCount++;
        }
        if (powerupEnum == Powerup.TimeFreeze)
        {
            timeFreezeAmountRemaining += constants.timeFreezeAmount;
        }
    }

    /// <summary>
    /// Decrement the bombs count whenever the player leaves a bomb.
    /// </summary>
    void OnBomb()
    {
        bombsCount--;
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
        //timeFreezeAmountRemaining = 0;
    }
}
