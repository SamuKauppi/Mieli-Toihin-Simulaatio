using System.Collections;
using UnityEngine;
using UnityEngine.AI;

//Scripti, joka on kaikissa hahmoissa ja löytää animaatioeventit
//Vastaa myös hahmon liikkumisen laskemisesta ja kaikkien asioiden tarkastelusta
public class AnimationEvents : MonoBehaviour
{
    //Referenssit
    ParticleEfeectManager pManager;                 //Referenssi particleeffectmanageriin
    public RTS_player player;                       //Referenssi pelaajaan (käytetään vain jos aRealDude = true)
    public AloitusScripti startScript;              //Referenssi tutoriaaliin

    public Animator anim;                           //Hahmon animaattori
    public bool aRealDude = false;                  //Tietokone vai pelaaja

    //transofrmeja
    public Transform handBonePos;                   //Käsi luun transform (käytetään juonti/syönnissä
    public Transform cupInHand;                     //Nykyinen objekti kädessä
    public Transform forwardEmptyObject;            //Aina pelaajan takana ja katsotan sen suuntaan, kun istuminen on valmis

    //Kaikki askeleiden äänet
    public AudioClip[] conreteClips;
    public AudioClip[] grassClips;
    public AudioClip[] indoorClips;
    //Ääniä
    AudioClip[] currentClips;                       //Nykyiset askeleet (vaihdetaan)
    AudioSource aud;                                //Askleten audiosource
    public int groundType;                          //Määrittää millä maalla hahmo seisoo (Vain ääniin liittyvä)
    int previousClip = 0;                           //Muistaa edellisen ääniklipin ja ei valitse samaa kahtakertaa peräkkäin

    public GameObject[] hatsAndStuff;               //Hatut

    //Navmesh
    public NavMeshAgent agent;                      //Navmesh agentti (Pelaajalla parentissa ja AIlla tässä. Haetaan Start metodissa)
    public float movementSpeed;                     //Perus maksimi nopeus (ei muuteta. Käytetään nopeuden muistamiseen)
    float currentSpeed;                             //Nykyinen maksimi nopeus (vaihtuu ja määrittää hahmon animaation nopuden)

    //Istuminen
    public bool willISit;                           //Onko istumis prosessi käynnissä vai ei?
    public bool isSitting;                          //Istuuko hahmo vai ei?
    public Transform interactableTarget;            //Kohde jonka luokse mennään istumaan
    BoxCollider myCollider;                         //Kun tietokone pelaaja istuu, siirretään collider hahmon mallin päälle. Näin hahmon hitbox ei jää tuolin eteen (interactable)
    public bool currentSeatType;                    //onko tietokonetuoli vai normaalituoli? (hahmon pitää kääntyä 180 astetta normaalilla tuolilla, joten pitää muistaa)    

    //Kohteen käyttö
    public Interactable currentFocus;               //Nykyinen Interactable-kohde
    Interactable toBeUsedFocus;                     //Interactable-kohde, jonka luokse kävellään ja käytetään
    public Interactable previousFocus;              //Edellinen Interactable-kohde (esim. jos istuu tuolilla se on estetty muille ja kun noustaan avataan tämä)
    float elapsed;                                  //Ajastin (laittaa pienen viiveen ennen kuin hahmo tekee jotain)
    bool onItsWayToUseTheThing;                     //Onko hahmo matkalla käyttämään jotakin objektia vai ei?

    //Hakee referenssejä, joita puuttuu ja määrittää pelaajalle hatun
    private void Start()
    {
        DetereineCosmetics();                               //Vaihtaa hatun
        pManager = PersistentManager.Instance.pManager;     //Haetaan pManager (näin ei tarvitse kirjoittaa "PersistentManager.Instance...")
        if (aRealDude)
        {
            agent = player.GetComponent<NavMeshAgent>();    //Haetaan pelaajan navmesh
            SwitchStepType(0);                              //ja askeltyyppin määritys
        }
        else
        {
            agent = GetComponent<NavMeshAgent>();           //Haetaan tietokoneen navmesh
            myCollider = GetComponent<BoxCollider>();       //ja collider
            SwitchStepType(2);                              //sekä askeltyyppin määritys
        }
        aud = GetComponent<AudioSource>();                  //Audiosource
        anim = GetComponent<Animator>();                    //Animaattori
    }

