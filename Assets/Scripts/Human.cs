﻿using System.Collections;
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
    public float moveSpeed = 3;
    public GameObject explosion;
    public GroundLineRenderer frontGroundLineRenderer;

    private Transform currentGround;
    private float actualSpeed = 0;
    private float verticalHalfSize;
    private float verticalHalfSizeOffset = 0.8f;
    private float groundLineRendererPosY;
    int moveDirection;
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
        frontGroundLineRenderer = GameObject.FindGameObjectWithTag("Ground Line Renderer").GetComponent<GroundLineRenderer>();
        groundLineRendererPosY = frontGroundLineRenderer.transform.position.y;

        float randomMovement = Random.Range(0.8f, 1.2f);
        moveSpeed *= randomMovement;

        moveDirection = Random.Range(0, 2) > 0 ? 1 : -1;
    }

    protected override void Update()
    {
        base.Update();
        if (utilities.gameState == Utilities.GameState.STOPPED) return;

        if (curState == State.FALLING)
        {
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
            gameObject.layer = 0;
            HandleMovement();
        }
        else
        {
            // hittable
            gameObject.layer = 8;
        }
    }

    void HandleMovement()
    {
        float yOffset = frontGroundLineRenderer.GetYPoint(transform.position.x);
        float negativeOffset = 0.4f;
        float targetYMovement = groundLineRendererPosY + yOffset - negativeOffset - transform.position.y;//, -moveSpeed * Time.deltaTime, moveSpeed * Time.deltaTime);
        Vector2 targetPos = new Vector2(transform.position.x + moveSpeed * moveDirection * Time.deltaTime, transform.position.y + targetYMovement * Time.deltaTime);
        transform.position = Vector2.Lerp(transform.position, targetPos, 0.5f);
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
