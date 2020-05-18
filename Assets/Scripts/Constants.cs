using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using System.Linq;

public class Constants : MonoBehaviour
{
    public int highScore = 0;
    public int score = 0;
    public float wrapDst = 40;
    public float topOffset = 1;
    public float bottomOffset = 0.8f;
    public int humanBonus = 100;
    public int catchHumanBonus = 500;
    public int rescueHumanBonus = 500;
    public float explosionOffset = 15f;
    public float timeFreezeAmount = 100f;
    public List<PowerupObj> powerups = new List<PowerupObj>();

    string playerPrefHighScoreKey = "playerHighScore";

    static public Constants instance;

    private void Awake()
    {
        instance = this;

        SetPlayerPrefs();
    }

    public void SetHighScore()
    {
        highScore = score;
        PlayerPrefs.SetInt(playerPrefHighScoreKey, highScore);
    }

    void SetPlayerPrefs()
    {
        if (PlayerPrefs.HasKey(playerPrefHighScoreKey))
        {
            highScore = PlayerPrefs.GetInt(playerPrefHighScoreKey);
        }
        else
        {
            PlayerPrefs.SetInt(playerPrefHighScoreKey, highScore);
        }
    }

    public PowerupObj PowerupObjByEnum(Powerup powerupEnum)
    {
        return powerups.FirstOrDefault(x => x.powerupEnum == powerupEnum);
    }

    public void resetScore()
    {
        score = 0;
    }

    public void setScore(int newScore)
    {
        score = newScore;
    }
}
