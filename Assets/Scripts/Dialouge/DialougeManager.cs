using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


//Scripti, joka suorittaa dialogin näyttämisen
public class DialougeManager : MonoBehaviour
{

    private Queue<string> sentences;                        //Jono lauseita Dialogitrigeristä
    public TextMeshProUGUI nametext;                        //Ui-text Nimi teksti
    public Image nameTextBackground;                        //Ui-image Nimitekstin taustakuva
    public TextMeshProUGUI sentenceText;                    //Ui-text teksti kenttä
    public GameObject nextText;                             //PopupTeksti, joka sanoo seuraavan tekstin tulevan näkyviin enter näppäimellä
    public GameObject skipText;                             //Lopettaa keskustelun heti (pitää olla pois päältä tutoriaalissa)

    public Animator textAnim;                               //dialogi laatikon animaattori (käytetään dialogilaatikon esiin tuomisessa)
    public AloitusScripti startScript;                      //Referenssi tutorial scriptiin
    public RTS_player player;                               //Referenssi pelaajan scriptiin
    AIScript aiPlayer;                                      //tietkonepelaaja, jolle jutellaan
    bool aiConversation;                                    //Onko kyseessä tietkonepelaaja vai tutoriaali

    string[] CharacterNames;                                //Tähän tallennetaan hahmojen nimet
    bool hadAStarSign;                                      //Tekstissä oli * merkki joten seuraava kirjan vaihtaa nimeä keskustelun aikana. Numero on indeksi
    bool hadAUpSign;                                        //Tekstissä oli ^ merkki joten seuraava kirjan suorittaa halutun metodin. Numeroa verrataan switch lauseessa

    bool waitWasSkipped;                                    //Pelaaja ei avannut seuraavaa tekstiä tarpeeksi nopeasti, joten seuraava lause ladataan
                                                            //waitWasSkipped on true silloin, kun tekstiä kirjoitetaan
    bool showEntireText;                                    //Ja jos seuraavaa lausetta pyydetään silloin kun ^ on true, näytetään koko lause heti

    public AudioSource aud;                                         //Äänenlähde
    private List<AudioClip> currentClips = new List<AudioClip>();   //Nykyiset ääni klipit
    int sentenceCounter;                                            //Käytetään ääni klippien soittamiseen                                    

    //Alustetaan jono Awake()-metodissa
    void Awake()
    {
        sentences = new Queue<string>();                  
        textAnim.gameObject.SetActive(false);
    }

    //Haetaan referenssit, jos ne puuttuu
    public void InitializeReferences()
    {
        if (GameObject.Find("Character"))
        {
            player = GameObject.Find("Character").GetComponent<RTS_player>();
            nextText.GetComponent<Button>().onClick.AddListener(() => player.StopMovement(true));
        }
        if (GameObject.Find("StartTutorialScript"))
        {
            startScript = GameObject.Find("StartTutorialScript").GetComponent<AloitusScripti>();
        }
    }

    //Kutsutaan juuri ennen kuin pelaaja puhuu tietkonepelaajalle
    //Tallentaa sen tiedot tänne
    public void AIScriptInitilization(AIScript ai)
    {
        aiPlayer = ai;
        aiConversation = true;
    }

