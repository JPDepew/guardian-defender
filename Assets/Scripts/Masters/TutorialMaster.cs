using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMaster : MonoBehaviour
{
    public GameObject playerShip;
    public GameObject alien;
    public GameObject human;
    public Text instructionsText;

    private ShipController shipController;
    private int arrowKeysPressed = 0;

    void Start()
    {
        Application.targetFrameRate = 60;
        shipController = playerShip.GetComponent<ShipController>();

        StartCoroutine(TutorialController());
    }

    private void Update()
    {
        
    }

    IEnumerator TutorialController()
    {
        while (arrowKeysPressed < 2)
        {
            if (ArrowKeyPressed())
            {
                arrowKeysPressed++;
            }
            yield return null;
        }
        instructionsText.text = "";
        yield return new WaitForSeconds(1f);

        GameObject alienRef = Instantiate(alien, new Vector3(playerShip.transform.position.x + 4, playerShip.transform.position.y), Quaternion.Euler(Vector3.zero));
        Alien alienScript = alienRef.GetComponent<Alien>();
        yield return null;
        alienScript.health = 1;
        alienScript.curState = Alien.State.DEMO;

        shipController.demo = true;

        instructionsText.text = "Z to shoot";
        while (!Input.GetKeyDown(KeyCode.Z))
        {
            yield return null;
        }
        instructionsText.text = "Shot!!!";
        shipController.demo = false;
    }

    private bool ArrowKeyPressed()
    {
        return Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow);
    }
}
