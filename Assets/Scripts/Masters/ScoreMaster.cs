using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ScoreMaster : MonoBehaviour
{
    public int scoreRows = 10;
    public Text ellipsesText;
    public Text[] scoreNames;
    public Text[] scoreValues;

    private bool dataFetched = false;

    void Start()
    {
        StartCoroutine(AnimateEllipses());
        StartCoroutine(GetScores());
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
        using (UnityWebRequest www = UnityWebRequest.Get("https://us-central1-guardian-scoreboard.cloudfunctions.net/get_scores/?limit=60"))
        {
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
                    if (i % scoreRows == 0 && i > 0)
                    {
                        scoreTextCount++;
                    }
                    scoreNames[scoreTextCount].text += $"{i + 1}) {res.userScores[i].name}\n";
                    scoreValues[scoreTextCount].text += $"{res.userScores[i].score}\n";
                    yield return new WaitForSeconds(0.05f);
                }
            }
        }
    }
}
