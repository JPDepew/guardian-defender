using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialMaster : MonoBehaviour
{
    public GameObject playerShip;
    public GameObject alien;
    public GameObject human;
    public GameObject mutatedAlien;
    public GameObject pausePanel;
    public Text instructionsText;
    public GameObject greenCircle;

    private AudioSource ambience;
    private ShipController shipController;
    private int arrowKeysPressed = 0;

    void Start()
    {
        Application.targetFrameRate = 60;
        shipController = playerShip.GetComponent<ShipController>();
        shipController.demo = true;
        ambience = GetComponent<AudioSource>();

        StartCoroutine(TutorialController());
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
        Vector3 alienPos;
        float alienOffset;
        if (shipController.leftShip.activeSelf == true)
        {
            alienOffset = -4;
            alienPos = new Vector3(playerShip.transform.position.x + alienOffset, playerShip.transform.position.y);
            alienRef = Instantiate(alien, alienPos, Quaternion.Euler(Vector3.zero));
        }
        else
        {
            alienOffset = 4;
            alienPos = new Vector3(playerShip.transform.position.x + alienOffset, playerShip.transform.position.y);
            alienRef = Instantiate(alien, alienPos, Quaternion.Euler(Vector3.zero));
        }
        yield return null;
        // Stupid, but accounts for a lag
        alienPos = new Vector3(playerShip.transform.position.x + alienOffset, playerShip.transform.position.y);
        alienRef.transform.position = alienPos;
        FreezeAlien(alienRef);

        FreezeShip();
        FreezeTutorial();

        instructionsText.text = "Z to shoot";

        while (alienRef != null)
        {
            yield return null;
        }
        UnFreezeTutorial();
        instructionsText.text = ":)";
        shipController.frozen = false;
    }

    private IEnumerator InstantiateAlienAndHuman()
    {
        GameObject alienRef = Instantiate(alien, new Vector3(playerShip.transform.position.x + 4, -4), Quaternion.Euler(Vector3.zero));
        GameObject humanRef = Instantiate(human, new Vector3(playerShip.transform.position.x + 4, -5.5f), Quaternion.Euler(Vector3.zero));
        Transform alienTransform = alienRef.transform;
        instructionsText.text = "Aliens abduct humans";
        FreezeTutorial();

        FreezeShip();

        yield return null;

        // Just so the player doesn't get any ideas
        alienRef.GetComponent<Alien>().health = 20;
        Time.timeScale = 3;

        while (alienTransform.position.y < 1)
        {
            if (humanRef == null)
            {
                instructionsText.text = "Wow. Why would you shoot a human?";
                humanRef = Instantiate(
                    human,
                    new Vector3(alienRef.transform.position.x, alienRef.transform.position.y - 0.4f),
                    Quaternion.Euler(Vector3.zero)
                );
                yield return null;
            }
            yield return null;
        }
        Time.timeScale = 1;

        FreezeAlien(alienRef);
        instructionsText.text = "Shoot the alien";
        shipController.frozen = false;
        UnFreezeTutorial();
        while (alienRef != null)
        {
            if (humanRef == null)
            {
                instructionsText.text = "Wow. Murderer. Shoot the alien";
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

        Human humanScript = humanRef.GetComponent<Human>();

        if (!shipController.HasHumans() && humanScript.GetState() != Human.State.GROUNDED)
        {
            humanScript.SetDemo();
        }
        GameObject greenCircleRef = Instantiate(greenCircle, humanRef.transform.position, Quaternion.Euler(Vector3.zero));
        instructionsText.text = "Go rescue the human";
        FreezeTutorial();

        while (!shipController.HasHumans() && humanScript.GetState() != Human.State.GROUNDED)
        {
            yield return null;
        }
        Destroy(greenCircleRef);
        UnFreezeTutorial();
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
        FreezeTutorial();

        yield return new WaitForSeconds(2f);

        instructionsText.text = "Press X to shoot the recombination gun and separate the mutated human";

        while (mutatedAlienRef != null)
        {
            yield return null;
        }
        UnFreezeTutorial();
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
        GameObject alienRef1 = Instantiate(alien, new Vector3(playerShip.transform.position.x + 4, -4), Quaternion.Euler(Vector3.zero));
        GameObject alienRef2 = Instantiate(alien, new Vector3(playerShip.transform.position.x - 4, -3), Quaternion.Euler(Vector3.zero));
        GameObject alienRef3 = Instantiate(alien, new Vector3(playerShip.transform.position.x - 4.5f, -3), Quaternion.Euler(Vector3.zero));
        GameObject alienRef4 = Instantiate(alien, new Vector3(playerShip.transform.position.x + 3, 2), Quaternion.Euler(Vector3.zero));
        GameObject alienRef5 = Instantiate(alien, new Vector3(playerShip.transform.position.x + 2, 3), Quaternion.Euler(Vector3.zero));
        GameObject alienRef6 = Instantiate(alien, new Vector3(playerShip.transform.position.x + 2, -1), Quaternion.Euler(Vector3.zero));
        GameObject alienRef7 = Instantiate(alien, new Vector3(playerShip.transform.position.x - 4, -4), Quaternion.Euler(Vector3.zero));
        FreezeShip();
        yield return new WaitForSeconds(0.5f);
        FreezeTutorial();
        FreezeAlien(alienRef1);
        FreezeAlien(alienRef2);
        FreezeAlien(alienRef3);
        FreezeAlien(alienRef4);
        FreezeAlien(alienRef5);
        FreezeAlien(alienRef6);
        FreezeAlien(alienRef7);
        yield return new WaitForSeconds(1.25f);
        instructionsText.text = "C - activate bomb";

        while (!Input.GetKeyDown(KeyCode.C))
        {
            yield return null;
        }
        shipController.frozen = false;
        UnFreezeTutorial();
        yield return new WaitForSeconds(0.6f);
        instructionsText.text = "¯\\_(ツ)_/¯";
        yield return new WaitForSeconds(3f);
        instructionsText.text = "Press space to play";
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }
        SceneManager.LoadScene("mainScene");
    }

    private void FreezeTutorial()
    {
        ambience.Pause();
        pausePanel.SetActive(true);
    }

    private void UnFreezeTutorial()
    {
        ambience.Play();
        pausePanel.SetActive(false);
    }

    private void FreezeAlien(GameObject alien)
    {
        Alien alienScript = alien.GetComponent<Alien>();
        alienScript.health = 1;
        alienScript.curState = Alien.State.DEMO;
    }

    private void FreezeShip()
    {
        shipController.frozen = true;
        shipController.fuelParticleSystem.Stop();
        shipController.StopEngineAudio();
    }

    private bool ArrowKeyPressed()
    {
        return Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow);
    }
}
