using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void resetScore()
    {
        score = 0;
    }

    public void setScore(int newScore)
    {
        score = newScore;
    }
}
