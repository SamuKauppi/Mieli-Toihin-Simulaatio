using System.Collections;
using UnityEngine;

//Scripti objecteille, jolla halutaan tehdä jotain erityistä (esim. tuoli, jolle voi istua tai vessa, jonka vetää)
//Interactable objetkin pitää olla:
//Interactable tag ja -layer, Collider, Tämä skripti, Outline skripti
//Sitten se tunnistetaan muissa skripteissä ja voidaan suorittaa jotain erityistä
public class Interactable : MonoBehaviour
{

    public string objectType;                       //Objektin tyyppi (määritetään editorissa)
    public int soundIndex;                          //Ääni-indkesi (jos tällä kohteella on ääniefekti, tämän pitää vastata huoneen ääni-indeksiä)
    public RTS_player playerScript;                 //Viittaus pelaajan
    public InteractableEffectManager iManager;      //Viittaus manageriin, jossa suoritaan asiat

    public bool dontDisableOutlineAtStart;          //Pitää outlinen päällä (false poistaa, true ei)

    Outline outline;                                //Oma outline scripti

    public bool stopMovementOnActivation;           //Käveleekö hahmo asian luoke ennen kuin se käyttää sen vai ei?
    public bool isBeingUsed;                        //Onko tämä kohde käytössä (estää, ettei tuolille istu kahta hahmoa samaan aikaan.

    //Startissa poistetaan outline skripti pois päältä
    private void Start()
    {
        outline = GetComponent<Outline>();
        if (!dontDisableOutlineAtStart)
            outline.enabled = false;
    }

    //Kutsuttaessa aktivoidaan tämä kohde
    //Lähetetään manageriin tieto, että tämä kohde aktivoidaan
    //Parametri int i kertoo onko suorittaja pelaaja vai ei ja jos on, niin suoritetaan StopMovement()
    public void ActivateTheThing(AnimationEvents a, bool i)
    {
        if (i)
            StartCoroutine(StopMovement());

        iManager.DoTheThing(a, soundIndex, this);
        a.currentFocus = null;
    }
    //Pysäyttää hahmon liikkumisen pienellä viivellä (vain pelaaja suorittaa tämän)
    IEnumerator StopMovement()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        if (stopMovementOnActivation)
        {
            playerScript.StopMovement(false);
        }
    }
    //Laitetaanko Ouline päälle vai ei
    public void ToggleOutline(bool b)
    {
        if (outline)
            outline.enabled = b;
    }

}
