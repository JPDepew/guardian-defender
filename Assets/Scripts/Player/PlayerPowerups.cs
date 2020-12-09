using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPowerups : MonoBehaviour
{
    public GameObject bomb;
    public GameObject timeFreezeTrail;

    public float timeFreezeTrailSpeed = 6;
    public float timeFreezeTrailDelay = 0.1f;

    private PlayerStats playerStats;
    private Constants constants;
    private AudioSource[] audioSources;
    private ShipController shipController;

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
        audioSources = GetComponents<AudioSource>();
        shipController = GetComponent<ShipController>();

        PowerupObj.onGetPowerup += OnPowerupActivate;

        if (IsTimeFreezeActive())
        {
            StartCoroutine("TimeFreezeActive");
        }
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
            if (Input.GetKeyDown(timeFreezeKeyCode) && IsTimeFreezeActive())
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

        if (targetTimeScale == max)
        {
            // Exit time freeze
            StartCoroutine(ActivateTimeFreezeTrails());
            audioSources[6].Play();
        }
        else
        {
            // Enter time freeze
            StartCoroutine(ActivateTimeFreezeTrails());
            audioSources[5].Play();
        }

        timeFreeze = !timeFreeze;
        while (Time.timeScale >= min && Time.timeScale <= 1 || shouldChange)
        {
            float newValue = Time.timeScale + fadeSpeed * sign;
            Time.timeScale = Mathf.Clamp(newValue, min, 1);
            Time.fixedDeltaTime = sign > 0 ? 0.02f : 0.001f;
            shouldChange = false;
            yield return null;
        }
    }

    IEnumerator ActivateTimeFreezeTrails()
    {
        StartCoroutine(ActivateTimeFreezeTrail());
        yield return new WaitForSeconds(timeFreezeTrailDelay);
        StartCoroutine(ActivateTimeFreezeTrail(true));
    }

    /// <summary>
    /// Enable and rotates the time freeze trail
    /// </summary>
    /// <param name="oppositeRotation">Should trail spin in opposite direction</param>
    /// <returns></returns>
    IEnumerator ActivateTimeFreezeTrail(bool oppositeRotation = false)
    {
        GameObject timeFreezeInstance = Instantiate(timeFreezeTrail, transform.position, transform.rotation, transform);
        Vector3 startRotation = oppositeRotation ? new Vector3(-45, 90, -90) : new Vector3(-135, 90, -90);
        timeFreezeInstance.transform.rotation = Quaternion.Euler(startRotation);
        ParticleSystem particleSystem = timeFreezeInstance.GetComponent<ParticleSystem>();
        float actualSpeed = oppositeRotation ? -timeFreezeTrailSpeed : timeFreezeTrailSpeed;
        while (particleSystem.IsAlive())
        {
            timeFreezeInstance.transform.Rotate(Vector3.up, actualSpeed);
            yield return null;
        }
    }

    private bool IsTimeFreezeActive()
    {
        return playerStats.IsPowerupActive(Powerup.TimeFreeze) && playerStats.timeFreezeAmountRemaining > 0;
    }
}
