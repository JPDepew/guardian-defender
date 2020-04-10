using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienExplosion : MonoBehaviour {

    AudioSource[] audioSources;
    
	void Start () {
        audioSources = GetComponents<AudioSource>();
        int index = Random.Range(0, 3);
        audioSources[index].Play();
    }
}
