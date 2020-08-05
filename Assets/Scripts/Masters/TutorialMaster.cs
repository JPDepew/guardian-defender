using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialMaster : MonoBehaviour
{
    public GameObject playerShip;
    public GameObject alien;
    public GameObject human;
    public GameObject mutatedAlien;
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

    private IEnumerator TutorialController()
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

        yield return InstantiateFirstAlien();



        yield return new WaitForSeconds(1f);

        yield return InstantiateAlienAndHuman();

        yield return InstantiateInfectedAlien();

        yield return new WaitForSeconds(1f);

        yield return BombInstructions();
    }

    private IEnumerator InstantiateFirstAlien()
    {
        GameObject alienRef;
        if (shipController.leftShip.activeSelf == true)
        {
            alienRef = Instantiate(alien, new Vector3(playerShip.transform.position.x - 4, playerShip.transform.position.y), Quaternion.Euler(Vector3.zero));
        }
        else
        {
            alienRef = Instantiate(alien, new Vector3(playerShip.transform.position.x + 4, playerShip.transform.position.y), Quaternion.Euler(Vector3.zero));
        }
        yield return null;
        FreezeAlien(alienRef);

        shipController.demo = true;
        shipController.fuelParticleSystem.Stop();
        shipController.StopEngineAudio();

        instructionsText.text = "Z to shoot";

        while (alienRef != null)
        {
            yield return null;
        }
        instructionsText.text = ":)";
        shipController.demo = false;
    }

    private IEnumerator InstantiateAlienAndHuman()
    {
        GameObject alienRef = Instantiate(alien, new Vector3(playerShip.transform.position.x + 4, -4), Quaternion.Euler(Vector3.zero));
        GameObject humanRef = Instantiate(human, new Vector3(playerShip.transform.position.x + 4, -5.5f), Quaternion.Euler(Vector3.zero));
        Transform alienTransform = alienRef.transform;
        instructionsText.text = "Aliens abduct humans";

        while (alienTransform.position.y < 1)
        {
            yield return null;
        }

        FreezeAlien(alienRef);
        instructionsText.text = "Shoot the alien";
        while(alienRef != null)
        {
            if (humanRef == null)
            {
                instructionsText.text = "Wow, you suck. Shoot the alien";
                humanRef = Instantiate(
                    human,
                    new Vector3(alienRef.transform.position.x, alienRef.transform.position.y - 0.4f),
                    Quaternion.Euler(Vector3.zero)
                );
                yield return null;
                FreezeAlien(alienRef);
            }
            yield return null;
        }

        instructionsText.text = "Nice";
        yield return new WaitForSeconds(0.8f);
        instructionsText.text = "Wait";
        yield return new WaitForSeconds(0.4f);
        instructionsText.text = "...";
        yield return new WaitForSeconds(0.2f);
        instructionsText.text = "Oh crap";
        yield return new WaitForSeconds(0.8f);

        humanRef.GetComponent<Human>().SetDemo();
        Vector3 humanPos = humanRef.transform.position;

        instructionsText.text = "Go rescue the human";

        while (!shipController.HasHumans())
        {
            yield return null;
        }
        instructionsText.text = "Return the human to the ground";
        while (shipController.HasHumans())
        {
            yield return null;
        }
    }

    private IEnumerator InstantiateInfectedAlien()
    {

        instructionsText.text = "Mutated aliens are more dangerous";
        GameObject mutatedAlienRef = Instantiate(mutatedAlien, new Vector3(playerShip.transform.position.x + 2.5f, 4), Quaternion.Euler(Vector3.zero));
        MutatedAlien mutatedAlienScript = mutatedAlienRef.GetComponent<MutatedAlien>();
        yield return null;
        mutatedAlienScript.SetDirection(Vector2.down);
        yield return new WaitForSeconds(0.4f);
        mutatedAlienScript.SetDemo();

        yield return new WaitForSeconds(2f);

        instructionsText.text = "Press X to shoot the recombination gun and separate the mutated human";

        while (mutatedAlienRef != null)
        {
            yield return null;
        }

        instructionsText.text = "";

        Human humanScript = FindObjectOfType<Human>();
        humanScript.SetToFalling();

        while (humanScript != null && !shipController.HasHumans())
        {
            yield return null;
        }
        if (humanScript == null)
        {
            instructionsText.text = "Ouch. Poor guy. You could have rescued him.";
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator BombInstructions()
    {
        instructionsText.text = "One last thing";
        yield return new WaitForSeconds(0.5f);
        instructionsText.text = "C - activate bomb";
    }

    private void FreezeAlien(GameObject alien)
    {
        Alien alienScript = alien.GetComponent<Alien>();
        alienScript.health = 1;
        alienScript.curState = Alien.State.DEMO;
    }

    private bool ArrowKeyPressed()
    {
        return Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow);
    }
}
