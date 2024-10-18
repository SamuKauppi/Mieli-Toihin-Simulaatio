using System.Collections;
using UnityEngine;

//Vastaa pelin alussa objektien päälle laitosta ja mitä laiteaan päälle eri moodeissa
//Katsoo myös pelin alussa, että kaikki objektit laitetaan päälle
//Skriptin tehtävänä oli vähentää lagia pelin alussa, mutta en ole varmaa tekeekö se edes sitä
public class ModeChanger : MonoBehaviour
{
    public GameObject[] objectsToDo;                //Objekteja, joiden näkyvyys riippuu pelaajan moodista (esim. katot eivät ole näkyvissä rts-moodissa)
    public GameObject spaceBarInfo;                 //Välilyönti Ui-image

    public GameObject[] startObjectsToPutActive;    //Ojektit ja sen lapset päälle , jotka laitetaan päälle pelin alussa.
    public GameObject[] roofObjects;                //Objektit arraysta objectsToDo[], jotka pitää olla pois päältä alussa

    public RoomList roomList;                       //Referenssi 

    float deltaTime;



    //Heti pelin alussa suorittaa corutiinin DlayOnStartActivation(), joka laittaa listalla olevat objektit ja sen lapset päälle 
    //Jokaisen objektin kohdalla katsotaan, onko fps korkeampi, kuin 30. Laitetaan objekti päälle vasta, kun fps on tarpeeksi korkea
    //Auttaa pelin alussa olevaa lag spikea
    //Sen jälkeen, kun kaikki objektit on päällä, suoritetaan RoomList.CalculateObjects(), joka kadottaa objektit, joita ei tarvita päälle
    private void Awake()
    {
        StartCoroutine(DelayOnStartActivation());
    }
    IEnumerator DelayOnStartActivation()
    {
        for (int i = 0; i < roofObjects.Length; i++)
        {
            if (roofObjects[i].activeSelf)
            {
                roofObjects[i].SetActive(false);
            }
        }

        for (int i = 0; i < startObjectsToPutActive.Length; i++)
        {
            while (!startObjectsToPutActive[i].activeSelf)
            {
                yield return new WaitForEndOfFrame();
                if (ReturnFps() > 15f)
                {
                    startObjectsToPutActive[i].SetActive(true);
                }
            }
            for (int v = 0; v < startObjectsToPutActive[i].transform.childCount; v++)
            {
                while (!startObjectsToPutActive[i].transform.GetChild(v).gameObject.activeSelf)
                {
                    yield return new WaitForEndOfFrame();
                    if (ReturnFps() > 15f)
                    {
                        startObjectsToPutActive[i].transform.GetChild(v).gameObject.SetActive(true);
                    }
                }
            }
        }
        if (roomList)
            roomList.CalculateObjects();
    }
    //Katsoo fps
    float ReturnFps()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        return Mathf.Ceil(fps);
    }

    //Käy läpi listan, joka sisältää objekteja, joiden näkyvyys riippuu pelaajan moodista (esim. katot eivät ole näkyvissä rts-moodissa)
    //ja togglee ne (eli false = true ja true = false)
    public void GoThroughAll()
    {
        for (int i = 0; i < objectsToDo.Length; i++)
        {
            if (objectsToDo[i])
            {
                objectsToDo[i].SetActive(!objectsToDo[i].activeSelf);
            }
        }
    }

    //Kustuttaessa kadottaa tai tuo välilyöntikuvakkeen
    public void HideOrRevealInfo(bool value)
    {
        spaceBarInfo.SetActive(!value);
    }

}
