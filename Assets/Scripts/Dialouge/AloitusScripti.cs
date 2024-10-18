using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Scripti joka hoitaa tutoriaalin ja hallitsee menu nappien toimivuuden
//Beware of spagetti
public class AloitusScripti : MonoBehaviour
{
    public float timer;                                     //Timer kaikelle tutoriaalissa
    public int stepCounter = 0;                             //Numero, joka seuraa missä vaiheessa mennään tutoriaalissa
    public int stepSkipper;                                 //muistaa stepcounterin ja jos stepcounter kasvaa dialogin aikana, verrataan tätä ja ei kasvateta stepcounteria liikaa
    public bool stopTimer;                                  //Pysäyttää ajastimen ja estää liian monen keskustelun päällekäisen käynnistymisen
    public bool skipTimer;                                  //Kun true, updaten sisällä voidaan suorittaa, mutta ajastin ei liiku
    public int previousCounter;                             //pitää muistissa stepcounterin, kun pelaaja aloittaa ohje dialogin
    
    List<string> questionsTexts = new List<string>();       //Lista mahdollisista vastauksista, jos halutaan kysyä kysymys pelaajalta

    Vector3 playerpos;                                      //Ottaa pelaajan aloitus position ja käyttää vertaa sitä. 
                                                            //Jos pelaaja on liikkunut jatkuu koodi, muuten toistaa

    public RTS_player player;                               //Referenssi pelaajan scriptiin
    public CameraMoveRTS camMove;                           //Referenssi pelaajan kameran scriptiin

    public DialougeTrigger[] dialouges;                     //Kaikki tutoriaali dialogit paitsi alkukysymys
    public DialougeTrigger startQuestion;                   //Kysyy pelaajalta haluaako pelata tutoriaalin

    public DoorScript[] Doors;                              //Referenssi oviScripetihin, jotka ovat lukittuna pelin alussa ja estävät pelaajaa menemään sisälle
    public Doorbell doorBell;                               //Referenssi ovikelloScriptiin, joka vastaa ovikellosta
    public QuestionScript questions;                        //Referenssi kysymysScriptiin, joka luo napit kysymyksiin

    public GameObject aiPlayers;                            //Ai pelaajat (laitetaan päälle, kun pelaaja suorittaa tutoriaalin)
    public AIScript ukkoHahmo;                              //Ai pelaaja, joka laitetaan päälle oven avautuessa (Tommi Tutoriaali)

    public GameObject iButtonOpen;                          //Infobutton, johon lisätään ominaisuus millä se laittaa Menubuttonin päälle ja pois'
    public GameObject iButtonClose;                         //Infobutton, johon lisätään ominaisuus liittyen dialogimanageriin
    public GameObject mButtonClose;                         //Menubutton, joka sulkee menun
    public GameObject mButtonOpen;                          //Menubutton, joka avaa menun. Lisätään myös ominaisuus liittyen dialogimanageriin
    public GameObject buttonBackground;                     //Ylläolevien nappien tausta

    public Transform walkPoint;                             //Piste johon pelaaja kävelee tutoriaalin aikana (kun ovi avautuu)

    //Estää inputin pelaajalta heti pelin alussa
    //Luo alkukysymyksen
    //lisää logiikkaa menu nappeihin (iButtonClose/Open ja mButtonOpen)
    void Start()
    {
        player.ToggleDisable(true, 0);
        player.ToggleLockMode(true);
        startQuestion.TriggerDialouge();
        stopTimer = true;
        player.ToggleQuestion(true);

        questionsTexts.Add("Ohjattu kierros");
        questionsTexts.Add("Katso esittely");
        questionsTexts.Add("Vapaa tutkiminen");
        questions.SpawnAQuestion(questionsTexts);

        camMove.isFpsCameraMoveAllowed = false;

        //Lisätään info ja menu nappeihin logiikkaa, koska dialoguemanager on DoNotDestroyOnLoad
        iButtonClose.GetComponent<Button>().onClick.AddListener(() => PersistentManager.Instance.dManager.DisplayNextSentence());
        iButtonClose.GetComponent<Button>().onClick.AddListener(() => player.ToggleQuestion(false)); //Pitää lisätä jälkeen. Muuten isInQuestion menee pois ennen kuin dialogi kadotetaan
        mButtonOpen.GetComponent<Button>().onClick.AddListener(() => PersistentManager.Instance.dManager.EndDialouge());

        mButtonOpen.gameObject.SetActive(false);
        iButtonOpen.gameObject.SetActive(false);
    }

