using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SoundPlayer))]
public class Enemy : Hittable
{
    public TextMesh scoreText;
    public GameObject explosion;
    public float maxHealth;
    public float bounceBackAmount = 0.4f;
    public float rotateAmount = 2f;
    public float rotateTime = 0.2f;
    public float health;
    public int destroyPoints;
    protected Vector2 direction;
    protected Vector2 newDirection;
    protected AudioSource[] audioSources;
    protected SpriteRenderer spriteRenderer;
    protected CircleCollider2D circleCollider;

    protected ShipController player;
    protected SoundPlayer soundPlayer;
    protected bool findingPlayer = false;

    public delegate void OnDestroyed(int scoreIncreaseBy);
    public static event OnDestroyed onEnemyDestroyed;

    bool isDestroyed = false;

    protected override void Start()
    {
        health = maxHealth;
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        audioSources = GetComponents<AudioSource>();
        soundPlayer = GetComponent<SoundPlayer>();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override bool DamageSelf(float damage, Vector2 hitPosition, Vector2? bulletDirection = null)
    {
        Vector2 directionToEnemy = ((Vector2)transform.position - hitPosition).normalized;
        health -= damage;
        if (health <= 0)
        {
            if (!isDestroyed)
            {
                isDestroyed = true;
                DestroySelf();
            }
        }
        else
        {
            base.DamageSelf(damage, hitPosition);
            direction += Vector2.right * directionToEnemy.x * bounceBackAmount;

            float directionToHitY = directionToEnemy.x > 0 ? Mathf.Sign(directionToEnemy.y) : -Mathf.Sign(directionToEnemy.y);

            StartRotation(directionToHitY);
        }
        return true;
    }

    protected virtual IEnumerator FindPlayer()
    {
        findingPlayer = true;
        while (player == null)
        {
            player = FindObjectOfType<ShipController>();
            yield return new WaitForSeconds(0.3f);
        }
        findingPlayer = false;
    }

    public virtual bool DisinfectEnemy(Vector2 hitPoint)
    {
        Vector2 directionToEnemy = ((Vector2)transform.position - hitPoint).normalized;
        float directionToHitY = directionToEnemy.x > 0 ? Mathf.Sign(directionToEnemy.y) : -Mathf.Sign(directionToEnemy.y);

        return true;
    }

    protected virtual void DestroySelf()
    {
        DestroySelf(true);
    }

    protected virtual void DestroySelf(bool invokeEvent)
    {
        if (invokeEvent)
        {
            InvokeOnEnemyDestroyed(destroyPoints);
        }
        if (scoreText)
        {
            scoreText = Instantiate(scoreText, new Vector3(transform.position.x, transform.position.y, -5), transform.rotation);
            scoreText.text = destroyPoints.ToString();
        }
        Instantiate(explosion, new Vector3(transform.position.x, transform.position.y, constants.explosionOffset), transform.rotation);
        Destroy(gameObject);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
    }

    protected virtual void StartRotation(float directionToHitY)
    {
        StartCoroutine(Rotate(directionToHitY));
    }

    IEnumerator Rotate(float directionToHitY)
    {
        float timer = Time.time + rotateTime;
        while (Time.time < timer)
        {
            transform.Rotate(new Vector3(0, 0, directionToHitY * rotateAmount));
            yield return new WaitForSeconds(0.1f);
        }
        timer = Time.time + rotateTime;
        while (Time.time < timer)
        {
            transform.Rotate(new Vector3(0, 0, -directionToHitY * rotateAmount));
            yield return new WaitForSeconds(0.1f);
        }
    }

    protected void InvokeOnEnemyDestroyed(int scoreIncrease)
    {
        onEnemyDestroyed?.Invoke(scoreIncrease);
    }
}
