using System.Collections;
using UnityEngine;

//Skripti, joka soittaa musiikkia radio objektista
//Toimii eri systeemillä, kuin äänimanagerin äänet
public class Radio : MonoBehaviour
{
    AudioSource mySource;       //Äänenlähde
    Vector3 originalSize;       //Radio-objektin alkuperäinen koko (käytetään animaatiossa)
    [SerializeField] private Sound[] clips;       //Kaikki musiikit (määritetään editorissa)
    [SerializeField] private int clipToBePlayed;  //Pitää muistissa, mikä musiikki pyörii nyt ja sen perusteella valitsee seuraavan

    //Aloittaa satunnaisen musiikin soiton Start metodissa
    void Start()
    {
        originalSize = transform.localScale;                            //Koko liittyy radion pomppimis animaatioon
        clipToBePlayed = Random.Range(0, clips.Length);                 //Valitsee satunnaisen int. Määrittää mikä musiikki soitetaan
        mySource = gameObject.GetComponent<AudioSource>();              //Haetaan äänenlähde
        while (!clips[clipToBePlayed].clip)
        {
            clipToBePlayed = Random.Range(0, clips.Length);
        }
        StartCoroutine(PlayMusic(clips[clipToBePlayed].clip.length));   //Aloitetaan musiikki
    }

    //Soittaa musiikkia biisin pituuden verran
    //Sitten 5-15s jälkeen soittaa seuraavan klipin
    IEnumerator PlayMusic(float time)
    {
        yield return new WaitForSecondsRealtime(0.25f);
        PlayingRadioAnimation();

        if (!clips[clipToBePlayed].clip)
        {
            PlayNextClip();
            StopRadioAnimation();
            StopAllCoroutines();
        }

        clips[clipToBePlayed].source = mySource;
        mySource.clip = clips[clipToBePlayed].clip;
        mySource.volume = clips[clipToBePlayed].volume * 0.38f;
        mySource.pitch = clips[clipToBePlayed].pitch;
        mySource.spatialBlend = clips[clipToBePlayed].spatialBlend;
        mySource.Play();

        yield return new WaitForSeconds(time * 1.2f);
        StopRadioAnimation();
        yield return new WaitForSecondsRealtime(Random.Range(5f, 15f));
        PlayNextClip();
    }

    //Kutsuttaessa soittaa seuraavan biisin
    public void PlayNextClip()
    {
        StopRadioAnimation();
        StopAllCoroutines();
        mySource.Stop();
        clipToBePlayed++;
        if (clipToBePlayed >= clips.Length)
        {
            clipToBePlayed = 0;
        }
        if(!clips[clipToBePlayed].clip)
        {
            PlayNextClip();
            return;
        }
        StartCoroutine(PlayMusic(clips[clipToBePlayed].clip.length));
    }

    //Kutsuttaessa laittaa musiikkin pois tai päälle
    //(ei käytössä)
    public void ToggleRadioMusic(bool b)
    {
        if (!b)
        {
            mySource.volume = 0;
        }
        else
        {
            mySource.volume = clips[clipToBePlayed].volume * 0.38f;
        }
    }

    //Radion fyysinen animaatio
    //Aloitetaan animaatio
    void PlayingRadioAnimation()
    {
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, (originalSize * 0.8f) * 2f, 0.7f).setLoopPingPong().setEase(LeanTweenType.easeInOutElastic);
    }
    //Lopetateaan animaatio
    void StopRadioAnimation()
    {
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, originalSize, 0.2f);
    }
}
