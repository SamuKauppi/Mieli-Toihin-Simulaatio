using UnityEngine;
using System;

//Scripti, joka vastaa peliscenen sisäisistä äänistä
public class AudioManager : MonoBehaviour
{
   [SerializeField] private Sound[] sounds;     //Lista ääniefekti luokista (tänne määritetään ääniefektit
    
    //Soittaa ääniefektin
    //Paramterinä ääniefektin nimi, jonka avulla määrittää mitä halutaan
    //Peliobjektin, josta ääni tulee
    //Indeksin, joka määrittää, mistä huoneesta ääni tulee (-1 ohittaa tämän)
    //Ääniefekti pitää määrittää editorissa
    //Jos ääniindeksi ei ole -1, katsoo AmbianceScript-skriptiä ja määrittää äänentason perustuen siihen
    public void Play(string name, GameObject from, int soundIndex)
    {
        //Etsitään ääni ja jos ei löydy, niin lopetetaan suoritus
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("Sound " + name + " was not found");
            return;
        }

        //Katsotaan onko peliobjektissa äänen lähde ja lisätään, jos ei ole
        if (from.TryGetComponent(typeof(AudioSource), out Component component))
        {
            s.source = component.GetComponent<AudioSource>();
        }
        else
        {
            s.source = from.AddComponent<AudioSource>();
        }
        //Lisätään kaikki tarvittava tieto äänenlähteeseen ääniluokasta
        s.source.clip = s.clip;
        s.source.pitch = s.pitch;
        s.source.loop = s.loop;
        s.source.spatialBlend = s.spatialBlend;
        s.source.minDistance = s.minRange;
        s.source.maxDistance = s.maxRange;
        s.source.rolloffMode = s.rollOffType;
        s.source.dopplerLevel = s.dopplerEffect;

        //Määritetään äänen voluumi perustuen ääniindeksiin. -1 pitää äänentason normaalina
        if (soundIndex != -1)
        {
            s.source.volume = PersistentManager.Instance.ambManager.CalculateVolumeModifier(s.volume, soundIndex);
        }
        else
        {
            s.source.volume = s.volume;
        }

        //Soitetaan ääni, jos se on aktiivinen 
        if (s.source.gameObject.activeInHierarchy)
        {
            s.source.Play();
        }
    }
}
