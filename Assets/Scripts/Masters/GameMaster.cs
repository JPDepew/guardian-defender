using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    public static GameMaster instance;

    public GameObject ship;
    public GameObject human;
    public AudioMixer audioMixer;

    // Enemies
    public ParticleSystem alienSpawn;
    public GameObject alien;
    public GameObject flyingSaucer;
    public GameObject watchAlien;
    public GameObject swarmEnemy;

    Utilities utilities;
    Data data;

    public int initialNumberOfHumans;
    public int initialNumberOfAliens;
    public float instantiateNewWaveDelay = 2f;

    public Text scoreText;
    public Text livesText;
    public Text bonusText;
    public Text waveText;
    public Text instructionsText;
    public Text respawnCountdownText;
    public Text popupScoreText;
    public GameObject pauseCanvas;
    public GameObject canvas;

    private Constants constants;
    private Camera mainCamera;
    private PlayerStats playerStats;
    private GameObject shipReference;
    private Animator bonusTextAnimator;
    private AudioSource[] audioSources;
    private UIAnimationsMaster uIAnimationsMaster;
    private GroundLineRenderer frontGroundLineRenderer;

    public int waveCount { get; private set; }
    private int bonus;
    private int alienDestroyedCountTracker;
    private int dstAliensCanSpawnFromPlayer = 3;
    private float verticalHalfSize = 0;
    private bool currentWatchAlien;
    private int waveEnemyCount = 0;

    private Vector3 playerPosition;
    private Quaternion rotation;

    float wrapDst = 100;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        SaveLoad.Load();
        // setting instance refs
        playerStats = PlayerStats.instance;
        utilities = Utilities.instance;
        data = Data.Instance;
        constants = Constants.instance;
        wrapDst = constants.wrapDst;
        mainCamera = Camera.main;
        uIAnimationsMaster = GetComponent<UIAnimationsMaster>();
        frontGroundLineRenderer = GameObject.FindGameObjectWithTag("Ground Line Renderer").GetComponent<GroundLineRenderer>();

        // Event listeners
        Enemy.onEnemyDestroyed += OnAlienDestroyed;
        Watch.onWatchDestroyed += OnWatchDestroyed;
        PlayerPowerups.onTimeFreeze += ToggleMusic;
        Konami.onKonamiEnabled += OnKonamiEnabled;

        verticalHalfSize = mainCamera.orthographicSize;
        bonusTextAnimator = bonusText.GetComponent<Animator>();
        audioSources = GetComponents<AudioSource>();
        playerPosition = new Vector3(0, 0, 0);
        utilities.gameState = Utilities.GameState.STOPPED;

        StartGame();
        StartCoroutine(InstructionsTextFadeOut());
    }

    private void Update()
    {
        if (utilities.gameState == Utilities.GameState.STOPPED)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                EndGame();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        HandleUI();
    }

    private void TogglePause()
    {
        if (utilities.gameState == Utilities.GameState.STOPPED)
        {
            utilities.gameState = Utilities.GameState.RUNNING;
            Time.timeScale = 1;
            pauseCanvas.SetActive(false);
        }
        else
        {
            utilities.gameState = Utilities.GameState.STOPPED;
            Time.timeScale = 0;
            pauseCanvas.SetActive(true);
        }
    }

    IEnumerator InstructionsTextFadeOut()
    {
        yield return new WaitForSeconds(3f);
        while (instructionsText?.color.a > 0.05f)
        {
            instructionsText.color = new Color(instructionsText.color.r, instructionsText.color.g, instructionsText.color.b, instructionsText.color.a - 0.05f);
            yield return null;
        }
        instructionsText.color = new Color(0, 0, 0, 0);
    }

    private void StartGame()
    {
        Data.Instance.ResetAll();
        utilities.gameState = Utilities.GameState.RUNNING;
        alienDestroyedCountTracker = 0;
        shipReference = Instantiate(ship);

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Application.targetFrameRate = -1;
        }
        else
        {
            Application.targetFrameRate = 60;
        }

        StartCoroutine(InstantiateNewWave());
    }

    private void HandleUI()
    {
        livesText.text = "Lives: " + playerStats.GetLives().ToString();
        scoreText.text = playerStats.GetScore().ToString();
    }

    IEnumerator InstantiateNewWave()
    {
        waveEnemyCount = 0;
        if (waveCount > 0)
        {
            bonusText.gameObject.SetActive(true);
            bonusTextAnimator.Play("Wave End");
            waveText.text = $"Wave {waveCount} Complete";
            bonusText.text = $"Surviving humans: {bonus / constants.humanBonus} x {constants.humanBonus} = {bonus} bonus";
        }
        else
        {
            bonusText.text = "";
        }

        waveCount++;

        yield return new WaitForSeconds(bonusTextAnimator.GetCurrentAnimatorStateInfo(0).length);

        if (shipReference)
        {
            shipReference.GetComponent<ShipController>().ClearAllHumans();
        }

        bonusText.text = "";
        bonusText.GetComponent<Animator>().StopPlayback();
        bonusText.gameObject.SetActive(false);
        playerStats.IncreaseScoreBy(bonus);
        bonus = 0;
        StartCoroutine(InstantiateEnemies());
        StartCoroutine(InstantiateHumans());
    }

    private IEnumerator InstantiateHumans()
    {
        float camPosX = mainCamera.transform.position.x;
        for (int i = 0; i < initialNumberOfHumans; i++)
        {
            float xPos = Random.Range(camPosX - wrapDst, camPosX + wrapDst);
            float yPos = frontGroundLineRenderer.GetWorldYPoint(xPos) - constants.negativeHumanOffset;
            Vector2 humanPositon = new Vector2(xPos, yPos);

            Instantiate(human, humanPositon, transform.rotation);

            yield return null;
        }
    }

    private IEnumerator InstantiateEnemies()
    {
        StartCoroutine(SpawnAliens());
        StartCoroutine(SpawnSwarmContainers());
        if (waveCount % 5 == 0 && waveCount != 0 && !currentWatchAlien)
        {
            while (shipReference == null)
            {
                yield return null;
            }
            currentWatchAlien = true;
            Instantiate(watchAlien, new Vector2(shipReference.transform.position.x + 4, mainCamera.orthographicSize + 3), watchAlien.transform.rotation);
            yield return new WaitForSeconds(6);
            if (!data.konamiEnabled)
            {
                audioSources[0].Stop();
                audioSources[1].Play();
            }
        }
    }

    void OnKonamiEnabled()
    {
        PlayMusic(audioSources[2]);
    }

    void PlayMusic(AudioSource audioSourceToPlay)
    {
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource == audioSourceToPlay)
            {
                audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }
        }
    }

    private IEnumerator SpawnSwarmContainers()
    {
        if (waveCount % 1 == 0)
        {
            for (int i = 0; i <= Mathf.Ceil(waveCount / 2); i++)
            {
                waveEnemyCount += 7;
                Vector2 position = GetRandomPosition();
                Instantiate(swarmEnemy, position, transform.rotation);
                yield return null;
            }
        }
    }

    /// <summary>
    /// Spawn an increasing number of Aliens every wave
    /// </summary>
    IEnumerator SpawnAliens()
    {
        int aliensToCreate = data.konamiEnabled ? initialNumberOfAliens + waveCount * 2 : initialNumberOfAliens;
        for (int i = 0; i < aliensToCreate; i++)
        {
            waveEnemyCount++;
            StartCoroutine(SpawnAlien());
            yield return null;
        }
    }

    /// <summary>
    /// Instantiate alien particle system and then instantiate alien
    /// </summary>
    IEnumerator SpawnAlien()
    {
        Vector2 alienPosition = GetRandomPosition();
        Transform tempTransform = Instantiate(alienSpawn, alienPosition, transform.rotation).transform;
        yield return new WaitForSeconds(alienSpawn.main.duration);
        Instantiate(alien, tempTransform.position, transform.rotation);
    }

    private void OnWatchDestroyed()
    {
        currentWatchAlien = false;
        if (!data.konamiEnabled)
        {
            audioSources[0].Play();
            audioSources[1].Stop();
        }
    }

    /// <summary>
    /// Gets a position within the vertical camera bounds and the correct direction to the left and right
    /// And not too close to the player
    /// </summary>
    /// <returns>New position Vector2</returns>
    private Vector2 GetRandomPosition()
    {
        Vector2 newPosition;
        do
        {
            float camPosX = mainCamera.transform.position.x;
            float xRange = Random.Range(camPosX - wrapDst, camPosX + wrapDst);
            int yRange = (int)Random.Range(-verticalHalfSize + constants.bottomOffset, verticalHalfSize - constants.topOffset);
            newPosition = new Vector2(xRange, yRange);
        } while (shipReference != null && (newPosition - (Vector2)shipReference.transform.position).magnitude < dstAliensCanSpawnFromPlayer);

        return newPosition;
    }

    /// <summary>
    /// If all the aliens are destroyed, start a new wave.
    /// If there are only 2 left, and it is an even wave, instantiate a saucer.
    /// </summary>
    private void OnAlienDestroyed(int scoreIncrease)
    {
        playerStats.IncreaseScoreBy(scoreIncrease);
        alienDestroyedCountTracker++;
        InstantiateFlyingSaucer();
        if (alienDestroyedCountTracker >= waveEnemyCount)
        {
            initialNumberOfAliens++;
            alienDestroyedCountTracker = 0;
            DealWithRemainingHumans();
            if (this != null)
            {
                StartCoroutine(InstantiateNewWave());
            }
        }
    }

    /// <summary>
    /// Decide if to instantiate a flying saucer. If in konami mode, instantate it every 3rd destroyed alien.
    /// </summary>
    void InstantiateFlyingSaucer()
    {
        bool shouldInstantiateSaucer = false;
        if (data.konamiEnabled)
        {
            shouldInstantiateSaucer = alienDestroyedCountTracker % 3 == 0;
        }
        else
        {
            shouldInstantiateSaucer = alienDestroyedCountTracker == initialNumberOfAliens - 2;
        }
        if (shouldInstantiateSaucer)
        {
            if (waveCount % 1 == 0 && shipReference != null)
            {
                int rand = Random.Range(0, 2);
                int sign = rand == 1 ? 1 : -1;
                Instantiate(flyingSaucer, new Vector2(shipReference.transform.position.x + sign * (wrapDst - 2), 0), transform.rotation);
            }
        }
    }

    /// <summary>
    /// Remove the remaining humans at end of wave. If there are any that are falling or being carried by
    /// the player, do not remove them.
    /// </summary>
    private void DealWithRemainingHumans()
    {
        Human[] humans = FindObjectsOfType<Human>();
        for (int i = 0; i < humans.Length; i++)
        {
            if (humans[i].GetState() != Human.State.GROUNDED) continue;
            Destroy(humans[i].gameObject);
            if (!(humans[i].curState == Human.State.DEAD))
            {
                bonus += constants.humanBonus;
            }
        }
    }

    private void EndGame()
    {
        Time.timeScale = 1;
        utilities.gameState = Utilities.GameState.STOPPED;
        SceneManager.LoadScene(1);
    }

    public void RespawnPlayer()
    {
        PlayAllMusic();
        playerPosition = shipReference.transform.position;
        rotation = shipReference.transform.rotation;
        playerStats.ResetAllPowerups();
        if (playerStats.GetLives() > 0)
        {
            StartCoroutine(RespawnPlayerTimer());
        }
        else
        {
            StartCoroutine(NewScene());
        }
    }

    public void InstantiateScorePopup(int scoreIncrease, Vector3 position)
    {
        uIAnimationsMaster?.InstanatiateScorePopup(scoreIncrease, position);
    }

    IEnumerator RespawnPlayerTimer()
    {
        GameObject parent = respawnCountdownText.transform.parent.gameObject;
        parent.SetActive(true);
        respawnCountdownText.text = "3";
        while (System.Convert.ToInt16(respawnCountdownText.text) > 0)
        {
            yield return new WaitForSecondsRealtime(1);
            respawnCountdownText.text = (System.Convert.ToInt16(respawnCountdownText.text) - 1).ToString();
        }
        parent.SetActive(false);
        shipReference = Instantiate(ship, new Vector2(playerPosition.x, 0), rotation);
    }

    // This is necessary
    // After reloading the scene, objects are still subscribed to events.
    private void OnDestroy()
    {
        Enemy.onEnemyDestroyed -= OnAlienDestroyed;
        Watch.onWatchDestroyed -= OnWatchDestroyed;
        PlayerPowerups.onTimeFreeze -= ToggleMusic;
        Konami.onKonamiEnabled -= OnKonamiEnabled;
    }

    IEnumerator NewScene()
    {
        yield return new WaitForSeconds(4);
        Data.Instance.score = playerStats.GetScore();
        SceneManager.LoadScene(3);
    }

    /// <summary>
    /// Toggles whether the current background is playing or not
    /// </summary>
    void ToggleMusic()
    {
        if (currentWatchAlien)
        {
            if (audioSources[1].isPlaying)
            {
                audioSources[1].Pause();
            }
            else
            {
                audioSources[1].UnPause();
            }
        }
        else
        {
            if (audioSources[0].isPlaying)
            {
                audioSources[0].Pause();
            }
            else
            {
                audioSources[0].UnPause();
            }
        }
    }

    /// <summary>
    /// Makes sure that music is playing. If the player dies when time is frozen,
    /// The music must be started again.
    /// </summary>
    void PlayAllMusic()
    {
        if (currentWatchAlien)
        {
            audioSources[1].UnPause();
        }
        else
        {
            audioSources[0].UnPause();
        }
    }

    public void IncreaseWaveEnemyCount(int increaseAmt)
    {
        waveEnemyCount += increaseAmt;
    }
}
