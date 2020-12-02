using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : Hittable
{
    public enum State { GROUNDED, ABDUCTED, FALLING, RESCUED, DEAD, DEMO }
    public State curState;
    Utilities utilities;

    public float acceleration = 0.01f;
    public float dieOffset = 1;
    public float humanToHumanOffset = 0.2f;
    public float initialShipOffset = 0.5f;
    public GameObject explosion;

    private Transform currentGround;
    private float actualSpeed = 0;
    private float verticalHalfSize;
    private float verticalHalfSizeOffset = 0.8f;
    private bool shouldDie = true;

    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        verticalHalfSize = Camera.main.orthographicSize;
    }

    protected override void Start()
    {
        base.Start();
        utilities = Utilities.instance;

        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    protected override void Update()
    {
        base.Update();
        if (utilities.gameState == Utilities.GameState.STOPPED) return;

        if (curState == State.FALLING)
        {
            //transform.parent = currentGround;
            if (transform.position.y > -verticalHalfSize + verticalHalfSizeOffset)
            {
                actualSpeed += acceleration;
                transform.Translate(Vector2.down * actualSpeed * Time.deltaTime, Space.World);
            }
            else
            {
                if (shouldDie)
                {
                    DestroySelf();
                }
                else
                {
                    curState = State.GROUNDED;
                    shouldDie = true;
                }
            }
        }
        else
        {
            actualSpeed = 0;
        }
        if (curState == State.RESCUED)
        {
            if (transform.position.y <= -verticalHalfSize + verticalHalfSizeOffset)
            {
                transform.parent.GetComponent<ShipController>().RemoveHuman(GetComponent<Human>());
                audioSource.Play();
                transform.parent = null;
                curState = State.GROUNDED;
            }
        }
        if (curState == State.GROUNDED)
        {
            // default
            gameObject.layer = 0;
            //transform.parent = currentGround;
        }
        else
        {
            // hittable
            gameObject.layer = 8;
        }
    }

    /// <summary>
    /// Set the human state to falling and detect if the human will die upon hitting the ground.
    /// </summary>
    /// <param name="newParent">The humans new parent (the current background).</param>
    public void SetToFalling()
    {
        shouldDie = true;
        float correctedPos = transform.position.y + verticalHalfSize;
        if (correctedPos < dieOffset)
        {
            //human can live if hit ground
            shouldDie = false;
        }
        transform.SetParent(null);
        curState = State.FALLING;
    }

    public void SetToAbducted(Transform alienTransform, float humanOffset)
    {
        transform.position = new Vector2(alienTransform.position.x, alienTransform.position.y - humanOffset);
        transform.SetParent(alienTransform);
        curState = State.ABDUCTED;
        shouldWrap = false; // (Alien takes care of the wrapping)
    }

    /// <summary>
    /// Set the human state to rescued and set it's position and layer corresponding to the humanCount.
    /// </summary>
    /// <param name="shipTransform">The player ship</param>
    /// <param name="humanCount">The number of humans currently rescued (not including this one)</param>
    public void SetToRescued(Transform shipTransform, int humanCount)
    {
        float offsetFromShip = -initialShipOffset - (humanCount * humanToHumanOffset);

        transform.SetParent(shipTransform);
        transform.position = new Vector2(shipTransform.position.x, shipTransform.position.y + offsetFromShip);
        curState = State.RESCUED;
        spriteRenderer.sortingOrder = humanCount;
    }

    public override bool DamageSelf(float damage, Vector2 hitPosition, Vector2? bulletDirection = null)
    {
        if (curState == State.FALLING || curState == State.ABDUCTED)
        {
            DestroySelf();
            return true;
        }
        return false;
    }

    protected void DestroySelf()
    {
        Instantiate(explosion, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    public void SetDemo()
    {
        curState = State.DEMO;
    }

    public State GetState()
    {
        return curState;
    }
}
