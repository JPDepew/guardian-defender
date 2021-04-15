using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    public GameObject gunPosition;
    public GameObject bullet;
    public GameObject bulletDisinfect;
    public GameObject bigLaser;
    public GameObject shield;
    public GameObject explosion;
    public GameObject healthIndicator;
    public GameObject leftShip;
    public GameObject rammer;

    public LayerMask layerMask;

    public ParticleSystem fuelParticleSystem;
    public Transform particleSystemPosLeft;
    public Transform particleSystemPosRight;

    public ParticleSystem boostParticleSystem;
    public Transform boostParticleSystemPosLeft;
    public Transform boostParticleSystemPosRight;

    public Transform healthIndicatorParent;
    public Transform healthIndicatorPos;
    public float healthIndicatorOffset = 0.5f;
    private float currentHealthIndicatorOffset = 0;

    public float horizontalAcceleration = 0.1f;
    public float verticalAcceleration = 0.6f;
    public float backwardsAcceleration = 0.1f;
    public float maxHorizontalSpeed = 2;
    public float maxVerticalSpeed = 2;
    public float maxBackwardsSpeed = 2;
    public float boostValue = 1.3f;

    public float verticalDecelerationLinearInterpolationTime = 0.12f;
    public float horizontalDecelerationLinearInterpolationTime = 0.2f;

    public bool demo = false;
    public bool frozen = false;

    private List<Human> shipHumans;
    private Stack<GameObject> healthIndicators;
    private AudioSource[] audioSources;
    private Vector2 direction;
    private SpriteRenderer spriteRenderer;
    private PlayerStats playerStats;
    private Utilities utilities;
    private Constants constants;
    private bool canShoot = true;
    private float boostMultiplier = 1f;

    private AudioSource[] boostAudioSources;

    private float invulnerabilityTime = 1f;
    private float invulnerabilityTargetTime;
    private bool shouldBeInvulnerable = true;

    float verticalHalfSize;
    bool destroyed = false;

    GameMaster gameMaster;

    public delegate void OnPlayerDie();
    public static event OnPlayerDie onPlayerDie;

    private void Start()
    {
        playerStats = PlayerStats.instance;
        utilities = Utilities.instance;
        constants = Constants.instance;
        gameMaster = GameMaster.instance;

        shipHumans = new List<Human>();
        healthIndicators = new Stack<GameObject>();
        audioSources = GetComponents<AudioSource>();
        verticalHalfSize = Camera.main.orthographicSize;
        invulnerabilityTargetTime = Time.time + invulnerabilityTime;
        boostAudioSources = boostParticleSystem.GetComponents<AudioSource>();
        InitializeHealthIndicators();

        PowerupObj.onGetPowerup += OnPowerup;

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.5f);

        StartCoroutine(HandleCollisions());
    }

    void Update()
    {
        if (utilities.gameState == Utilities.GameState.STOPPED) return;

        GetInput();

        HandleInvulnerability();
        transform.position = transform.position + (Vector3)direction * Time.unscaledDeltaTime;
    }

    void OnPowerup(Powerup powerupName)
    {
        if (powerupName == Powerup.Shield)
        {
            audioSources[3].Play();
            shield.SetActive(true);
        }
        if (powerupName == Powerup.Boost)
        {
            boostParticleSystem.gameObject.SetActive(true);
        }
        if (powerupName == Powerup.Rammer)
        {
            rammer.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        PowerupObj.onGetPowerup -= OnPowerup;
    }

    public void InitializeHealthIndicators()
    {
        currentHealthIndicatorOffset = 0;
        foreach (GameObject gameObject in healthIndicators)
        {
            Destroy(gameObject);
        }
        healthIndicators.Clear();
        
        int playerLives = playerStats.GetLives();
        int maxHealthIndicators = 3;
        int healthIndicatorLimit = playerLives > maxHealthIndicators ? maxHealthIndicators : playerLives;
        for (int i = 0; i < healthIndicatorLimit; i++)
        {
            GameObject tempHealthIndicator = Instantiate(healthIndicator, healthIndicatorPos.position + Vector3.right * currentHealthIndicatorOffset, transform.rotation);
            tempHealthIndicator.transform.parent = healthIndicatorParent;
            healthIndicators.Push(tempHealthIndicator);
            if (leftShip.gameObject.activeSelf)
            {
                tempHealthIndicator.transform.localScale = new Vector2(-tempHealthIndicator.transform.localScale.x, tempHealthIndicator.transform.localScale.y);
                currentHealthIndicatorOffset -= healthIndicatorOffset;
            }
            else
            {
                currentHealthIndicatorOffset += healthIndicatorOffset;
            }
        }
    }

    private void GetInput()
    {
        HandleHorizontalInput();
        HandleReverseInput();
        HandleVerticalInput();
        ManageVerticalBounds();

        if (frozen)
        {
            direction = Vector2.zero;
        }

        bool autoShot = playerStats.IsPowerupActive(Powerup.AutoShot);
        bool shootInput = autoShot ? Input.GetKey(KeyCode.Z) : Input.GetKeyDown(KeyCode.Z);
        bool disinfectInput = autoShot ? Input.GetKey(KeyCode.X) : Input.GetKeyDown(KeyCode.X);

        // Shooting
        if (shootInput && canShoot)
        {
            if (!playerStats.IsPowerupActive(Powerup.Laser))
            {
                ShootBullet(bullet);
            }
            else
            {
                StartCoroutine(ShootBigLaser());
                canShoot = false;
                StartCoroutine(WaitBetweenShooting(false));
            }
        }
        if (disinfectInput && canShoot)
        {
            ShootBullet(bulletDisinfect);
        }
    }

    private IEnumerator HandleCollisions()
    {
        while (true)
        {
            // todo: different size for different things?
            Collider2D col = Physics2D.OverlapBox(transform.position, new Vector3(1.6f, 0.2f), 0, layerMask);
            if (col && col.tag == "Human")
            {
                Human human = col.transform.GetComponent<Human>();
                if (human.curState == Human.State.FALLING || human.curState == Human.State.DEMO)
                {
                    float audioPitchIncrease = 0.05f;

                    audioSources[4].pitch = 1 + shipHumans.Count * audioPitchIncrease;
                    shipHumans.Add(human);
                    audioSources[4].Play();
                    human.SetToRescued(transform, shipHumans.Count);
                    if (gameMaster)
                    {
                        gameMaster.InstantiateScorePopup(constants.catchHumanBonus * shipHumans.Count, transform.position);
                    }
                }
            }
            else if (!shouldBeInvulnerable && col?.tag == "Alien")
            {
                col.GetComponent<Enemy>().DamageSelf(12, transform.position);
                DestroySelf();
            }
            yield return new WaitForSecondsRealtime(.02f);
        }
    }

    void ShootBullet(GameObject bullet)
    {
        GameObject tempBullet = Instantiate(bullet, gunPosition.transform.position, transform.rotation);

        tempBullet.transform.localScale = leftShip.activeSelf == true ?
            new Vector2(-tempBullet.transform.localScale.x, tempBullet.transform.localScale.y) :
            new Vector2(tempBullet.transform.localScale.x, tempBullet.transform.localScale.y);

        Bullet bulletScript = tempBullet.GetComponent<Bullet>();
        // Add the ships speed to the bullet speed
        bulletScript.speed += Mathf.Abs(direction.x);
        // multiply by ship's boostMultiplier
        bulletScript.speed *= boostMultiplier;

        canShoot = false;
        StartCoroutine(WaitBetweenShooting(true));
    }

    void ManageVerticalBounds()
    {
        // Checking to make sure it is not off the screen
        if (transform.position.y <= -verticalHalfSize + 1 && direction.y < 0)
        {
            direction = new Vector2(direction.x, 0);
        }
        if (transform.position.y >= verticalHalfSize - constants.topOffset && direction.y > 0)
        {
            direction = new Vector2(direction.x, 0);
        }
    }

    public float GetPlayerSpeed()
    {
        return direction.x;
    }

    void HandleHorizontalInput()
    {
        // Side to side movement
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            HandleFlippingLeft();
            if (direction.x > -maxHorizontalSpeed * boostMultiplier)
            {
                direction += horizontalAcceleration * Vector2.left * boostMultiplier;
            }
            else
            {
                direction = Vector2.Lerp(direction, new Vector2(-maxHorizontalSpeed, direction.y), horizontalDecelerationLinearInterpolationTime);
            }
            HandleParticleSystemsMoveLeft();
            PlayEngineAudio();
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            HandleFlippingRight();
            if (direction.x < maxHorizontalSpeed * boostMultiplier)
            {
                direction += horizontalAcceleration * Vector2.right * boostMultiplier;
            }
            else
            {
                direction = Vector2.Lerp(direction, new Vector2(maxHorizontalSpeed, direction.y), horizontalDecelerationLinearInterpolationTime);
            }
            HandleParticleSystemsMoveRight();
            PlayEngineAudio();
        }
        // Horizontal Deceleration
        if (!Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.Space))
        {
            boostMultiplier = 1;
            if (Mathf.Abs(direction.x) > 0.01f)
            {
                direction = Vector2.Lerp(direction, new Vector2(0, direction.y), horizontalDecelerationLinearInterpolationTime);
                HandleEngineParticleSystemStop();
            }
            else
            {
                direction = new Vector2(0, direction.y);
            }
        }
    }

    void HandleFlippingRight()
    {
        healthIndicatorParent.localScale = new Vector2(Mathf.Abs(healthIndicatorParent.localScale.x), healthIndicatorParent.localScale.y);
        rammer.transform.localScale = new Vector2(Mathf.Abs(rammer.transform.localScale.x), rammer.transform.localScale.y);
        leftShip.SetActive(false);
        spriteRenderer.enabled = true;
    }

    void HandleFlippingLeft()
    {
        healthIndicatorParent.localScale = new Vector2(-Mathf.Abs(healthIndicatorParent.localScale.x), healthIndicatorParent.localScale.y);
        rammer.transform.localScale = new Vector2(-Mathf.Abs(rammer.transform.localScale.x), rammer.transform.localScale.y);
        leftShip.SetActive(true);
        spriteRenderer.enabled = false;
    }

    void HandleParticleSystemsMoveLeft()
    {
        fuelParticleSystem.transform.rotation = Quaternion.Euler(new Vector3(180, -90, 0));
        fuelParticleSystem.transform.position = particleSystemPosRight.position;
        boostParticleSystem.transform.rotation = Quaternion.Euler(new Vector3(180, -90, 0));
        boostParticleSystem.transform.position = boostParticleSystemPosLeft.position;
        HandleEngineParticleSystems();
    }

    void HandleParticleSystemsMoveRight()
    {
        fuelParticleSystem.transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
        fuelParticleSystem.transform.position = particleSystemPosLeft.position;
        boostParticleSystem.transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
        boostParticleSystem.transform.position = boostParticleSystemPosRight.position;
        HandleEngineParticleSystems();
    }

    void HandleEngineParticleSystems()
    {
        if (frozen) return;
        if (!fuelParticleSystem.isEmitting)
        {
            fuelParticleSystem.Play();
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (boostParticleSystem.gameObject.activeSelf && !boostParticleSystem.isEmitting)
            {
                boostAudioSources[0].Play();
                boostAudioSources[1].Play();
                boostMultiplier = boostValue;
                boostParticleSystem.Play();
            }
        }
        else
        {
            boostMultiplier = 1;
            boostAudioSources[1].Stop();
            boostAudioSources[0].Stop();
            boostParticleSystem.Stop();
        }
    }

    void HandleEngineParticleSystemStop()
    {
        if (fuelParticleSystem.isPlaying)
        {
            fuelParticleSystem.Stop();
        }
        if (boostParticleSystem.isPlaying)
        {
            boostAudioSources[1].Stop();
            boostAudioSources[0].Stop();
            boostParticleSystem.Stop();
        }
        StopEngineAudio();
    }

    void PlayEngineAudio()
    {
        if (!audioSources[1].isPlaying && !frozen)
        {
            audioSources[1].Play();
        }
    }

    public void StopEngineAudio()
    {
        if (audioSources[1].isPlaying)
        {
            audioSources[1].Stop();
        }
    }

    void HandleReverseInput()
    {
        // Reverse movement
        if (Input.GetKey(KeyCode.Space))
        {
            if (leftShip.activeSelf)
            {
                if (direction.x < maxBackwardsSpeed)
                {
                    direction += backwardsAcceleration * Vector2.right;
                }
                fuelParticleSystem.transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
                fuelParticleSystem.transform.position = particleSystemPosRight.position;
                if (!fuelParticleSystem.isEmitting)
                {
                    fuelParticleSystem.Play();
                }
                if (!audioSources[1].isPlaying)
                {
                    audioSources[1].Play();
                }
            }
            else
            {
                if (direction.x > -maxBackwardsSpeed)
                {
                    direction += backwardsAcceleration * Vector2.left;
                }
                fuelParticleSystem.transform.rotation = Quaternion.Euler(new Vector3(180, -90, 0));
                fuelParticleSystem.transform.position = particleSystemPosLeft.position;
                if (!fuelParticleSystem.isEmitting)
                {
                    fuelParticleSystem.Play();
                }
                if (!audioSources[1].isPlaying)
                {
                    audioSources[1].Play();
                }
            }
        }
    }

    void HandleVerticalInput()
    {
        // Up and down movement
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            if (direction.y < maxVerticalSpeed)
                direction += verticalAcceleration * Vector2.up;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            if (direction.y > -maxVerticalSpeed)
                direction += verticalAcceleration * Vector2.down;
        }
        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.DownArrow))
        {
            if (Mathf.Abs(direction.y) > 0.01f)
            {
                direction = Vector2.Lerp(direction, new Vector2(direction.x, 0), verticalDecelerationLinearInterpolationTime);
            }
            else
            {
                direction = new Vector2(direction.x, 0);
            }
        }
    }

    IEnumerator ShootBigLaser()
    {
        if (!playerStats.IsPowerupActive(Powerup.AutoShot))
        {
            audioSources[2].Play();
        }
        yield return new WaitForSecondsRealtime(0.1f);
        GameObject tempBullet = Instantiate(bigLaser, gunPosition.transform.position, transform.rotation);
        tempBullet.transform.localScale = leftShip.activeSelf == true ?
            new Vector2(-tempBullet.transform.localScale.x, tempBullet.transform.localScale.y) :
            new Vector2(tempBullet.transform.localScale.x, tempBullet.transform.localScale.y);
    }

    IEnumerator WaitBetweenShooting(bool disinfect)
    {
        float waitTime = 0.01f;
        if (!playerStats.IsPowerupActive(Powerup.AutoShot))
        {
            waitTime = playerStats.IsPowerupActive(Powerup.Laser) && !disinfect ? 0.45f : 0.1f;
        }
        yield return new WaitForSecondsRealtime(waitTime);
        canShoot = true;
    }

    private void HandleInvulnerability()
    {
        if (shouldBeInvulnerable)
        {
            if (Time.time > invulnerabilityTargetTime)
            {
                shouldBeInvulnerable = false;
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1);
            }
        }
    }

    public void RemoveHuman(Human human)
    {
        if (gameMaster)
        {
            gameMaster.InstantiateScorePopup(constants.rescueHumanBonus, transform.position);
        }
        audioSources[4].pitch = 1;
        audioSources[4].Play();
        shipHumans.Remove(human);
    }

    public void ClearAllHumans()
    {
        audioSources[4].pitch = 1;
        shipHumans.Clear();
    }

    /// <summary>
    /// Destroys the player, instantiates the explosion particle system, which has the explosion sound on it, and decrements lives.
    /// </summary>
    public void DestroySelf()
    {
        if (!destroyed && !frozen)
        {
            destroyed = true;
            onPlayerDie?.Invoke();
            Instantiate(explosion, transform.position, transform.rotation);
            playerStats.DecrementLives();
            gameMaster.RespawnPlayer();
            Destroy(gameObject);
        }
    }

    public bool HasHumans()
    {
        return shipHumans.Count > 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (demo || shouldBeInvulnerable)
        {
            return;
        }
        //if (collision.tag == "Alien")
        //{
        //    collision.GetComponent<Enemy>().DamageSelf(12, transform.position);
        //    DestroySelf();
        //}
        if (collision.tag == "SwarmTop" || collision.tag == "SwarmBottom")
        {
            DestroySelf();
        }
        if (collision.tag == "AlienBullet")
        {
            Destroy(collision.gameObject);
            DestroySelf();
        }
    }
}