    //Updaten sisällä tarkkaillaan missä vaiheessa tutoriaalia pelaaja menee ja määrittää asioita sen mukaan
    //Seuraava askel updaten sisällä voi tapahtua, jos ajastin on päällä, mutta skipTimer boolin kanssa voi suorittaa seuraavia askeleita ilman ajastinta
    void Update()
    {
        if (!stopTimer || !skipTimer)
        {
            if (!stopTimer)
            {
                timer += Time.deltaTime;
            }

            //Alku puhe 3s jälkeen
            if (timer > 3f && stepCounter == 0)
            {
                NextStep();
            }
            //Muistutus 15s jälkeen, että pitää painaa väli
            if (timer > 15f && stepCounter == 1)
            {
                NextStep();
            }
            //Pelaaja alussa painoi väli ja on RTS moodissa
            if (!player.isFPS && stepCounter == 1)
            {
                NextStepWithParameter(2);
            }

            //Pelaajaa kerrotaan liikkumaan. Jos pelaaja ei ole liikkunut muistuttaa peli 15s jälkeen tästä
            if (timer > 15f && stepCounter == 4)
            {
                NextStep();
            }
            //Pelaaja liikkui ja kerrotaan menemään etuovelle
            if (playerpos != player.transform.position && stepCounter == 4)
            {
                NextStepWithParameter(5);
            }
            //muistutus pelaajalle että etuvoi on paikka minne mennä
            if (timer > 35f && stepCounter == 6)
            {
                NextStep();
            }

            //Pelaaja meni fps moodiin etuoven edessä
            if (stepCounter == 9 && player.isFPS)
            {
                NextStep();
            }
            //Muistuttaa pelaajaa ovikellosta
            if (stepCounter == 10 && timer > 25f)
            {
                NextStep();
            }
            //Pelaaja on soittanut ovikelloa ja 5s ovi avautuu
            if (stepCounter == 14 && timer > 5f)
            {
                NextStep();
            }
        }
    }

