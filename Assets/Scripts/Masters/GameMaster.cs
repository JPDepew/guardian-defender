using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    public static GameMaster instance;

    public GameObject alien;
    public GameObject flyingSaucer;
    public GameObject ship;
    public GameObject human;
    public GameObject watchAlien;
    public ParticleSystem alienSpawn;
    public AudioMixer audioMixer;

    public GameObject side1;
    public GameObject side2;

    Utilities utilities;

    public float initialNumberOfHumans;
    public float initialNumberOfAliens;
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
    private bool respawningCharacter;
    private Animator bonusTextAnimator;
    private AudioSource[] audioSources;
    private UIAnimationsMaster uIAnimationsMaster;

    private float waveCount = 0f;
    private int bonus;
    private int score;
    private int scoreTracker;
    private int alienDestroyedCountTracker;
    private int dstAsteroidsCanSpawnFromPlayer = 3;
    private float verticalHalfSize = 0;
    private bool currentWatchAlien;

    private Vector3 playerPosition;
    private Quaternion rotation;

    float wrapDst = 100;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // setting instance refs
        playerStats = PlayerStats.instance;
        utilities = Utilities.instance;
        constants = Constants.instance;
        wrapDst = constants.wrapDst;
        mainCamera = Camera.main;
        uIAnimationsMaster = GetComponent<UIAnimationsMaster>();

        // Event listeners
        Alien.onAlienDestroyed += OnAlienDestroyed;
        MutatedAlien.onMutatedAlienDestroyed += OnAlienDestroyed;
        Watch.onWatchDestroyed += OnWatchDestroyed;

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
        while (instructionsText.color.a > 0.05f)
        {
            instructionsText.color = new Color(instructionsText.color.r, instructionsText.color.g, instructionsText.color.b, instructionsText.color.a - 0.05f);
            yield return null;
        }
        instructionsText.color = new Color(0, 0, 0, 0);
    }

    private void StartGame()
    {
        Data.Instance.score = 0;
        utilities.gameState = Utilities.GameState.RUNNING;
        alienDestroyedCountTracker = 0;
        shipReference = Instantiate(ship);
        Application.targetFrameRate = 60;

        StartCoroutine(InstantiateNewWave());
    }

    private void HandleUI()
    {
        livesText.text = "Lives: " + playerStats.GetLives().ToString();
        scoreText.text = playerStats.GetScore().ToString();
    }

    IEnumerator InstantiateNewWave()
    {
        if (waveCount > 0)
        {
            bonusText.gameObject.SetActive(true);
            bonusTextAnimator.Play("Wave End");
            waveText.text = $"Wave {waveCount} Complete";
            bonusText.text = $"Surviving humans: {bonus / constants.humanBonus} x {constants.humanBonus} = {bonus} bonus\nTotal Points: {playerStats.GetScore() + bonus}";
        }
        else
        {
            bonusText.text = "";
        }

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
        StartCoroutine(InstantiateAliens());
        StartCoroutine(InstantiateHumans());
        waveCount++;
    }

    private IEnumerator InstantiateHumans()
    {
        float camPosX = mainCamera.transform.position.x;
        for (int i = 0; i < initialNumberOfHumans; i++)
        {
            float xRange = Random.Range(camPosX - wrapDst, camPosX + wrapDst);
            float yRange = -verticalHalfSize + constants.bottomOffset;

            Vector2 humanPositon = new Vector2(xRange, yRange);

            Instantiate(human, humanPositon, transform.rotation);

            yield return null;
        }
    }

    private IEnumerator InstantiateAliens()
    {
        float camPosX = mainCamera.transform.position.x;
        for (int i = 0; i < initialNumberOfAliens; i++)
        {
            float xRange = Random.Range(camPosX - wrapDst, camPosX + wrapDst);
            int yRange = (int)Random.Range(-verticalHalfSize + constants.bottomOffset, verticalHalfSize - constants.topOffset);

            Vector2 alienPositon = new Vector2(xRange, yRange);
            while (shipReference == null)
            {
                yield return new WaitForSeconds(0.2f);
            }
            // Making sure aliens don't spawn too close to the player
            if ((alienPositon - (Vector2)shipReference.transform.position).magnitude < dstAsteroidsCanSpawnFromPlayer)
            {
                i--; // This is probably really sketchy, I know... But it works really well...
            }
            else
            {
                StartCoroutine("SpawnAlien", alienPositon);
            }
            yield return null;
        }
        if (waveCount % 6 == 0 && !currentWatchAlien)
        {
            currentWatchAlien = true;
            audioSources[0].Stop();
            Instantiate(watchAlien, new Vector2(shipReference.transform.position.x + 4, mainCamera.orthographicSize + 3), watchAlien.transform.rotation);
            yield return new WaitForSeconds(6);
            audioSources[1].Play();
        }
    }

    IEnumerator SpawnAlien(Vector2 alienPosition)
    {
        Transform tempTransform = Instantiate(alienSpawn, alienPosition, transform.rotation).transform;
        yield return new WaitForSeconds(alienSpawn.main.duration);
        Instantiate(alien, tempTransform.position, transform.rotation);
    }

    private void OnWatchDestroyed()
    {
        currentWatchAlien = false;
        audioSources[0].Play();
        audioSources[1].Stop();
    }

    private void OnAlienDestroyed()
    {
        alienDestroyedCountTracker++;
        if (alienDestroyedCountTracker == initialNumberOfAliens - 2)
        {
            if (waveCount % 3 == 0 && shipReference != null)
            {
                Instantiate(flyingSaucer, new Vector2(shipReference.transform.position.x + 12, mainCamera.orthographicSize - 2), transform.rotation);
            }
        }
        if (alienDestroyedCountTracker >= initialNumberOfAliens)
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

    private void DealWithRemainingHumans()
    {
        Human[] humans = FindObjectsOfType<Human>();
        for (int i = 0; i < humans.Length; i++)
        {
            Destroy(humans[i].gameObject);
            if (!(humans[i].curState == Human.State.DEAD))
            {
                bonus += constants.humanBonus;
            }
        }
    }

    private void EndGame()
    {
        SceneManager.LoadScene(1);
        Time.timeScale = 1;
        utilities.gameState = Utilities.GameState.STOPPED;
    }

    public void RespawnPlayer()
    {
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
        uIAnimationsMaster.InstanatiateScorePopup(scoreIncrease, position);
    }

    IEnumerator RespawnPlayerTimer()
    {
        GameObject parent = respawnCountdownText.transform.parent.gameObject;
        parent.SetActive(true);
        respawnCountdownText.text = "3";
        yield return new WaitForSeconds(1);
        respawnCountdownText.text = "2";
        yield return new WaitForSeconds(1);
        respawnCountdownText.text = "1";
        yield return new WaitForSeconds(1);
        parent.SetActive(false);
        shipReference = Instantiate(ship, new Vector2(playerPosition.x, 0), rotation);
    }

    // This is necessary
    // After reloading the scene, objects are still subscribed to events.
    private void OnDestroy()
    {
        Alien.onAlienDestroyed -= OnAlienDestroyed;
        MutatedAlien.onMutatedAlienDestroyed -= OnAlienDestroyed;
        Watch.onWatchDestroyed -= OnWatchDestroyed;
    }

    IEnumerator NewScene()
    {
        yield return new WaitForSeconds(4);
        Data.Instance.score = playerStats.GetScore();
        SceneManager.LoadScene(3);
    }
}
