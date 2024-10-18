using UnityEngine.Audio;
using UnityEngine;


//Ääniluokka
//Pitää sisällään kaiken yhteen ääneen tarvittavat tiedot

[System.Serializable]
public class Sound
{
    public string name;
    public GameObject audioSource;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;
    [Range(0.3f, 3f)]
    public float pitch;

    [Range(0f, 1f)]
    public float spatialBlend;

    [Range(0f, 1f)]
    public float dopplerEffect;

    public bool loop;

    [HideInInspector]
    public AudioSource source;

    public float minRange;
    public float maxRange;

    public AudioRolloffMode rollOffType;
}
