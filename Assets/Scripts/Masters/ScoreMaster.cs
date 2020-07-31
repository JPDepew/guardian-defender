using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ScoreMaster : MonoBehaviour
{
    public int scoreRows = 17;
    public Text ellipsesText;
    public Text[] scoreTexts;

    private bool dataFetched = false;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(AnimateEllipses());
        StartCoroutine(GetScores());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator AnimateEllipses()
    {
        while (!dataFetched)
        {
            int periodCount = ellipsesText.text.Length % 3;
            string newText = "";
            for (int i = 0; i < periodCount + 1; i++)
            {
                newText += ".";
            }
            ellipsesText.text = newText;
            yield return new WaitForSeconds(0.5f);
        }
        ellipsesText.text = "";
    }

    IEnumerator GetScores()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("https://guardian-scoreboard.ue.r.appspot.com/get_user_scores/"))
        {
            www.SetRequestHeader("Best-Header", "test");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                RootUserScores res = JsonUtility.FromJson<RootUserScores>("{\"userScores\":" + www.downloadHandler.text + "}");
                int scoreTextCount = 0;
                dataFetched = true;
                for (int i = 0; i < res.userScores.Length; i++)
                {
                    if (i >= scoreRows)
                    {
                        scoreTextCount++;
                    }
                    scoreTexts[scoreTextCount].text += $"{res.userScores[i].name}:\t{res.userScores[i].score}\n";
                    yield return new WaitForSeconds(0.05f);
                }
            }
        }
    }
}
