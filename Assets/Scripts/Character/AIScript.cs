using System.Collections;
using UnityEngine;

//Vastaa tietkonepelaajan logiikasta
public class AIScript : MonoBehaviour
{
    public int walkType;                    //M‰‰ritt‰‰ mik‰ k‰vely tyyppi on kyseess‰
                                            //1 = satunnainen, 2 = k‰y listan l‰pi ja pys‰htyy viimeiseen, 3 = j‰rjestyksess‰, 4 = tutoriaali

    public Transform[] pointsOfIntrest;     //K‰vely pisteet (Jos transformissa on interactable-skripti, se k‰ytet‰‰n)
    float randomTimer = 1f;                 //Satunnainen ajastin, jos hahmo k‰velee niin m‰‰ritet‰‰n satunnainen aika kauan hahmo odottaa kunnes menee seuraavaan pisteeseen
    float timer;                            //Ajastin, jota k‰yet‰‰n ajan mittaamiseen, kun pelaaja puhuu hahmolle

    public AnimationEvents animEvent;       //Referenssi animaatioevent skriptiin 

    public DialougeTrigger NpcDialogue;     //Tietokonepelaajan dialogi

    public SpeechBubble speechBubbleScript; //Referenssi puhekuplaskriptiin


    //Kun objekti tulee p‰‰lle, aloitetaan corutiini: AiLogic()
    private void OnEnable()
    {
        StartCoroutine(AiLogic());
    }
    //timer laskee aikaa updatessa koko ajan
    private void Update()
    {
        timer += Time.deltaTime;
    }



    //Tietokonepelaajan logiikka
    //Vertaa int walkType-muuttujaa switch-lauseessa
    //1 = satunnainen, 2 = k‰y listan l‰pi ja pys‰htyy viimeiseen, 3 = j‰rjestyksess‰, 4 = tutoriaali
    public IEnumerator AiLogic()
    {
        yield return new WaitForSecondsRealtime(randomTimer);

        switch (walkType)
        {
            // 1 = Satunnainen k‰vely. K‰velee satunnaiseen kohteeseen
            case 1:
                while (true)
                {
                    if (!animEvent.willISit)
                    {
                        timer = 0f;
                        randomTimer = Random.Range(25f, 30f);
                        MoveToPos(pointsOfIntrest[Random.Range(0, pointsOfIntrest.Length)]);
                    }
                    yield return new WaitForSeconds(randomTimer);
                }

            // 2 = K‰y kohteet l‰pi ja pys‰htyy viimeiseen
            case 2:
                for (int v = 0; v < pointsOfIntrest.Length; v++)
                {
                    MoveToPos(pointsOfIntrest[v]);
                    yield return new WaitForSeconds(2);
                }
                break;

            // 3 = Pisteest‰ pisteeseen k‰vely. K‰y listaa j‰rjestyksess‰ l‰pi (loop)
            case 3:
                int i = 0;
                while (true)
                {
                    if (!animEvent.willISit)
                    {
                        timer = 0f;
                        MoveToPos(pointsOfIntrest[i]);
                        i++;
                        if (i >= pointsOfIntrest.Length)
                        {
                            i = 0;
                        }
                        randomTimer = Random.Range(25f, 30f);
                    }
                    yield return new WaitForSeconds(randomTimer);
                }

            // Tutoriaali hahmon logiikka, kun tutoriaali loppuu
            case 4:
                Outline outline = GetComponent<Outline>();
                for (i = 0; i < pointsOfIntrest.Length; i++)
                {
                    MoveToPos(pointsOfIntrest[i]);
                    yield return new WaitForSecondsRealtime(1.5f);
                    outline.enabled = false;
                    yield return new WaitForSeconds(1.5f);
                }
                walkType = 10; 
                break;

            default:
                break;
        }
    }

    //Laittaa hahmon liikkumaan tiettyyn paikkaan ja jos kohde, jonka luokse k‰vell‰‰n sis‰lt‰‰ Interactable-skriptin, niin suoritetaan se
    //Suoritetaan AiLogic()-metodista
    void MoveToPos(Transform v)
    {

        //Jos tietkonepelaajalla on edellinen kohde viel‰, tyhjennet‰‰n se
        if (animEvent.currentFocus)
        {
            animEvent.DetermineFocus(null);
        }

        //Jos uusi kohde on interactable, suoritetaan se
        if (v.GetComponent<Interactable>())
        {
            animEvent.DetermineFocus(v.GetComponent<Interactable>());
            animEvent.ActivateFocus(animEvent.currentFocus.stopMovementOnActivation);
        }
        //Muuten k‰vele kohteen luokse
        else
        {
            animEvent.MoveTo(v.position);
        }
    }

    //Puhuu pelaajalle
    //AiLogic() pys‰ytet‰‰n ja k‰ynnistet‰‰n dialogi
    public void TalkingTo(Transform t)
    {
        //Katsotaan, ettei tietokonehahmo ole menossa istumaan
        if (!animEvent.willISit)
        {
            //Kadotetaan puhekupla ja lopetetaan logiikka hetkellisesti
            speechBubbleScript.ToggleSpeechBubble(false);
            StopAllCoroutines();

            //Jos hahmo ei istu, niin hahmo pys‰htyy ja katsoo pelaaja p‰in
            if (!animEvent.isSitting)
            {
                animEvent.MoveTo(transform.position);
                LookAtPlayer(t);
            }
            //Aloitetaan dialogi
            NpcDialogue.TriggerDialouge();
        }
    }

    //Kutsuttaessa katsoo pelaajaa p‰in, jos ei istu tai ei ajo istua
    public void LookAtPlayer(Transform t)
    {
        if (!animEvent.isSitting && !animEvent.willISit)
        {
            transform.LookAt(t.position);
        }
    }

    //V‰hennet‰‰n kulunut aika (float timer) satunnaisesta ajastimesta (float randomTimer)
    //Ja k‰ynnistet‰‰n logiikka erotuksella
    public void DoneTalking()
    {
        StopAllCoroutines();
        randomTimer -= timer;
        if (walkType != 2)
        {
            StartCoroutine(AiLogic());
        }
    }

}
