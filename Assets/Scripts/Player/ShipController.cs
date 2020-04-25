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
    public GameObject bomb;
    public GameObject healthIndicator;
    public GameObject leftShip;

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
    private BoxCollider2D boxCollider;
    private bool shouldBeInvulnerable = true;

    KeyCode bombKeyCode;

    float verticalHalfSize;
    bool destroyed = false;

    GameMaster gameMaster;

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

        bombKeyCode = constants.PowerupObjByEnum(Powerup.Bomb).keyCode;

        PowerupObj.onGetPowerup += OnPowerup;

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.5f);

        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.enabled = false;
    }

    void Update()
    {
        if (utilities.gameState == Utilities.GameState.STOPPED) return;

        GetInput();

        HandleInvulnerability();
        transform.position = transform.position + (Vector3)direction * Time.deltaTime;
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
        for (int i = 0; i < playerStats.GetLives(); i++)
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

        HandleBomb();

        // Shooting
        if (Input.GetKeyDown(KeyCode.Z) && canShoot)
        {
            if (!playerStats.IsPowerupActive(Powerup.Laser))
            {
                ShootBullet(bullet);
            }
            else
            {
                audioSources[2].Play();

                StartCoroutine(ShootBigLaser());
                canShoot = false;
                StartCoroutine(WaitBetweenShooting(false));
            }
        }
        if (Input.GetKeyDown(KeyCode.X) && canShoot)
        {
            ShootBullet(bulletDisinfect);
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
            }
        }
    }

    void ShootBullet(GameObject bullet)
    {
        audioSources[0].Play();
        GameObject tempBullet = Instantiate(bullet, gunPosition.transform.position, transform.rotation);

        tempBullet.transform.localScale = leftShip.activeSelf == true ?
            new Vector2(-tempBullet.transform.localScale.x, tempBullet.transform.localScale.y) :
            new Vector2(tempBullet.transform.localScale.x, tempBullet.transform.localScale.y);

        tempBullet.GetComponent<Bullet>().speed *= boostMultiplier;

        canShoot = false;
        StartCoroutine(WaitBetweenShooting(false));
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

    void HandleHorizontalInput()
    {
        // Side to side movement
        if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)))
        {
            healthIndicatorParent.localScale = new Vector2(-Mathf.Abs(healthIndicatorParent.localScale.x), healthIndicatorParent.localScale.y);
            leftShip.SetActive(true);
            spriteRenderer.enabled = false;
            if (direction.x > -maxHorizontalSpeed * boostMultiplier)
            {
                direction += horizontalAcceleration * Vector2.left * boostMultiplier;
            }
            else
            {
                direction = Vector2.Lerp(direction, new Vector2(-maxHorizontalSpeed, direction.y), horizontalDecelerationLinearInterpolationTime);
            }
            HandleParticleSystemsMoveLeft();
            if (!audioSources[1].isPlaying)
            {
                audioSources[1].Play();
            }
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            healthIndicatorParent.localScale = new Vector2(Mathf.Abs(healthIndicatorParent.localScale.x), healthIndicatorParent.localScale.y);
            leftShip.SetActive(false);
            spriteRenderer.enabled = true;
            if (direction.x < maxHorizontalSpeed * boostMultiplier)
            {
                direction += horizontalAcceleration * Vector2.right * boostMultiplier;
            }
            else
            {
                direction = Vector2.Lerp(direction, new Vector2(maxHorizontalSpeed, direction.y), horizontalDecelerationLinearInterpolationTime);
            }
            HandleParticleSystemsMoveRight();
            if (!audioSources[1].isPlaying)
            {
                audioSources[1].Play();
            }
        }
        // Horizontal Deceleration
        if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.Space))
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
        yield return new WaitForSeconds(0.1f);
        GameObject tempBullet = Instantiate(bigLaser, gunPosition.transform.position, transform.rotation);
        tempBullet.transform.localScale = leftShip.activeSelf == true ?
            new Vector2(-tempBullet.transform.localScale.x, tempBullet.transform.localScale.y) :
            new Vector2(tempBullet.transform.localScale.x, tempBullet.transform.localScale.y);
    }

    IEnumerator WaitBetweenShooting(bool disinfect)
    {
        float waitTime = playerStats.IsPowerupActive(Powerup.Laser) && !disinfect ? 0.45f : 0.1f;
        yield return new WaitForSeconds(waitTime);
        canShoot = true;
    }

    private void HandleInvulnerability()
    {
        if (shouldBeInvulnerable)
        {
            if (Time.time > invulnerabilityTargetTime)
            {
                boxCollider.enabled = true;
                shouldBeInvulnerable = false;
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1);
            }
        }
    }

    public void RemoveHuman(Human human)
    {
        gameMaster.InstantiateScorePopup(constants.rescueHumanBonus, transform.position);
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
        if (!destroyed)
        {
            destroyed = true;
            Instantiate(explosion, transform.position, transform.rotation);
            playerStats.DecrementLives();
            playerStats.ResetAllPowerups();
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Alien")
        {
            collision.GetComponent<Enemy>().DamageSelf(12, transform.position);
            DestroySelf();
            gameMaster.RespawnPlayer();
        }
        if (collision.tag == "AlienBullet")
        {
            Destroy(collision.gameObject);
            DestroySelf();
            gameMaster.RespawnPlayer();
        }
        if (collision.tag == "Human")
        {
            Human human = collision.transform.GetComponent<Human>();
            if (human.curState == Human.State.FALLING)
            {
                float audioPitchIncrease = 0.05f;

                audioSources[4].pitch = 1 + shipHumans.Count * audioPitchIncrease;
                shipHumans.Add(human);
                audioSources[4].Play();
                human.SetToRescued(transform, shipHumans.Count);
                gameMaster.InstantiateScorePopup(constants.catchHumanBonus, transform.position);
            }
        }
    }
}
