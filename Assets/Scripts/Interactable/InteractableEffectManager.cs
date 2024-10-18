using UnityEngine;
using System.Collections;

//Scripti, joka vastaaa kaikista objekteista, jonka kanssa voi käydä vuorovaikutusta 
//Esimerkiksi tuoli, jolle voi istua tai taulu, jonka voi avata ruudulle
public class InteractableEffectManager : MonoBehaviour
{

    public RTS_player player;               //Referenssi pelaajan scriptiin, jota kautta suoritetaan eri asiat
    public AloitusScripti startScript;      //Viittaus tutoriaaliin 
    public PaintingScript paintings;        //Viittaus taulujen näyttö scriptiin (Helvetillinen!)
    public Infotable infoTable;             //Viittaus infotaulun scriptiin

    public Interactable focus;              //Nykyinen kohde
    public string currentName;              //Nykyisen kohteen nimi

    public GameObject aave;                 //Nyaa salaisuus (^.~)7


    //Interactable-kohteen suoritus
    //Verrataan currentName ja suoritetaan halutut metodit
    //Kannattaa katsoa itse Switch lausetta, jos haluat tietää mitä yksittäiset objektit tasan tarkkaan tekevät
    public void DoTheThing(AnimationEvents eventScript, int soundIndex, Interactable newFocus)
    {
        focus = newFocus;
        currentName = newFocus.objectType;

        switch (currentName)
        {
            //Tuolit
            //kun käytetään, istumis prosessi aloitetaan
            //jos hahmo istuu jo, nousee hahmo ylös
            case "sohvatuoli":
            case "tuoli":
            case "nojatuoli":
            case "konetuoli":
            case "konetuoliextra":
                if (!eventScript.isSitting)
                {
                    if (!currentName.Contains("kone"))
                        eventScript.Sit(focus.transform.GetChild(0), false);
                    else
                        eventScript.Sit(focus.transform.GetChild(0), true);

                    focus.ToggleOutline(false);

                    if (currentName.Contains("extra"))
                    {
                        aave.SetActive(true);
                    }
                }
                else
                {
                    eventScript.anim.ResetTrigger("Sit");
                    eventScript.anim.ResetTrigger("SitOffice");
                    eventScript.anim.SetTrigger("GetUp");

                    if (eventScript.aRealDude)
                    {
                        eventScript.player.ToggleLockMode(true);
                    }
                }
                break;

            //Ovikello
            //Soittaa äänen ja laittaa animaation pyörimään
            //Animaatio event suorittaa tutoriaalissa kohdan
            case "ovikello":
                PersistentManager.Instance.aManager.Play("ovikello", focus.gameObject, soundIndex);
                focus.GetComponent<Animator>().SetBool("bell", true);
                focus.ToggleOutline(false);
                startScript.ToggleHudButtons(true);
                break;

            //Taulu
            //Tuo taulun esiin näytölle ja estää monta eri asiaa tapahtumasta, kuten lukitsee kontrollit
            case "taulu":
                if (!paintings.isOnScreen)
                {
                    paintings.ShowImageOnScreen(focus.GetComponent<Renderer>().material.name);
                    focus.ToggleOutline(false);
                    player.ToggleLockMode(true);
                    player.StopMovement(true);
                    player.ToggleDisable(true, 0);
                    player.CamMove.isFpsCameraMoveAllowed = false;
                }
                break;
            //Ilmoitustaulu
            //Lähes sama kuin taulu, mutta täyttää koko ruudun
            //Suorittaa lähes samat lukitukset kuin taulun kohdalla, mutta itse skriptissä
            case "ilmoitustaulu":
                infoTable.ShowInfoTable();
                break;

            //Ovien avaaminen. Avaa kaapin ovi skriptillä
            case "kaapinOvi":
            case "jaakaapinOvi":
                focus.GetComponent<DoorScript>().ToggleDoor();
                startScript.ToggleHudButtons(true);
                break;

            //Moccamster. Hahmo juo
            case "kahvi":
                eventScript.Consume("Kuppi");
                PersistentManager.Instance.aManager.Play("kahvi", eventScript.gameObject, soundIndex);
                break;

            //Syö omenan tai makkaran
            case "keksi":
                eventScript.Consume("Omena");
                PersistentManager.Instance.aManager.Play("ruoka", eventScript.gameObject, soundIndex);
                break;
            case "grilli":
                eventScript.Consume("Makkara");
                PersistentManager.Instance.aManager.Play("ruoka", eventScript.gameObject, soundIndex);
                break;

            //Radio. Vaihtaa musiikkia
            case "radio":
                focus.GetComponent<Radio>().PlayNextClip();
                startScript.ToggleHudButtons(true);
                break;

            //Aloittaa 3D-tulostamisen
            case "tulostin":
                focus.GetComponent<PrinterScript>().StartPrinting();
                startScript.ToggleHudButtons(true);
                break;

            //Tuhlaa vettä SinkScript.ActivateSink()-metodissa
            case "hana":
                focus.GetComponent<SinkScript>().ActivateSink();
                startScript.ToggleHudButtons(true);

                if (player.isFPS && eventScript.aRealDude)
                {
                    StartCoroutine(ResetCameraRot());
                }
                break;

            //Aloittaa keskustelun AIScript.TalkingTo()-metodissa
            case "hahmo":
                focus.GetComponent<AIScript>().TalkingTo(player.transform);

                if (!eventScript.isSitting && !player.isFPS && !eventScript.willISit)
                {
                    eventScript.anim.Play("Armature|Wave");
                    eventScript.player.transform.LookAt(focus.transform);
                }

                if (!focus.GetComponent<AnimationEvents>().isSitting)
                {
                    focus.GetComponent<AnimationEvents>().anim.Play("Armature|Wave");
                }

                player.ToggleConversation(true);
                startScript.ToggleHudButtons(true);
                break;

            //Vetää vessan eli soittaa äänen
            case "vessa":
                PersistentManager.Instance.aManager.Play("vessa", focus.gameObject, soundIndex);
                startScript.ToggleHudButtons(true);
                break;

            //Vaihtaa hatun
            case "henkari":
                startScript.ToggleHudButtons(true);

                if (!eventScript.isSitting)
                {
                    eventScript.anim.Play("Armature|ChangeCostume");

                    if (player.isFPS && eventScript.aRealDude)
                    {
                        StartCoroutine(ResetCameraRot());
                        player.ToggleDisable(true, 1);
                        player.ToggleLockMode(true);
                    }
                }
                break;

            //Suorittaa salaisuuden
            case "aave":
                focus.gameObject.SetActive(false);
                break;

            default:
                Debug.Log("En ymmärtänyt " + currentName);
                break;
        }

        //Jos on pelaaja
        if (eventScript.aRealDude)
        {
            PersistentManager.Instance.curManager.DefaultMouse();
            PersistentManager.Instance.missionManager.CheckForMissions(1, currentName);

            if (!currentName.Contains("tuoli"))
            {
                PersistentManager.Instance.pManager.tempObject.KillMe();
            }
        }
    }

    //Metodi, joka resetoi kuvakulman fps, ettei kuvakulma jää jumiin
    IEnumerator ResetCameraRot()
    {
        yield return new WaitForSecondsRealtime(3f);
        player.CamMove.resetRot = true;
    }

}
