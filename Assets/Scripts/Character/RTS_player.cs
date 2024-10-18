using UnityEngine;

//Pää scripti joka on pelaajalla
//Suorittaa raycastit, kontrollien lukitsemisen ja RTS/FPS moodien välisen hallinnan
//Look at your own risk
public class RTS_player : MonoBehaviour
{

    DialougeManager dManager;                           //Referenssi dialogi manageriin
    ParticleEfeectManager pManager;                     //Referenssi partikkeli manageriin
    CursorScript curManager;                            //Referenssi kursori manageriin
   [SerializeField] ModeChanger mManager;               //Referenssi ModeChanger manageriin

    public CameraMoveRTS CamMove;                       //Referenssi kameran scriptiin
    public Camera cam;                                  //Referenssi kameraan

    public AloitusScripti startScript;                  //Referenssi tutoriaali sciptiin

    public Animator characterAnim;                      //Hahmo objektin animaattori
    public AnimationEvents characterAnimScript;         //Referenssi character animaattori-skriptiin

    public Transform handBone;                          //Hahmon käsi transform, johon kutsutaan kuppi juomista varten
    public int hasADrink;                               //Onko hahmolla juoma

    public LayerMask ignoreInMovement;                  //Layer maskit, mitä EI käytetä liikumisen laskemiseen
    public LayerMask ignoreInInteraction;               //Layer maskit, mitä EI käytetä, kun etsitään interactableja

    public LayerMask interactableLayerFPS;              //Kun halutaan liikkua, katsotaan ensin onko interactable edessä
    public LayerMask interactableLayerRTS;              //interactableLayer saa arvonsa perustuen mikä moodi on päällä (interactableLayerFPS/RTS)
    LayerMask interactableLayer;                        //Näin pelaaja ei liiku seinien läpi, kun on fps, mutta rts voi

    Collider tempCollider;                              //Tilapäinen collider jota käytetään interactable muutujan katselemiseen

    bool MouseIsActive;                                 //Katsoo milloin hiiren vasen on klikattu (koska raycast on fixed se ei ota klikkausta sulavasti vastaan)
    public float MouseHeld;                             //Katsoo kuinka kauan hiiri on ollut pohjassa

    public bool hitsTheGround;                          //Katsoo osuuko hiiri maahan
    public bool isOnHudButton;                          //katsoo onko hiiri ui elementin päällä
    public string nameOfHudElement;                     //Jos hiiri on hudin päällä, nimi tallenetaan tähän. jos false, niin on tyhjä
    public float isOverInteractable;                    //Katsoo kauan hiiri on ollut interactablen päällä. 
    public bool isFPS;                                  //Katsoo onko pelaaja FPS vai RTS moodissa
    public bool isInConversation;                       //Katsoo onko pelaaja keskustelussa vai ei
    public bool isInQuestion;                           //Onko kysymyksessä
    public bool disableControls;                        //Estää kaiken inputin paitsi kameran liikkeen pelaajalta
    public int extraLock_m;                             //Extra lukitus vaihtoehto. Esimerkiksi jos menu avataan lukitusta ei saa avata muualta
    public bool lockMode;                               //Estää pelaajan vaihtamasta kamerakulmaa
    public bool controlsWillRemainLocked;               //Jos kontrollit halutaan pitää lukittuna sen jälkeen, kun kamera lopettaa liikkumisen
                                                        //True, kun kamera liikkuu ja disablecontrols lukitaan liikkeen aikana
                                                        //False, kun kamera lopettaa liikkumisen
    public float rayDistance;                           //Kaikkien Raycastien maksimi pituus
    float elapsed;                                      //Paljon aikaa on mennyt edellisestä klikkauksesta

    //Haetaan manager muuttujille alkuarvot, jotta ei tarvitse hakea updaten sisällä
    private void Start()
    {
        dManager = PersistentManager.Instance.dManager;
        curManager = PersistentManager.Instance.curManager;
        pManager = PersistentManager.Instance.pManager;
    }