    //Aloittaa dialogin
    //Metodi aloittaa keskustelun lataamalla parametristä tiedot ja lukitsee pelaajan kontrollit
    //Sitten tyhjentää edellisen dialogin ja kutsuu DisplayNextSentence()
    public void StartDialouge(Dialouge dialouge)
    {
        StopAllCoroutines();
        textAnim.gameObject.SetActive(true);

        //Alustaa referenssit, koska objekti on "doNotDestroyOnLoad"
        if(!player || !startScript)
        {
            InitializeReferences();
        }
        //Lukitaan pelaajan controllit liikkumisen ajaksi
        player.ToggleDisable(true, 0);
        player.ToggleLockMode(true);
        player.ToggleConversation(true);

        //Haetaan nimet ja väri. Ensimmäinen nimi aloittaa keskustelun 
        nameTextBackground.color = dialouge.nameBackgroundImage;
        nametext.text = dialouge.characterNames[0];
        CharacterNames = dialouge.characterNames;

        //Äänet
        //katsoo onko dialogissa vähintään yksi ääni klippi
        if (dialouge.voices.Length > 0)
        {
            //Tyhjentää mahdolliset edelliset klipit
            if(currentClips.Count > 0)
            {
                currentClips.Clear();
            }

            //Lisää dialogissa olevat klipit listaan
            for (int i = 0; i < dialouge.voices.Length; i++)
            {
                currentClips.Add(dialouge.voices[i]);
            }
        }

        sentenceCounter = 0;

        textAnim.gameObject.SetActive(true);

        sentences.Clear();

        foreach (string sentence in dialouge.sentences)
        {
            sentences.Enqueue(sentence);
        }
        textAnim.SetBool("IsOpen", true);
        waitWasSkipped = false;

        DisplayNextSentence();
    }
    //Lataa seuraavan lauseen näkyviin kutsumalla TypeSentence(string sentence), jos lauseita on jäjellä
    //Jos ei, niin EndDialogue()
    public void DisplayNextSentence()
    {
        nextText.SetActive(false);
        if (waitWasSkipped && !player.isInQuestion)       
        {
            showEntireText = true;
            return;
        }
        if (sentences.Count == 0)
        {
            EndDialouge();
            return;
        }
        PersistentManager.Instance.aManager.Play("snap", player.gameObject, -1);
        waitWasSkipped = true;
        string sentence = sentences.Dequeue();

        if (startScript.stepCounter >= 15)
        {
            skipText.SetActive(true);
        }
        else
        {
            skipText.SetActive(false);
        }
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    //Kirjoittaa lauseen kirjain kerrallaan
    //Erikois merkit ^ ja * tekevät asioita kesken lauseen
    //* ja numero vaihtaa nimen indeksillä ChangeName(char c)-metodissa
    //^ ja numero suorittaa halutut asiat DoAMethod(char c)-metodissa
    //Soittaa myös ääniklipin, jos sellainen on
    IEnumerator TypeSentence(string sentence)
    {
        sentenceText.text = "";

        //Äänen toisto
        //Katsoo onko klippien määrä oikein (jos on yksi liian vähän skippaa)
        if (sentenceCounter < currentClips.Count)
        {
            //Katsoo onko listan indeksin kohdalla klippiä
            if (currentClips[sentenceCounter])
            {
                //lopettaa mahdollisen edellisen klipin ja toistaa seuraavan
                if (aud.isPlaying)
                {
                    aud.Stop();
                }
                aud.clip = currentClips[sentenceCounter];
                aud.Play();
            }
        }
        sentenceCounter++;

        foreach (char letter in sentence.ToCharArray())
        {
            if (hadAStarSign)
            {
                ChangeName(letter);
            }
            else if (hadAUpSign)
            {
                DoAMethod(letter);
            }
            else if (letter.Equals('^'))
            {
                hadAUpSign = true;
            }
            else if (letter.Equals('*'))
            {
                hadAStarSign = true;
            }
            else if (showEntireText)
            {
                sentenceText.text += letter;
            }
            else
            {
                sentenceText.text += letter;
                yield return new WaitForSecondsRealtime(0.01f);
            }
        }
        showEntireText = false;
        waitWasSkipped = false;
        if (!player.isInQuestion)
        {
            nextText.SetActive(true);
            StartCoroutine(SkipToNextTextAuto(sentence.Length));
        }
    }
    //Lataa seuraavan tekstin automaattisesti ajan jälkeen, jos pelaaja ei tee sitä itse
    IEnumerator SkipToNextTextAuto(float time)
    {
        yield return new WaitForSecondsRealtime(time * 0.15f + 2f);
        if (!waitWasSkipped && player.isInConversation) //Varmistaa, että pelaaja on keskustelussa ja että seuraavaa tekstiä ei ole aloitettu
        {
            DisplayNextSentence();
        }
    }
    //Vaihtaa nimeä kesken keskustelun
    //*-merkki tunnistaa, että nimi halutaan vaihtaa
    //merkkiä seuraava numero toimii indeksinä (ei käytetä missään)
    void ChangeName(char c)
    {
        int i = int.Parse(c.ToString());

        nametext.text = CharacterNames[i];

        hadAStarSign = false;
    }
    //Tekee metodin keskellä dialogia
    //^-merkki tunnistaa, että halutaan suorittaa metodi 
    //merkkiä seuraava numero määrittää minkä metodin
    void DoAMethod(char c)
    {
        switch (c)
        {
            case '0':
                player.ToggleLockMode(!player.lockMode);
                break;
            case '1':
                player.ToggleDisable(!player.disableControls, 0);
                break;
            case '2':
                startScript.stepCounter++;
                break;
            case '3':
                startScript.stepCounter--;
                break;
            case '4':
                startScript.stopTimer = !startScript.stopTimer;
                break;
            case '5':
                startScript.skipTimer = !startScript.skipTimer; //HUOM! PITÄÄ AINA OLLA, JOS HALUAA SKIPATA KOHTIA TUTORIAALISSA (ellei ole jo 4)
                break;
            case '6':
                startScript.ukkoHahmo.walkType = 4;
                StartCoroutine(startScript.ukkoHahmo.AiLogic());
                break;
            case '7':
                startScript.iButtonOpen.SetActive(true);
                break;

                //Pelin end creditsien lataaminen, kun kaikki tehtävät on suoritettu
            case '8':
                EndDialouge();
                PersistentManager.Instance.sManager.NextLevelToBeLoaded(3);
                PersistentManager.Instance.sManager.anim.SetTrigger("fade");
                break;

                //Tutoriaalin lopetus
            case '9':
                player.ToggleFPS();
                EndDialouge();
                break;

            default:
                break;
        }
        hadAUpSign = false;
    }

    //Lopettaa keskustelun 
    //Avaa kontrollit ja lataa seuraavan askeleen tutoriaalissa
    //Jos kyseessä on tietkonepelaaja, niin tehdään siihen liittyvät operaatiot
    public void EndDialouge()
    {
        StopAllCoroutines();
        textAnim.SetBool("IsOpen", false);
        if (startScript)
        {
            if (startScript.stepCounter <= 15)
            {
                startScript.AdvanceStep();
            }
            player.ToggleConversation(false);
        }
        skipText.SetActive(false);
        waitWasSkipped = false;

        if (aud.isPlaying)
        {
            aud.Stop();
        }
        if (aiConversation)
        {
            aiPlayer.DoneTalking();
            aiPlayer.speechBubbleScript.ToggleSpeechBubble(false);
            aiConversation = false;
            player.ToggleDisable(false, 0);
            player.ToggleLockMode(false);
        }
        currentClips.Clear();
    }
}
