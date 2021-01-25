using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    AudioSource[] audioSources;

    void Start()
    {
        audioSources = GetComponents<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <param name="minSoundIndex">The minimum sound index to play [inclusive]</param>
    /// <param name="maxSoundIndex">The maximum sound index to play [inclusive]</param>
    //public void PlayRandomSoundFromRange(int soundIdex, float minPitch, float maxPitch)
    //{
    //    int index = Random.Range(minSoundIndex, maxSoundIndex + 1);
    //    audioSources[index].Play();
    //}

    /// <param name="minSoundIndex">The minimum sound index to play [inclusive]</param>
    /// <param name="maxSoundIndex">The maximum sound index to play [inclusive]</param>
    public void PlayRandomSoundFromRange(int minSoundIndex, int maxSoundIndex)
    {
        int index = Random.Range(minSoundIndex, maxSoundIndex + 1);
        if (audioSources[index].isActiveAndEnabled)
        {
            audioSources[index].Play();
        }
    }

    /// <param name="minSoundIndex">The minimum sound index to play [inclusive]</param>
    /// <param name="maxSoundIndex">The maximum sound index to play [inclusive]</param>
    /// <param name="minPitch">The minimum pitch that the sound can be played at [inclusive]</param>
    /// <param name="maxPitch">The maximum pitch that the sound can be played at [inclusive]</param>
    public void PlayRandomSoundFromRange(int minSoundIndex, int maxSoundIndex, float minPitch, float maxPitch)
    {
        int index = Random.Range(minSoundIndex, maxSoundIndex + 1);
        audioSources[index].pitch = Random.Range(minPitch, maxPitch);
        audioSources[index].Play();
    }
}