    //Itse dialoejen suoritus tutoriaalissa
    //Katsoo Switch lauseen sisällä, vertaillen stepCounter numeroa ja laittaa oikean dialogin pyörimään
    //Ennen kuin aloittaa dialogin, lukitsee pelaajan näppäimet, kertoo pelaaja scriptiin, 
    //että on keskustelussa, pysäyttää ajastimen ja poistaa RTS liikkumis indikaattorin
    //Huom! Suorittaa asioita itse Switch lauseen sisällä, joten katso tätä metodia, jos löytyy ongelmia tutoriaalissa
    public void NextStep()
    {

        iButtonOpen.SetActive(false);
        mButtonOpen.SetActive(false);

        stepSkipper = stepCounter;
        stopTimer = true;
        skipTimer = stopTimer;
        player.ToggleDisable(true, 0);
        player.ToggleLockMode(true);
        if (PersistentManager.Instance.pManager.tempObject && stepCounter != 5)
        {
            PersistentManager.Instance.pManager.tempObject.KillMe(); //Kadotetaan mahdollinen liikkumis indikaattori
        }
        timer = 0f;
        switch (stepCounter)
        {
            //Ensimmäinen puhe (pelin aloitus puhe)
            case 0:
                dialouges[0].TriggerDialouge();
                break;
            //Toinen puhe (Ensimmäisen puheen toisto jos on hankaluuksia)
            case 1:
                dialouges[1].TriggerDialouge();
                break;
            //Kolmas puhe (laittaa hahmon heiluttamaan kameraa päin)
            case 2:
                dialouges[2].TriggerDialouge();
                break;
            //Neljäs puhe (kertoo liikkumisesta)
            case 3:
                dialouges[3].TriggerDialouge();
                break;
            //Viides puhe (neljännen puheen toisto jos on hankaluuksia)
            case 4:
                dialouges[4].TriggerDialouge();
                break;
            //Kuudes puhe (pelaajan pitää mennä etuovelle)
            case 5:
                dialouges[5].TriggerDialouge();
                break;
            //Seitsemäs puhe (kuudennen puheen toisto jos on hankaluuksia)
            case 6:
                dialouges[6].TriggerDialouge();
                break;
            //Kahdeksas puhe (ohje joka laukaistaan i napilla, kun puhe ei ole käynnissä. Yleinen ohje)
            case 7:
                dialouges[7].TriggerDialouge();
                PersistentManager.Instance.missionManager.CheckForMissions(1, "info"); //Tehtävän suoritus
                break;
            //Yhdeksäs puhe (pelaaja on etuovella ja pitää mennä FPS moodin)
            //Pysäyttää hahmon ja laittaa sen katsomaan kohti ovikelloa
            case 8:
                dialouges[8].TriggerDialouge();
                player.StopMovement(true);
                if (!player.isFPS)
                {
                    player.transform.LookAt(doorBell.transform.position);
                }
                break;
            //Kymmenes puhe (pelaaja on etuovella ja menee FPS moodiin)
            case 9:
                dialouges[9].TriggerDialouge();
                break;
            //Yhdestoista puhe (kymmenennen puheen toisto jos on hankaluuksia)
            case 10:
                dialouges[10].TriggerDialouge();
                break;
            //Kahdestoista puhe (Kahdeksannen puheen failsafe eli jos pelaaja lähtee ovelta ennen kuin menee fps moodiin)
            case 11:
                dialouges[11].TriggerDialouge();
                stepCounter = 5;
                stepSkipper = stepCounter;
                break;
            //Kolmastoista puhe (pelaaja menee takaovelle ennen kuin menee sisään. Toistaa kymmennenen puheen)
            case 12:
                dialouges[12].TriggerDialouge();
                stepCounter = 5;
                break;
            //Neljästoista puhe (Ovikello on soinut ja pelaajan pitää odottaa 5s)
            //Liikuttaa pelaajan oven luokse
            case 13:
                dialouges[13].TriggerDialouge();
                player.characterAnimScript.MoveTo(walkPoint.position);
                break;
            //Viidestoista puhe (Ovi avautuu ja prefab hahmo tulee ja puhuu pelaajalle)
            //Pistää pelaajan ja kameran katsomaan ovea kohti
            case 14:
                dialouges[14].TriggerDialouge();
                player.transform.LookAt(Doors[0].transform, Vector3.up);
                camMove.resetRot = true;
                player.CamMove.playerSubBody.LookAt(Doors[0].transform, Vector3.up);
                Doors[0].UnlockDoor(true);
                ukkoHahmo.gameObject.SetActive(true);
                ukkoHahmo.transform.LookAt(player.transform);
                ukkoHahmo.animEvent.anim.Play("Armature|Wave");
                break;

            default:
                stopTimer = false;
                skipTimer = stopTimer;
                player.ToggleDisable(false, 0);
                player.ToggleLockMode(false);
                iButtonOpen.SetActive(true);
                mButtonOpen.SetActive(true);
                buttonBackground.SetActive(true);
                break;
        }
    }

    //Metodi, jonka avulla voidaan helposti aktivoida mikä tahansa vaihe tutoriaalissa
    //Käytä tätä, ellei haluta suoraan kutsua NextStep();
    public void NextStepWithParameter(int stepToActivate)
    {
        if (stepToActivate == 7)
        {
            previousCounter = stepCounter;
            stepToActivate = 7;
            player.StopMovement(false);
            if (PersistentManager.Instance.pManager.tempObject)
            {
                PersistentManager.Instance.pManager.tempObject.KillMe(); //Kadotetaan mahdollinen liikkumis indikaattori
            }
        }

        stepCounter = stepToActivate;
        stepSkipper = stepCounter;

        NextStep();
    }


