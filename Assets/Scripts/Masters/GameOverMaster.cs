using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMaster : MonoBehaviour
{
    public Text scoreText;
    public InputField nameInput;
    public Button submitButton;

    private bool submitting = false;

    private void Start()
    {
        if (Constants.instance.score > Constants.instance.highScore)
        {
            Constants.instance.SetHighScore();
        }
        nameInput.Select();
        scoreText.text = Data.Instance ? $"Score: {Data.Instance.score.ToString()}" : $"Score: 0";
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
            nameInput.interactable = false;
            submitButton.interactable = false;
            submitButton.GetComponentInChildren<Text>().text = "...";
            StartCoroutine(UploadScore());
        }
    }

    IEnumerator UploadScore()
    {
        string score = Data.Instance ? Data.Instance.score.ToString() : "0";
        WWWForm form = new WWWForm();
        form.AddField("name", nameInput.text);
        form.AddField("score", score);
        submitting = true;

        using (UnityWebRequest www = UnityWebRequest.Post("https://us-central1-guardian-scoreboard.cloudfunctions.net/post_score/", form))
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