    //Updatessa määrittää hahmon kävely animaation ja suorittaa DoAThing()-metodin, jos Interactable-kohhteen suoritus aloitetaan
    private void Update()
    {
        //Animaation nopeuden määritys (tarpeeksi hidas on idle)
        currentSpeed = Mathf.InverseLerp(0, 1, agent.velocity.magnitude);
        anim.SetFloat("WalkSpeed", currentSpeed);

        //Jos muuttuja on true, silloin suoritetaan objektin viereen kävely ja suoritus
        if (onItsWayToUseTheThing)
        {
            DoAThing();
        }
    }

    //Interactable-kohteen käyttö
    //Kävelee asian eteen ja käyttää sen, kun on pysähtynyt
    //Tehdään monia asioita perustuen onko pelaaja vai tietokone tai onko tavallinen objekti vai tuoli?
    private void DoAThing()
    {
        elapsed += Time.deltaTime;
        //Mittaa pelaajan liikettä ja katsoo, että aikaa on mennyt 1 s
        if (agent.velocity == Vector3.zero && elapsed > 1f)
        {
            agent.transform.LookAt(new Vector3(interactableTarget.position.x, transform.position.y, interactableTarget.position.z));
            //laitetaan hahmo katsomaan asiaa päin

            if (aRealDude)          //Pelaaja
            {
                if (player.isFPS)   //on fps 
                {
                    //Jos kyseessä on tuoli, katsotaan maahan tuolin edessä 
                    if (willISit)
                    {
                        player.CamMove.playerSubBody.LookAt(new Vector3(forwardEmptyObject.position.x, player.CamMove.playerSubBody.position.y, forwardEmptyObject.position.z));
                        player.cam.transform.LookAt(new Vector3(forwardEmptyObject.position.x, interactableTarget.position.y, forwardEmptyObject.position.z));
                    }
                    //Muuten katsotaan itse kohdetta
                    else
                    {
                        player.CamMove.playerSubBody.LookAt(new Vector3(interactableTarget.position.x, player.CamMove.playerSubBody.position.y, interactableTarget.position.z));
                        player.cam.transform.LookAt(interactableTarget);
                    }
                }
            }

            //Jos kyseessä istuminen laitetaan animaatio
            //(katso metodi "Sit()", josta istuminen alkaa)
            if (willISit)
            {
                anim.ResetTrigger("GetUp");
                if (!currentSeatType)
                    anim.SetTrigger("Sit");
                else
                    anim.SetTrigger("SitOffice");
            }
            //Muuten kävellään asian eteen ja käytetään se
            //Ja suoritus loppuu tähän
            else if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (aRealDude)
                {
                    player.ToggleDisable(false, 0);
                    player.ToggleLockMode(false);
                }
                ActivateFocus(true);
                onItsWayToUseTheThing = false;
            }
        }
        else
        {
            //Jos hahmo liikku ja istumis prosessi on käynnissä
            //Määritetään hahmon noepus perustuen kuinka kaukana se on asiasta, mutta ei koskaan ylity maksimin (movementSpeed) ja mene alle minimin (0.35)
            //(katso metodi "Sit()", josta istuminen alkaa)
            if (willISit)
            {
                if (agent.remainingDistance + 0.35f > movementSpeed)
                {
                    agent.speed = movementSpeed;
                }
                else
                {
                    agent.speed = agent.remainingDistance + 0.35f;
                }

                if (aRealDude && player.isFPS) //Koko prosessin ajan kamera katsoo tuolia, jos pelaaja on fps moodissa
                {
                    player.CamMove.playerSubBody.LookAt(interactableTarget.position, Vector3.up);
                    player.cam.transform.LookAt(interactableTarget);
                }
            }
        }
    }


    //Laittaa satunnaisen hatun hahmolle kutsuttaessa
    public void DetereineCosmetics()
    {
        for (int i = 0; i < hatsAndStuff.Length; i++)
        {
            hatsAndStuff[i].SetActive(false);
        }
        int index = Random.Range(0, hatsAndStuff.Length);

        if (index == 0) //0 = lasit, joten laitetaan toinen hattu myös
        {
            hatsAndStuff[index].SetActive(true);
            index = Random.Range(1, hatsAndStuff.Length);
        }

        hatsAndStuff[index].SetActive(true);
    }

    //Aktivoi Interactable-kohteen, jos paramteri on true
    //Jos on false, laittaa hahmon kävelemään Interactable-kohteen luo sekä pitää kohteen muistissa. Metodi aktivoidaan uudstaan true arvolla, kun hahmo on Interactable-kohteen luona
    //Jos on Interactable-kohteen on tuoli, suoritetaan Interactable-kohteen nyt, koska kohteen luokse kävely tehdään Sit(Transform point, bool value)-metodissa
    public void ActivateFocus(bool value)
    {
        //Katsotaan onko focus olemassa tai onko focus joka tullaan käyttämään
        if (currentFocus || toBeUsedFocus)
        {
            //reset aika ja tavallinen hiiri
            elapsed = 0f;
            PersistentManager.Instance.curManager.DefaultMouse();

            //Jos on mahdollinen edellinen focus, vapautetaan se käytöstä
            if (previousFocus)
            {
                previousFocus.isBeingUsed = false;
                previousFocus = null;
            }

            //Varmistetaan, että kohde ei ole tuoli ja hahmo ei ole jo menossa istumaan
            if (!value && !currentFocus.objectType.Contains("tuoli") && !isSitting)
            {
                //Haetaan istumiseen tarvittava kohde ja kävellään sen luo
                interactableTarget = currentFocus.transform;
                agent.SetDestination(interactableTarget.position);

                //Kerrotaan että hahmo on menossa tekemään asian. (update metodissa)
                onItsWayToUseTheThing = !value;
                //Kohde joka tullaan käyttämään määritetään
                toBeUsedFocus = currentFocus;

                //Katsotaan onko kyseessä pelaaja
                if (aRealDude)
                {
                    //lukitetaan kontrollit ja luodaan particle effect kohteeseen
                    player.ToggleDisable(true, 0);
                    player.ToggleLockMode(true);
                    pManager.SpawnAParticleEffect(interactableTarget.position);

                    //Määritetään navmeshagent kääntyminen perustuen onko pelaaja fps
                    if (player.isFPS)
                    {
                        agent.updateRotation = false;
                    }
                    else
                    {
                        agent.updateRotation = true;
                    }
                }
            }
            else
            {
                //Määritetään tilapäinen intreactable.
                //Jos toBeUsedFocus on käytössä, 
                //niin se tarkoittaa sitä, että metdoi suoritettiin äsken false arvolla ja hahmo on nyt kohteen luona
                Interactable i;
                if (toBeUsedFocus)
                    i = toBeUsedFocus;
                else
                    i = currentFocus;

                //Jos kohde ei ole käytössä, suoritetaan se
                if (!i.isBeingUsed)
                {
                    i.ActivateTheThing(this, aRealDude);
                    i.ToggleOutline(false);
                    previousFocus = i;
                    previousFocus.isBeingUsed = true;
                    toBeUsedFocus = null;
                }
                //Jos kohde on käytössä, pelaajan kontrollit pitää avata
                else if (aRealDude)
                {
                    player.ToggleDisable(false, 0);
                    player.ToggleLockMode(false);

                    if (player.isFPS)
                    {
                        agent.updateRotation = false;
                    }
                    else
                    {
                        agent.updateRotation = true;
                    }
                }
            }
            //Piilotetaan hud näppäimet, suorituksen ajaksi
            if (aRealDude)
            {
                startScript.ToggleHudButtons(value);
            }
        }
    }

    //Määrittää Interactable-kohteen ja sen outlinen käytön. Jos parametri ei ole null, siitä tulee nykyinen kohde.
    //katsoo samalla onko Interactable-kohde tietkonepelaaja ja jos on, niin päivitetään puhekuplan näkyvyys
    public void DetermineFocus(Interactable i)
    {
        //Bug fix
        //Ei suoriteta, jos pelaaja on ulkona, kun ovet on lukittuna
        if (aRealDude)
        {
            if (startScript.stepCounter < 9)
            {
                return;
            }
        }

        //Määritetään uusi focus
        if (i)
        {
            if (currentFocus != i)
            {
                currentFocus = i;
                if (aRealDude)
                {
                    currentFocus.ToggleOutline(true);
                    if (currentFocus.objectType.Equals("hahmo"))
                    {
                        currentFocus.GetComponent<AIScript>().speechBubbleScript.ToggleSpeechBubble(true);
                    }

                }
            }
            else { return; }
        }
        else
        {
            if (currentFocus && aRealDude) //katsotaan onko edellistä kohdetta ja jos on, kadotetaan sen outline
            {
                currentFocus.ToggleOutline(false);

                if (currentFocus.objectType.Equals("hahmo"))
                {
                   currentFocus.GetComponent<AIScript>().speechBubbleScript.ToggleSpeechBubble(false);
                }
            }
            currentFocus = null;
        }

    }

    //Laittaa navmeshagentille määränpään
    //Resetoi mahdollisen istumis animaation
    public void MoveTo(Vector3 v)
    {
        //Määränpää
        agent.SetDestination(v);

        //Animaation resetointi
        anim.ResetTrigger("Sit");
        anim.ResetTrigger("SitOffice");
        anim.SetTrigger("GetUp");

        willISit = false;

        //Jos on edellinen focus, niin se vapautetaan
        if (previousFocus)
        {
            previousFocus.isBeingUsed = false;
            previousFocus = null;
        }

        //Paricle-effectin luonti
        if (aRealDude && agent.destination.magnitude > agent.stoppingDistance)
        {
            pManager.SpawnAParticleEffect(v);
        }
    }

    //Askeleen äänet
    //Vaihtaa askeleen äänen tyypiä
    public void SwitchStepType(int i)
    {
        groundType = i;
        switch (groundType)
        {
            case 0:
                currentClips = conreteClips;
                break;
            case 1:
                currentClips = grassClips;
                break;
            case 2:
                currentClips = indoorClips;
                break;
            default:
                return;
        }
    }
    //Soittaa satunnaisen askeleen äänen 
    public void Step()
    {
        aud.Stop();
        int i = Random.Range(0, currentClips.Length);

        while (previousClip == i) //välttää toistamisen
        {
             i = Random.Range(0, currentClips.Length);
        }
        aud.clip = currentClips[i];
        aud.Play();
        previousClip = i;
    }


    //Istumisprosessi alkaa tästä
    //Kustuttaessa hahmo kävelee tiettyyn pisteeseen (tuolin edessä tai takana oleva emptygameobject)
    //Lukitsee kontrollit ja laittaa "willISit" true (kertoo muille skirpteille, että hahmo on istumisprosessissa)
    //Kun hahmo lopettaa liikkumisen (eli seisoo emptyn päällä), 
    //Hahmo katsoo tuolia ja aloittaa istumis animaation (tapahtuu Updatessa).
    //bool value-parametri päivittää currentSeatType. Onko tuoli vai konetuoli (istumisanimaatio on erilainen, joten se pitää muistaa)
    public void Sit(Transform point, bool value)
    {
        onItsWayToUseTheThing = true;

        currentSeatType = value;

        anim.ResetTrigger("Sit");
        anim.ResetTrigger("SitOffice");
        anim.SetTrigger("GetUp");

        agent.stoppingDistance = 0.05f;
        agent.SetDestination(point.position);
        interactableTarget = point.parent;     //Tallennetaan emptyn gameobjektin parent (tuoli objekti itse), jotta sitä päin voidaan katsoa
        willISit = true;
        isSitting = false;

        if (aRealDude)
        {
            player.ToggleDisable(true, 1);
            player.ToggleLockMode(true);
            pManager.SpawnAParticleEffect(point.parent.position);

            if (player.isFPS)
            {
                agent.updateRotation = false;
            }
            else
            {
                agent.updateRotation = true;
            }
        }
    }
    
    //Hahmo käveli tuolin eteen ja istumis animaatio on nyt loppu. Siiryttiin istumis idle animaatioon.
    //Kontrollien lukitus poistetaan (disablecontrols ja willISit) ja isSitting = true. Scriptit tietää nyt, että pelaaja istuu ja voi nousta ylös
    //Suoritetaan animaatioeventistä
    public void NowSitting()
    {
        willISit = false;
        if (!isSitting) //Suoritetaan vain kerran per istuminen
        {
            //Hahmo ei ole enään matkalla kohteeseen
            onItsWayToUseTheThing = false;

            if (aRealDude)
            {
                //Avataan kontrollit
                player.ToggleLockMode(false);
                player.ToggleDisable(false, -1);

                //Kameran positio fps moodissa päivitetään
                player.CamMove.reverseSitting = currentSeatType;
                //Kamera tietää, että pelaaja istuu
                player.CamMove.isSitting = true;

                //Jos pelaaja on fps, niin kameran positio ja rotaatio pitää päivittää
                if (player.isFPS)
                {
                    player.CamMove.ChangeHeadPos(true);
                    player.CamMove.resetRot = true;
                }

                if (PersistentManager.Instance.pManager.tempObject)
                {
                    PersistentManager.Instance.pManager.tempObject.KillMe(); //Kadotetaan mahdollinen liikkumis indikaattori
                }

                startScript.ToggleHudButtons(true); //Tuodaan hud napit takaisin esiin
            }
            else
            {
                //tietokoneen hitboxin positio päivitetään
                myCollider.center = new Vector3(0f, myCollider.center.y, 3f);
            }

            isSitting = true;
            agent.stoppingDistance = 0.5f;
            agent.speed = 0f;
        }
    }

    //Animaatio event, joka
    //kustutaan, kun pelaaja on lopettanut nousemis animaation.
    //Ei lähde liikkelle ennen kuin tämä suoritetaan, koska nopeus on istumisen aikana 0
    public void NowGettingUp()
    {
        if (aRealDude)
        {
            if (!player.isFPS)
            {
                agent.updateRotation = true;
                anim.ResetTrigger("Sit");
                anim.ResetTrigger("SitOffice");
                anim.SetTrigger("GetUp");
            }
            else
            {
                player.CamMove.ChangeHeadPos(false);
            }
            player.ToggleLockMode(false);
            player.CamMove.isSitting = false;
        }
        else
        {
            anim.ResetTrigger("Sit");
            anim.ResetTrigger("SitOffice");
            anim.SetTrigger("GetUp");
            myCollider.center = new Vector3(0f, myCollider.center.y, 0f);
        }
        agent.speed = movementSpeed;
        isSitting = false;
    }
    //Istumisprosessi päättyy

    //Aina kun hahmo suorittaa idle animaation, hahmolla on mahdollisuus tehdä erikoinen idle animaatio'
    //Animaationevent
    public void IdleAnim()
    {

        int random = Random.Range(0, 300);

        if (random == 0)
        {
            anim.Play("Armature|Idle_animS");
        }
        else if (random > 0 && random <= 30)
        {
            anim.Play("Armature|Idle_anim1");
        }
        else if (random > 30 && random <= 60)
        {
            anim.Play("Armature|Idle_anim2");
        }
        else if (random > 60 && random <= 90)
        {
            anim.Play("Armature|Idle_anim3");
        }
        else if (random > 90 && random <= 120)
        {
            anim.Play("Armature|Idle_anim4");
        }

    }

    //Kun pelaaja on suorittanut vilkutus animaation, seuraava askel suoritetaan tutoriaalissa
    public void WaveDone()
    {
        if (aRealDude)
        {
            if (startScript.stepCounter < 13 && startScript.stepCounter != 7) //Vain tietyssä vaiheessa tutoriaalia suoritetaan seuraava askel
                startScript.NextStep();
        }
    }

    //Juominen/syöminen
    //katsoo istuuko pelaaja vai ei
    //ja onko halutaanko kuppi vai ruokaa
    public void Consume(string s)
    {
        SpawnADrink(s);

        if (s.Contains("Kuppi"))
        {
            anim.SetTrigger("Drink");
        }
        else
        {
            anim.SetTrigger("Hunger");
        }

        if (aRealDude)
        {
            player.ToggleLockMode(true);
            player.ToggleDisable(true, 1);
        }
    }
    //Kustuttaessa luodaan ruoka ja parametri määrittää, mitä ruokaa tehdään
    public void SpawnADrink(string s)
    {
        if (cupInHand == null)
        {
            PersistentManager.Instance.cManager.SpawnADrink(handBonePos, this, s);
            StartCoroutine(CupRotation());
        }
    }
    //Pitää ruuan kädssä kiinni
    IEnumerator CupRotation()
    {
        float i = 0;
        while (cupInHand != null)
        {
            i++;
            cupInHand.position = new Vector3(handBonePos.position.x, handBonePos.position.y - 0.1f, handBonePos.position.z);
            yield return new WaitForSeconds(0.03f);
            if (i > 200) //Failsafe jos ShatterCup() ei tule animaatio eventistä
            {
                ShatterCup();
            }
        }
    }
    //Kustuttaessa rikkoo kupin (animaationevent tai ajastimella CupRotation()-metodissa)
    public void ShatterCup()
    {
        if (cupInHand)
        {
            PersistentManager.Instance.cManager.ShatterCup(cupInHand);
        }
        StopCoroutine(CupRotation());
        if (aRealDude)
        {
            player.ToggleDisable(false, -1);
            player.ToggleLockMode(false);

            if (player.isFPS)
                player.CamMove.resetRot = true;

            startScript.ToggleHudButtons(true);
        }

        if (previousFocus)
        {
            previousFocus.isBeingUsed = false;
            previousFocus = null;
        }
    }
}