    //Metodi joka kutsutaan aina kun mikään dialogi loppuu ja ollaan tutoriaalissa
    //Switch lauseessa on poikkeustapaukset
    //Lyhykäisyydessä avaa kontrollit, tarkastelee menu nappeja ja kasvattaa stepcounterin oikeaan arvoon
    public void AdvanceStep()
    {
        switch (stepCounter)
        {
            //Toisto, jos pelaajalla on hankaluuksia 
            case 1:
            case 4:
            case 6:
            case 10:
            case 16:
                if (stepCounter != 1)   //Jos vaihe on 1, pitää liikkuminen rajoittaa
                {
                    player.ToggleDisable(false, 0);
                }

                if (stepCounter != 4)   //Jos on vaihe 4, niin moodi pitää lukita
                {
                    player.ToggleLockMode(false);
                }
                stopTimer = false;
                stepSkipper--;
                break;
            //Kun pelaaja lopettaa kolmannen puheen laitetaan pelaaja katsomaan kameraa päin ja vilkutus animaatio
            case 2:
                player.characterAnim.Play("Armature|Wave");
                LeanTween.rotate(player.gameObject, new Vector3(player.cam.transform.position.x, 0f, player.cam.transform.position.z), 0.5f);
                playerpos = player.transform.position;
                break;
            //Pelaajalle kerrotaan liikkumisesta ja moodi pidetään lukittuna
            case 3:
                player.ToggleDisable(false, 0);
                stopTimer = false;
                break;
            //Kahdeksas puhe on ohjeen pyytäminen ja sen voi kustua monessa eri kohdassa tutoriaalia
            //Koodi muistaa edellisen askeleen ja määrittää sen takaisin tässä (stepCounter = previousCounter - 1).
            // -1 on koska sitä kasvatetaan yhdellä myöhemmin
            case 7:
                stepCounter = previousCounter - 1;
                stepSkipper = stepCounter;
                player.ToggleDisable(false, 0);
                player.ToggleLockMode(false);
                stopTimer = false;
                break;
            //Pelaaja on soittanut ovikelloa ja pitää odottaa 5s kontrollit lukittuna
            case 14:
                stopTimer = false;
                break;
            //Viidestoista puheen päätyttyä pitää kutsua komentoa, jolla prefab hahmo liikkuu pois näkymästä
            //if lause varoo tilannetta jolloin pelaaja kerkeää lopettaa keskustelun ennen kuin
            //hahmo edes luodaan
            case 15:
                CompleteTutorial();
                break;

          
            //Kaikissa muissa tapauksissa pitää poistaa näppäinten lukitus ja aloittaa ajastin (case 2: on ainoa paikka missä tätä ei saa tehdä)
            //Tämän vaiheen pitäisi tulla vian jos pelaaja on suorittanut tutoriaalin

            default:
                player.ToggleDisable(false, 0);
                player.ToggleLockMode(false);
                stopTimer = false;
                break;
        }

        //katsotaan mitkä hud napit saa näyttää (vain tutoriaalissa, muuten käyttää metodia "ToggleHudButtons(bool)")
        if (stepCounter >= 5 && !player.disableControls && !player.characterAnimScript.willISit)
        {
            iButtonOpen.SetActive(true);
            buttonBackground.SetActive(true);
            iButtonOpen.GetComponent<Button>().enabled = true;
            if (stepCounter > 24)
            {
                if (!mButtonClose.activeSelf)
                {
                    mButtonOpen.SetActive(true);
                    mButtonOpen.GetComponent<Button>().enabled = true;
                }
                else
                {
                    player.ToggleDisable(true, 0);
                    player.ToggleLockMode(true);
                }
            }
        }

        //Jotkin dialogit kasvattavat stepcounteria keskustelun aikana
        //Jotta stepCounter ei kasva liikaa, niin verrataan sitä stepSkipperin
        //skipper = stepcounter jo ennen kuin keskustelu alkaa ja jos stepcounter kasvaa
        //Niin stepcounteria ei kasvateta skipperin yli
        if (stepSkipper >= stepCounter)
        {
            stepCounter++;
            previousCounter = 0;
        }
        stepSkipper = stepCounter;
        skipTimer = stopTimer;
    }

