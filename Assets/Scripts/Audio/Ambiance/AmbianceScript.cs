using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Skirpit, joka muuttaa ‰‰ni‰, kun pelaaja k‰velee huoneesta toiseen
//Toimii hyvin samalla tavalla kuin RoomList-skripti, mutta eroaa muutamalla tavalla
public class AmbianceScript : MonoBehaviour
{
    AudioManager aManager;                                                          //AudioManageri (Nopeuttaa siihen viittausta)

    public List<AmbianceTrigger> currentTriggers = new List<AmbianceTrigger>();     //Lista triggerist‰ (Vain ne miss‰ huoneessa pelaaja on)

    public AmbianceTrigger outSide;                                                 //Ainoa poikkeus on Ulkotila (Pit‰‰ aina muistaa)

    public List<int> currentRooms = new List<int>();                                //Nykyisten huoneiden indeksit
    public List<int> currentNeighbours = new List<int>();                           //Naapuri huoneiden indeksit

    public GameObject[] ambianceSounds;                                             //Peliobjektit, joista toistetaan ambianssi ‰‰net

    //Varmistetaan, ett‰ pelaaja on ulkona
    //Ja m‰‰ritet‰‰n ambianssi‰‰net 
    private void Start()
    {
        currentTriggers.Add(outSide);
        ResetRoomsAndNeigbours();
        aManager = PersistentManager.Instance.aManager;
        aManager.Play("linnunlaulu", ambianceSounds[0], 0);
        aManager.Play("ulkotuuli", ambianceSounds[1], 0);
    }

    //Pelaaja saapuu huoneeseen
    //Lis‰t‰‰n indeksin perusteella huone ja sen naapurit 
    public void EnterARoom(AmbianceTrigger currentRoomIndex)
    {
        //Jos ensimm‰isen triggerin indeksi on 0 ollaan ulkona ja tullaan sis‰lle
        if (currentTriggers[0].myIndex == 0)
        {
            //Tyhjennet‰‰n triggeri lista (ulkotila pois) ja muutetaan ambianssi ‰‰ni‰
            currentTriggers.Clear();
            ambianceSounds[0].GetComponent<AudioSource>().Stop();
            ambianceSounds[1].GetComponent<AudioSource>().Stop();
            aManager.Play("sisailma", ambianceSounds[2], -1);
        }
        //Lis‰t‰‰n huone, johon tultiin triggereiden joukkoon
        currentTriggers.Add(currentRoomIndex);
        ResetRoomsAndNeigbours();
    }

    //Pelaaja poistuu huoneesta
    public void ExitARoom(AmbianceTrigger currentRoomIndex)
    {
        //Poistetaan huone, josta pelaaja poistui
        currentTriggers.Remove(currentRoomIndex);

        //Jos lista on tyhj‰, tiedet‰‰n, ett‰ pelaaja on ulkona 
        if (currentTriggers.Count == 0)
        {
            //Lis‰t‰‰n ulkotila ja m‰‰ritell‰‰n ambianssi
            currentTriggers.Add(outSide);
            aManager.Play("linnunlaulu", ambianceSounds[0], -1);
            aManager.Play("ulkotuuli", ambianceSounds[1], -1);
            ambianceSounds[2].GetComponent<AudioSource>().Stop();

        }
        ResetRoomsAndNeigbours();
    }


    //Suoritetaan, joka kerta kun pelaaja saapuu tai poistuu
    //Naapuri lista korjataan poistamalla sielt‰ nykyisen huoneiden indeksit ja kopiot
    void ResetRoomsAndNeigbours()
    {
        //Tyhjennet‰‰n nykyiset indeksit ja naapurit
        currentRooms.Clear();
        currentNeighbours.Clear();

        //Lis‰t‰‰n nykyisten triggereiden indeksit ja naapurit listaan
        foreach (var item in currentTriggers)
        {
            currentRooms.Add(item.myIndex);
            currentNeighbours.AddRange(item.myNeighbours);
        }
        //Poistetaan naapureista kaikki samat arvot
        currentNeighbours.Distinct().ToList();

        //poistetaaan naapureista kaikki nykyisten huoneiden indeksit
        foreach (var item in currentRooms)
        {
            currentNeighbours.Remove(item);
        }
    }

    //Suoritetaan audiomanagerista
    //Laskee ‰‰nen voimakkuuden tason sen perusteella miss‰ huoneessa pelaaja on
    //ƒ‰nen l‰hteest‰ tulee indeksi, jota verrataan nykyisiin ja naapuri huoneisiin
    //Nykyinen huone = 100% ‰‰ni
    //Naapuri huone = v‰hennetty ‰‰ni
    //Ei ole naapuri tai nykyinen = 0% ‰‰ni
    public float CalculateVolumeModifier(float volume, int soundIndex)
    {
        if (currentRooms.Contains(soundIndex) || soundIndex < 0)
        {
            return volume;
        }
        else if (currentNeighbours.Contains(soundIndex))
        {
            return volume * 0.25f;
        }
        else
        {
            return 0;
        }
    }
}
