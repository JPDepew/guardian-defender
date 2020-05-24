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

    void Start()
    {
        playerStats = PlayerStats.instance;
        bombs = new Stack<GameObject>();

        for (int i = 0; i < playerStats.bombsCount; i++)
        {
            bombs.Push(
                Instantiate(bombUI, new Vector3(bombsParent.position.x + offset * i, bombsParent.position.y), bombsParent.rotation, bombsParent)
            );
        }

        PowerupObj.onGetPowerup += OnGetPowerup;
        PlayerPowerups.onBomb += OnBomb;
    }

    void OnGetPowerup(Powerup powerup)
    {
        if (powerup == Powerup.Bomb)
        {
            bombs.Push(
                Instantiate(bombUI, new Vector3(bombsParent.position.x + offset * playerStats.bombsCount, bombsParent.position.y), bombsParent.rotation, bombsParent)
            );
        }
    }

    void OnBomb()
    {
        GameObject bombToRemove = bombs.Pop();
        Destroy(bombToRemove);
    }

    private void OnDestroy()
    {
        PowerupObj.onGetPowerup -= OnGetPowerup;
        PlayerPowerups.onBomb -= OnBomb;
    }
}
