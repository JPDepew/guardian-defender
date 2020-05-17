using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimationsMaster : MonoBehaviour
{
    private PlayerStats playerStats;
    public Text popupScoreText;
    public GameObject canvas;

    // Start is called before the first frame update
    void Start()
    {
        // setting instance refs
        playerStats = PlayerStats.instance;
    }

    public void InstanatiateScorePopup(int scoreIncrease, Vector3 position)
    {
        print(scoreIncrease);
        Text popupText = Instantiate(popupScoreText, new Vector2(position.x, position.y + 0.5f), transform.rotation, canvas.transform);
        popupText.text = scoreIncrease.ToString();
        StartCoroutine(AnimatePopupText(popupText));
        playerStats.IncreaseScoreBy(scoreIncrease);
    }

    /// <summary>
    /// Starts the animations for moving and fading out the popup text
    /// Realtime to account for time slow down powerup.
    /// </summary>
    /// <param name="popupText"></param>
    /// <returns></returns>
    IEnumerator AnimatePopupText(Text popupText)
    {
        StartCoroutine(MovePopupText(popupText.transform));
        yield return new WaitForSecondsRealtime(0.5f);
        yield return StartCoroutine(FadeOutPopupText(popupText));
        Destroy(popupText.gameObject);
    }

    IEnumerator MovePopupText(Transform popupTransform)
    {
        float moveAmount = 1f;
        float moveDecreaseFraction = 0.95f;
        float seconds = 0.75f;
        float targetTime = Time.realtimeSinceStartup + seconds;

        while (Time.realtimeSinceStartup < targetTime)
        {
            popupTransform.transform.position = popupTransform.transform.position + (Vector3)(Vector2.up * moveAmount * Time.unscaledDeltaTime);
            moveAmount *= moveDecreaseFraction;
            yield return 0;
        }
    }

    IEnumerator FadeOutPopupText(Text popupText)
    {
        Color curTextColor = popupText.color;
        float alphaDecreaseAmt = 0.05f;

        while (curTextColor.a >= 0)
        {
            popupText.color = new Color(curTextColor.r, curTextColor.g, curTextColor.b, popupText.color.a - alphaDecreaseAmt);
            yield return 0;
        }
    }
}
