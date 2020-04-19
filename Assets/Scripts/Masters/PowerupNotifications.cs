using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PowerupNotifications : MonoBehaviour
{
    public Text powerupInstructions;
    public float displayTime = 3f;

    Animator textAnimator;
    Constants constants;

    void Start()
    {
        PowerupObj.onGetPowerup += OnPowerupActivate;

        textAnimator = powerupInstructions.GetComponent<Animator>();
        constants = Constants.instance;
    }

    void OnPowerupActivate(Powerup powerup)
    {
        PowerupObj powerupObj = constants.PowerupObjByEnum(powerup);
        if (powerupObj.keyCode != KeyCode.None)
        {
            StartCoroutine(PlayAnimations(powerupObj));
        }
    }

    IEnumerator PlayAnimations(PowerupObj powerupObj)
    {
        powerupInstructions.text = $"{powerupObj.keyCode.ToString()} to activate {powerupObj.powerupName}";
        textAnimator.Play("PowerupInstructionsFadeIn");
        yield return new WaitForSeconds(displayTime);
        textAnimator.Play("PowerupInstructionsFadeOut");

    }
}