    //Hallitsee hud nappeja (menu ja info)
    //Metdoin avulla tietää mistä kustu päälle tai pois lähetettiin
    public void ToggleHudButtons(bool value)
    {
        if (stepCounter >= 15)
        {
            mButtonOpen.GetComponent<Button>().enabled = value;
            iButtonOpen.GetComponent<Button>().enabled = value;
        }

        if (value)
        {
            if (stepCounter >= 5)
            {
                buttonBackground.SetActive(true);
                iButtonOpen.SetActive(true);
                iButtonClose.SetActive(false);
                if (stepCounter > 24)
                {
                    if (!mButtonClose.activeSelf)
                    {
                        mButtonOpen.SetActive(true);
                        mButtonClose.SetActive(false);
                    }
                }
            }
        }
        else
        {
            mButtonClose.SetActive(false);
            iButtonClose.SetActive(false);
        }
    }



    //Aloittaa tutoriaalin
    public void StartTuotorial()
    {
        stopTimer = false;
        PersistentManager.Instance.dManager.textAnim.SetBool("IsOpen", false);
        camMove.isFpsCameraMoveAllowed = true;
        player.isInQuestion = false;
    }
    //Ohittaa tutoriaalin alkukysymyksessä
    public void SkipTutorial()
    {
        PersistentManager.Instance.dManager.DisplayNextSentence();
        camMove.isFpsCameraMoveAllowed = true;
        player.ToggleQuestion(false);
        ukkoHahmo.GetComponent<Outline>().enabled = false;

        //Tommi Tutoriaali tuodaan näkyviin ja menee tuolille istumaan
        //Tehdään kun skipataan tutoriaali
        ukkoHahmo.walkType = 4;
        ukkoHahmo.gameObject.SetActive(true);

        EndTutorial();
    }
    //Suorittaa tutoriaalin normaalisti
    void CompleteTutorial()
    {
        stopTimer = true;
        EndTutorial();
    }

    //Lopettaa tutoriaalin
    //Suoritetaan aina tutoriaalin loppuessa, ohittettiin tai ei
    //Avaa kontrollit ja ovet, jotka on lukittu
    //Lisää menu nappeihin kaiken tarvittavan logiikan ja laittaa tietokonepelaajat päälle EnableAiPlayer()-metodissa
    void EndTutorial()
    {
        //Laitetaan stepcounter lukuun, jossa se ei häiritse
        stepCounter = 1000;
        //Avataan lukitut ovet
        for (int i = 0; i < Doors.Length; i++)
        {
            Doors[i].UnlockDoor(true);
        }
        //Avataan pelaajan kontrollit
        player.ToggleDisable(false, 0);
        player.ToggleLockMode(false);

        //Määritetään napit
        mButtonOpen.SetActive(true);
        iButtonOpen.SetActive(true);
        iButtonClose.SetActive(false);
        mButtonClose.SetActive(false);
        buttonBackground.SetActive(true);
        iButtonOpen.GetComponent<Button>().onClick.AddListener(() => mButtonOpen.SetActive(true));
        iButtonOpen.GetComponent<Button>().onClick.AddListener(() => mButtonClose.SetActive(false));
        mButtonOpen.GetComponent<Button>().enabled = true;
        iButtonOpen.GetComponent<Button>().enabled = true;

        //Tehtävän suoritus
        PersistentManager.Instance.missionManager.CheckForMissions(1, "tutorial");
        player.ToggleFPS();
        player.MouseHeld = 0;
        aiPlayers.SetActive(true);
        gameObject.SetActive(false);
    }
    //


    //Pelaaja saapuu ulko-ovelle soittamaan ovikelloa
    //Kun pelaaja on oikeassa vaiheessa tutoriaalissa (stepCounter < 13) ja koskee ulkooven lähellä olevaa hitboxia)
    //Peli huomaa, että pelaaja on oven lähellä
    private void OnTriggerEnter(Collider other)
    {
        if (stepCounter < 13 && stepCounter != 9)
        {
            NextStepWithParameter(8);
        }
    }
    //Jos pelaaja lähtee hitboxista pois ennenkuin saa vaiheen valmiiksi
    //Peli huomauttaa tästä
    private void OnTriggerExit(Collider other)
    {
        if (stepCounter < 13 && stepCounter != 8)      //Kun pelaaja on soittaunut ovikelloa tätä kohtaa ei enään tarkkailla. Stepcounter =! 8 estää bugin, jossa hahmo poistuu ja osuu samaan ai´kaan
        {
            NextStepWithParameter(11);
        }
    }
}
