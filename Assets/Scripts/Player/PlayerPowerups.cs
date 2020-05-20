using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPowerups : MonoBehaviour
{
    private PlayerStats playerStats;
    private Constants constants;

    private KeyCode timeFreezeKeyCode;

    private bool timeFreeze = false;
    
    void Start()
    {
        constants = Constants.instance;
        playerStats = PlayerStats.instance;
        timeFreezeKeyCode = constants.PowerupObjByEnum(Powerup.TimeFreeze).keyCode;
    }

    void Update()
    {
        if (Input.GetKeyDown(timeFreezeKeyCode))
        {
            StopCoroutine("ChangeTimeScale");
            StartCoroutine("ChangeTimeScale");
        }
        if (timeFreeze)
        {
            playerStats.timeFreezeAmountRemaining -= 1;
            print(playerStats.timeFreezeAmountRemaining);
        }
    }

    IEnumerator ChangeTimeScale()
    {
        float min = 0.005f;
        float targetTimeScale = Time.timeScale > 0.5f ? min : 1;
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
