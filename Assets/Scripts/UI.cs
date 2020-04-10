using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
    AudioSource[] audioSource;
    public Animator Instructions;
    bool zDownLastFrame = false;

    // Use this for initialization
    void Start()
    {
        audioSource = GetComponents<AudioSource>();
    }

    public void Play()
    {
        audioSource[0].Play();
        StartCoroutine(DelayedLoad(2));
    }

    public void ShowInstructions()
    {
        audioSource[0].Play();
        StartCoroutine(CycleInstructions());
    }

    public void CloseInstructions()
    {
        audioSource[0].Play();
    }

    public void Exit()
    {
        audioSource[0].Play();
        Application.Quit();
    }

    public void LoadCredits()
    {
        audioSource[0].Play();
        StartCoroutine(DelayedLoad(4));
    }

    IEnumerator DelayedLoad(int buildIndex)
    {
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(buildIndex);
    }

    IEnumerator CycleInstructions()
    {
        Instructions.gameObject.SetActive(true);
        Instructions.Play("Instructions1");
        while (true)
        {
            if (zDownLastFrame && Input.GetKeyUp(KeyCode.Z))
            {
                //Input.ResetInputAxes();
                zDownLastFrame = false;
                StartCoroutine(CycleInstructions2());
                break;
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                zDownLastFrame = true;  
            }
            yield return null;
        }
    }

    IEnumerator CycleInstructions2()
    {
        Instructions.Play("Instructions2");
        while (true)
        {
            if (zDownLastFrame && Input.GetKeyUp(KeyCode.Z))
            {
                //Input.ResetInputAxes();
                zDownLastFrame = false;
                StartCoroutine(CycleInstructions3());
                break;
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                zDownLastFrame = true;
            }
            yield return null;
        }
    }

    IEnumerator CycleInstructions3()
    {
        Instructions.Play("Instructions3");
        while (true)
        {
            if (zDownLastFrame && Input.GetKeyUp(KeyCode.Z))
            {
                //Input.ResetInputAxes();
                zDownLastFrame = false;
                StartCoroutine(CycleInstructions4());
                break;
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                zDownLastFrame = true;
            }
            yield return null;
        }
    }

    IEnumerator CycleInstructions4()
    {
        Instructions.Play("FadeOut");
        yield return new WaitForSeconds(Instructions.GetCurrentAnimatorStateInfo(0).length);
        Instructions.gameObject.SetActive(false);
    }
}
