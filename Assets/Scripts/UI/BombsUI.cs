using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombsUI : MonoBehaviour
{
    public Transform bombsParent;
    public GameObject bombUI;
    public float offset = 0.5f;

    Stack<GameObject> bombs;
    PlayerStats playerStats;
    int bombsCount;

    void Start()
    {
        playerStats = PlayerStats.instance;
        bombsCount = playerStats.bombsCount;
        bombs = new Stack<GameObject>();

        for (int i = 0; i < bombsCount; i++)
        {
            bombs.Push(
                Instantiate(bombUI, new Vector3(bombsParent.position.x + offset * i, bombsParent.position.y), bombsParent.rotation, bombsParent)
            );
        }

        PowerupObj.onGetPowerup += OnGetPowerup;
        ShipController.onBomb += OnBomb;
    }

    void OnGetPowerup(Powerup powerup)
    {
        bombs.Push(
            Instantiate(bombUI, new Vector3(bombsParent.position.x + offset * bombsCount, bombsParent.position.y), bombsParent.rotation, bombsParent)
        );
        bombsCount++;
    }

    void OnBomb()
    {
        GameObject bombToRemove = bombs.Pop();
        Destroy(bombToRemove);
        bombsCount--;
    }

    private void OnDestroy()
    {
        PowerupObj.onGetPowerup -= OnGetPowerup;
        ShipController.onBomb -= OnBomb;
    }
}
