using System.Collections;
using UnityEngine;

public class Konami : MonoBehaviour
{
    public GameObject konamiBoss;
    public float konamiBossWaitSeconds = 60;
    Data data;
    KeyCode[] keyCodes = new KeyCode[] {
        KeyCode.UpArrow,
        KeyCode.UpArrow,
        KeyCode.DownArrow,
        KeyCode.DownArrow,
        KeyCode.LeftArrow,
        KeyCode.RightArrow,
        KeyCode.LeftArrow,
        KeyCode.RightArrow,
        KeyCode.B,
        KeyCode.A
    };
    int keyCodeIndex = 0;
    Constants constants;
    GameObject konamiBossRef;
    bool timerIsRunning = false;

    public delegate void OnKonami();
    public static event OnKonami onKonamiEnabled;

    private void Start()
    {
        constants = Constants.instance;
        data = Data.Instance;
        ShipController.onPlayerDie += StopBossTimer;
        PlayerStats.onGatherAllPowerups += StartBossTimer;
    }

    void OnDestroy()
    {
        ShipController.onPlayerDie -= StopBossTimer;
        PlayerStats.onGatherAllPowerups -= StartBossTimer;
    }

    void Update()
    {
        if (keyCodeIndex >= keyCodes.Length && !data.konamiEnabled)
        {
            // Konami surprise!!
            onKonamiEnabled?.Invoke();
            data.konamiEnabled = true;
            keyCodeIndex = 0;
        }
        else
        {
            if (keyCodeIndex < keyCodes.Length && Input.GetKeyDown(keyCodes[keyCodeIndex]))
            {
                keyCodeIndex++;
            }
            else if (Input.anyKeyDown)
            {
                keyCodeIndex = 0;
            }
        }
    }

    void StopBossTimer()
    {
        StopCoroutine("BossTimer");
        timerIsRunning = false;
    }

    public void StartBossTimer()
    {
        if (!timerIsRunning && (konamiBossRef == null || (konamiBossRef && !konamiBossRef.activeSelf)))
        {
            StartCoroutine("BossTimer");
        }
    }

    IEnumerator BossTimer()
    {
        timerIsRunning = true;
        yield return new WaitForSeconds(konamiBossWaitSeconds);
        timerIsRunning = false;
        Vector2 cameraPosition = Camera.main.transform.position;
        Vector2 position = new Vector2(cameraPosition.x + constants.wrapDst, 0);
        if (!konamiBossRef)
        {
            konamiBossRef = Instantiate(
                konamiBoss,
                position,
                Quaternion.identity
            );
        }
        else
        {
            konamiBossRef.transform.position = position;
            konamiBossRef.GetComponent<KonamiBoss>().StartChase();
        }
    }
}
