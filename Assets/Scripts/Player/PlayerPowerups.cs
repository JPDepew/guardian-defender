using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPowerups : MonoBehaviour
{
    public GameObject bomb;

    private PlayerStats playerStats;
    private Constants constants;

    private KeyCode timeFreezeKeyCode;
    private KeyCode bombKeyCode;

    private bool timeFreeze = false;

    public delegate void OnBomb();
    public static event OnBomb onBomb;
    public delegate void OnTimeFreeze();
    public static event OnTimeFreeze onTimeFreeze;

    void Start()
    {
        constants = Constants.instance;
        playerStats = PlayerStats.instance;
        timeFreezeKeyCode = constants.PowerupObjByEnum(Powerup.TimeFreeze).keyCode;
        bombKeyCode = constants.PowerupObjByEnum(Powerup.Bomb).keyCode;

        PowerupObj.onGetPowerup += OnPowerupActivate;
    }

    private void Update()
    {
        HandleBomb();
    }

    private void OnDestroy()
    {
        PowerupObj.onGetPowerup -= OnPowerupActivate;
    }

    void OnPowerupActivate(Powerup powerup)
    {
        switch (powerup)
        {
            case Powerup.TimeFreeze:
                StopCoroutine("TimeFreezeActive");
                StartCoroutine("TimeFreezeActive");
                break;
        }
    }

    void HandleBomb()
    {
        if (Input.GetKeyDown(bombKeyCode))
        {
            if (playerStats.bombsCount > 0)
            {
                Instantiate(bomb, transform.position, transform.rotation);
                playerStats.bombsCount--;
                onBomb?.Invoke();
            }
        }
    }

    IEnumerator TimeFreezeActive()
    {
        while (playerStats.timeFreezeAmountRemaining > 0)
        {
            if (Input.GetKeyDown(timeFreezeKeyCode) && playerStats.IsPowerupActive(Powerup.TimeFreeze) && playerStats.timeFreezeAmountRemaining > 0)
            {
                onTimeFreeze?.Invoke();
                StopCoroutine("ChangeTimeScale");
                StartCoroutine("ChangeTimeScale", false);
            }
            if (timeFreeze)
            {
                playerStats.timeFreezeAmountRemaining -= 1;
            }
            yield return null;
        }
        onTimeFreeze?.Invoke();
        StopCoroutine("ChangeTimeScale");
        StartCoroutine("ChangeTimeScale", true);
    }

    IEnumerator ChangeTimeScale(bool overrideSpeedUp = false)
    {
        float min = 0.005f;
        float max = 1;
        float targetTimeScale = Time.timeScale < 0.5f || overrideSpeedUp ? max : min;
        float fadeSpeed = 0.05f;
        float sign = Mathf.Sign(targetTimeScale - Time.timeScale);
        bool shouldChange = true;

        timeFreeze = !timeFreeze;
        while (Time.timeScale >= min && Time.timeScale <= 1 || shouldChange)
        {
            float newValue = Time.timeScale + fadeSpeed * sign;
            Time.timeScale = Mathf.Clamp(newValue, min, 1);
            shouldChange = false;
            yield return null;
        }
    }
}
