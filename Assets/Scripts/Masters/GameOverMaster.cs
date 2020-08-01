using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMaster : MonoBehaviour
{
    public Text scoreText;
    public InputField nameInput;

    private bool submitting = false;

    private void Start()
    {
        print(Data.Instance.score);
        if (Constants.instance.score > Constants.instance.highScore)
        {
            Constants.instance.SetHighScore();
        }
        nameInput.Select();
        scoreText.text = "Score: " + Data.Instance.score.ToString();
        Constants.instance.resetScore();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SubmitHighScore();
        }
    }

    public void SubmitHighScore()
    {
        if (!submitting)
        {
            StartCoroutine(UploadScore());
        }
    }

    IEnumerator UploadScore()
    {
        WWWForm form = new WWWForm();
        form.AddField("name", nameInput.text);
        form.AddField("score", Data.Instance.score);
        submitting = true;

        using (UnityWebRequest www = UnityWebRequest.Post("https://guardian-scoreboard.ue.r.appspot.com/create_user_score/", form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }
            SceneManager.LoadScene("ScoreBoard");
        }
    }
}