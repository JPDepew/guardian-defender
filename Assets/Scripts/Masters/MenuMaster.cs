using UnityEngine;

public class MenuMaster : MonoBehaviour {

    public UI ui;

	void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ui.Play();
        }
	}
}