    //Fixed Update Raycastille
    //Tekee 2 raycastia. Toinen liikkumiselle ja toinen Interacatble-kohteen käyttöön
    //Lyhykäisyydessä tämä spagetti katsoo liikutaanko vai käytetetäänkö Interactable-kohde?
    private void FixedUpdate()
    {
        //RayCastaaminen tehdään kontrollit avattuna
        if (!disableControls)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            //Tehdään ray casti liikkumiselle
            //Ohitetaan liikkumista haittaavat layerit (vaihtuu fps/rts välillä)

            //Liikkuminen
            //Katsotaan osuuko raycasti maa layerilla olevaan objektiin (katsotaan yli kaikki ignoreMovement-muutujuan layerit)
            if (Physics.Raycast(ray, out hit, rayDistance, ~ignoreInMovement) && !curManager.cameraIsRotating && !isOnHudButton)
            {
                //osuu maahan
                if (hit.collider.CompareTag("Maa"))
                {
                    //Näytetään ympyrä indikaattori, jos hiiri ei osu interactableen (ei liikkumis, vaan voi liikkua)
                    if (!Physics.Raycast(ray, rayDistance, interactableLayer) && elapsed > 0.025f)
                    {
                        pManager.ShowCircleOnly(hit.point, true);

                        curManager.playerIsMoving = true;
                        hitsTheGround = true;

                        //Liikkuminen
                        //Klikkaus ja kontrollit ei lukittuna
                        if (MouseIsActive && !disableControls)
                        {
                            //Liikutaan ja kursori resetoidaan
                            characterAnimScript.MoveTo(hit.point);
                            elapsed = 0f;

                            curManager.playerIsMoving = false;
                            MouseIsActive = false;
                        }
                        if (MouseHeld >= 0.25f)
                            MouseHeld = 1;
                    }
                }
                else
                {
                    hitsTheGround = false;
                    //Ei osu maahan
                    //Kadotetaan ympyrä indikaattori (ei liikkumis vaan voi liikkua)
                    //Resetoidaan hiiren vasen
                    //Kursorin muutos
                    pManager.ShowCircleOnly(hit.point, false);
                    MouseIsActive = false;
                    curManager.playerIsMoving = false;
                }
            }
            else
            {
                hitsTheGround = false;
                curManager.playerIsMoving = false;
                pManager.ShowCircleOnly(Vector3.zero, false);
                MouseIsActive = false;
            }


            //Interactable-kohteen raycast
            //Tehdään raycasti, jossa katsotaan yli tietyt tasot (esim. ovien valtavat hitboxit)
            if (Physics.Raycast(ray, out hit, rayDistance, ~ignoreInInteraction))
            {
                //Verrataan onko hit = interactable?
                //Jos kohde ei ole interacatble
                //poistetaan mahdollisen edellisen kohteen outline ja resetoidaan kohteen muuttujat AnimationEvents-skriptissä
                if (!hit.collider.CompareTag("interactable"))
                {
                    if (characterAnimScript.currentFocus != null && hit.collider != tempCollider)
                    {
                        characterAnimScript.DetermineFocus(null);
                        tempCollider = null;
                    }
                    curManager.mouseIsOver = false;
                    isOverInteractable = 0;
                }
                //Jos kohde on interactable ja ei ole hud napin päällä
                //Laitetaan kohteen outline ja muutujat AnimationsEvents-skriptissä päälle
                else if (!isOnHudButton)
                {
                    pManager.ShowCircleOnly(hit.point, false);

                    //Outline otetaan pois mahdollisesta edellisestä objektista
                    //Verrattaessa käyetään collider muuttujaa
                    if (characterAnimScript.currentFocus != null && hit.collider != tempCollider)
                    {
                        characterAnimScript.DetermineFocus(null);
                        tempCollider = null;
                    }

                    //currentInteractable sekä tempcollider päivitetään ja outline päälle
                    characterAnimScript.DetermineFocus(hit.collider.GetComponent<Interactable>());    //Käytetään outlinen muuttamiseen
                    tempCollider = hit.collider;                                                      //Käytetään vain colliderejen vertaamiseen
                    curManager.playerIsMoving = false;
                    curManager.mouseIsOver = true; //Kursorin muutos
                    isOverInteractable += Time.deltaTime;
                    MouseHeld = 0;
                }
            }
            //jos ray ei osu mihinkään, kohteen muuttujat restetoidaan
            else
            {
                pManager.ShowCircleOnly(hit.point, false);
                if (characterAnimScript.currentFocus != null)
                {
                    characterAnimScript.DetermineFocus(null);
                    tempCollider = null;
                }
                curManager.mouseIsOver = false; //Kursorin muutos
                curManager.playerIsMoving = false;
                isOverInteractable = 0;
            }
        } 
    }

    //Update
    //Kuvakulman vaihto välilyönnillä (ToggleFPS())
    //Enterillä seuraava teksti keskustelussa (dManager.DisplayNextSentence())
    //MouseLeft()
    void Update()
    {
        elapsed += Time.deltaTime;
        //Painamalla välilyöntiä pelaaja vaihtaa kameran peli näkymää
        // RTS/FPS
        if (Input.GetKeyDown(KeyCode.Space) && !lockMode)
        {
            ToggleFPS();
        }

        //Enter-näppäimeen liittyvää
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            //Jos on keskustelussa ja ei ole kysymyksessä
            //näyttää seuraavan lauseen
            if (isInConversation && !isInQuestion)
            {
                dManager.DisplayNextSentence();
            }

        }
        //Speed hack
        if (Input.GetKeyDown(KeyCode.Alpha9) && Input.GetKey(KeyCode.Alpha1))
        {
            characterAnimScript.agent.speed = 20f;
            characterAnimScript.agent.angularSpeed = 720f;
            characterAnimScript.agent.acceleration = 50f;

        }
        //Kun kontrollit on vapaat ja hiiri ei ole ui:n päällä, käytetään hiiren vasenta
        if (!disableControls && !isOnHudButton)
        {
            MouseLeft();
        }
    }


    //Hiiren vasempaan näppäimeen liittyvä koodi
    //Kun hiiren vasenta pitää pohjassa, niin "MouseHeld" alkaa kasvamaan
    //Kun MouseHeld on suurempi kuin 0.55, niin silloin tiedetään, että pelaaja haluaa kääntää kameraa fps-moodissa
    //Jos hiirestä päästää irti ennen 0.55, niin tiedetään, että klikkaus tapahtui ja hahmo liikkuu kohteeseen, jos hiiri osuu maahan
    //Jos klikkaus tapahtui ja hiiri on ollut kohteen päällä tarpeeksi kauan, käytetään kohde (vain fps moodissa)
    //Rts moodissa kuvakulma ei käänny, joten se ohitetaan. Liikkuminen ja asian käyttäminen riippuu siitä, onko hiiri kohteen päällä
    void MouseLeft()
    {
        //Kun hiirestä päästetään irti ja ei ole kohdetta
        if (Input.GetMouseButtonUp(0) && characterAnimScript.currentFocus == null)
        {
            //Jos hiiren näppäintä on pidetty pohjassa alle .25 sec
            //Rekiströidään se klikkaukseksi ja pelaaja liikkuu sinne missä hiiri on
            if (MouseHeld < 0.25f)
            {
                MouseIsActive = true;
            }
            //Resetoidaan hiiren ajastin
            MouseHeld = 0f;
        }

        //Kun hiirellä klikataan ja se on ollut yli X sec kohteen päällä
        //Aktivoidaan kohde
        if (isFPS ? isOverInteractable > 0.4f && MouseHeld < 0.25f : MouseHeld < 0.25f)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //Jos on kohde, niin aktivoidaan kohde
                if (characterAnimScript.currentFocus != null && elapsed > 0.05f)
                {
                    elapsed = 0f;
                    characterAnimScript.ActivateFocus(characterAnimScript.currentFocus.stopMovementOnActivation);
                    characterAnimScript.DetermineFocus(null);
                    MouseIsActive = false;
                    MouseHeld = 0f;
                    isOverInteractable = 0f;
                }
                //Jos pelaaja ei ole istumassa ja istuu, niin hahmo nousee ylös
                else if (!characterAnimScript.willISit && elapsed > 0.05f)
                {
                    if (characterAnimScript.isSitting && !isFPS)
                    {
                        characterAnim.ResetTrigger("Sit");
                        characterAnim.SetTrigger("GetUp");
                        ToggleLockMode(true);
                    }
                }
            }
        }

        //Niin kauan kuin hiiren vasen on pohjassa ja ei ole kohdetta
        if (Input.GetMouseButton(0) && characterAnimScript.currentFocus == null)
        {
            //Hiiren ajastin kasvaa, kun hiiri osuu maahan
            if (hitsTheGround)
            {
                MouseHeld += Time.deltaTime;
            }

            //Jos kameraa käännetään tai aikaa on mennyt yli X sec
            //Silloin tiedetään, että kameraa halutaan kääntää fps-moodissa. Rts-moodissa ei tehdä mitään
            else if (curManager.cameraIsRotating || MouseHeld >= 0.25f)
            {
                if (isFPS)
                {
                    MouseHeld = 1f;
                    curManager.playerIsMoving = false;
                    curManager.cameraIsRotating = true;
                }
                else
                {
                    MouseHeld = 0;
                }
            }
            //Muuten hiiren ajastin resetoidaan
            else
            {
                MouseHeld = 0f;
            }
        }
    }


    //Metodi controllejen lukitsemiseen
    //jotta on helppo löytää mistä lukitaan
    //Parametri int extraLock käytetään lisälukituksen luomiseen
    //esim. valikossa ainoa tapa, miten menu avataan, tapahtuu X-näppäimestä, joten kasvatetaan lukitusta +1
    //Näin kontrollit eivät vapaudu vahingossa jostain muualta, vaan sieltä missä on -1
    public void ToggleDisable(bool value, int extraLock)
    {
        //Jos lukitus halutaan lukita ja avata vain tietystä lähteestä
        //Laitetaan +1, kun lukitaan ja -1, kun avataan
        extraLock_m += extraLock;

        if(!value && extraLock_m > 0)
        {
            return;
        }

        disableControls = value;

        if (CamMove.cameraIsMoving)
        {
            controlsWillRemainLocked = value;
        }
    }
    //Lukitsee moodin vaihtamisen
    public void ToggleLockMode(bool value)
    {
        lockMode = value;
        mManager.HideOrRevealInfo(value);       //Hallitsee onko spacebar info sprite päällä vai ei
    }
    //Katsoo onko pelaaja keskustelussa
    public void ToggleConversation(bool value)  
    {
        isInConversation = value;
    }
    //Katsoo onko pelaaja kysymys tyyppisessä keskustelussa
    public void ToggleQuestion(bool value)      
    {
        isInQuestion = value;
    }
    //Lopetetaan hahmon liike. Jos true, liike loppuu kuin seinään
    public void StopMovement(bool instant)                  
    {
        characterAnimScript.agent.SetDestination(transform.position);
        if (instant)
            characterAnimScript.agent.velocity = Vector3.zero;
        if (PersistentManager.Instance.pManager.tempObject)
        {
            PersistentManager.Instance.pManager.tempObject.KillMe(); //Kadotetaan mahdollinen liikkumis indikaattori
        }
    }
    //Katsoo onko hiiri hud elementin päällä ja minkä
    public void MouseOverhud(string value) 
    {
        nameOfHudElement = value;

        if (!value.Equals(""))
        {
            characterAnimScript.DetermineFocus(null);
            tempCollider = null;
            curManager.mouseIsOver = false; //Kursorin muutos
            isOnHudButton = true;
            MouseHeld = 1;
            curManager.isInMenu = true;
            curManager.DefaultMouse();
        }
        else
        {
            curManager.isInMenu = false;
            isOnHudButton = false;
        }
    }


    //Kustuttaessa vaihtaa moodia FPS ja RTS välillä
    public void ToggleFPS()
    {
        if (!isFPS)
        {
            //RTS to FPS
            characterAnimScript.agent.updateRotation = false;
            CamMove.MoveCameraToPos(false);     //Liikuttaa kameran FPS tilaan
            interactableLayer = interactableLayerFPS;
            rayDistance -= 8;
        }
        else
        {
            //FPS to RTS
            characterAnimScript.agent.updateRotation = true;
            CamMove.MoveCameraToPos(true);      //Liikuttaa kameran RTS tilaan
            characterAnim.transform.GetChild(1).GetComponent<Renderer>().enabled = true; //Tuo pelaaja hahmo objektin näkyviin (piilottaa sen myöhemmin, kun RTS to FPS)
            interactableLayer = interactableLayerRTS;
            rayDistance += 8;
        }
        //Vaihdetaan kursori tavalliseksi
        if (curManager)
            curManager.DefaultMouse();

        //Tapahtuu aina kun moodi vaihtuu
        //Pysäyttää liikkeen
        StopMovement(true);
        //Käy läpi tietyt objektit ja laittaa ne näkyviin/piiloon
        mManager.GoThroughAll();
        //vaihtaa muuttujan
        isFPS = !isFPS;
    }

}

