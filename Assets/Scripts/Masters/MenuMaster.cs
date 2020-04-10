using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMaster : MonoBehaviour {

    public UI ui;

	void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ui.Play();
        }
	}
}
