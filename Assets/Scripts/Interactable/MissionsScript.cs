using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Skripti, joka vastaa teht‰vien suorituksesta ja Ui-efekteist‰
//Poikkeuksena on suoritettujen teht‰vien piilotus ilmoitustaulu valikossa
public class MissionsScript : MonoBehaviour
{
    //Osa hudia ja k‰ytet‰‰n vain ulkon‰ˆllisesti
    [SerializeField] private Image progressUpdateImage;               //Template kuva joka tulee vasemmalta oikealle, kun osa teht‰v‰st‰ suoritetaan
    [SerializeField] private  Image progressUpdateCheckmark;           //Laitetaan p‰‰lle vain, jos teht‰v‰ on 100% suoritettu

    [SerializeField] private TextMeshProUGUI progressUpdateValue;     //Jos ei ole 100%, niin n‰yet‰‰n numero (esim. 1/2)
    [SerializeField] private Image valueBackground;                   //NumeroArvo kuvakkeen tausta. Kadotetaan ja tuodaan esiin samaan aikaan kuin "progressUpdateImage"

    [SerializeField] private Sprite CompletedSpirteBackground;        //UI-tausta vaihdetaan ilmoitustaulu valikossa, kun teht‰v‰ suoritetaan

    //T‰ss‰ luokassa pidet‰‰n muistissa teht‰vien suoritus
    public MissionClass[] missions;

    MissionsList listOfMissions;                    //scripti, joka muistaa teht‰v‰t scenen sis‰ll‰

    [SerializeField] private RectTransform onPosition;                //Start, On ja Off positio. progressUpdateImage liikkuu n‰iden v‰lill‰ 
    [SerializeField] private RectTransform offPostion;
    [SerializeField] private RectTransform startPosition;     

    bool hasValuesBeenUpdated;                      //Ensimm‰isell‰ suorituskerralla ladataan teht‰v‰t suoraan MissonList-skriptist‰
                                                    //Koska t‰m‰ skripri on DoNotDestroyOnLoad(), niin se muistaa teht‰v‰t, mutta ei niiden ui elementtej‰
                                                    //T‰m‰ bool menee true ensimm‰isen suorituskerran j‰lkeen eli muistaa


    //Teht‰vien suorituksen tarkastus
    //Ensin etsii mahdolliset puuttuvat UI-referenssit metodissa UpdateMissionImages(bool value)
    //Sitten k‰y l‰pi kaikki teht‰v‰t ja katsoo vastaako parametri string s
    //Jos vastaa, niin lis‰t‰‰n suoritusta siihen ja suoritetaan corutiini ShowProgress(MissionClass mission)
    //Suorittaa myˆs CheckIfGameWasCompleted()-corutiinin, joka katsoo, onko teht‰v‰t tehty
    //Jos on, suoritetaan t‰m‰ metodi string "parasPelaaja", joka vaihtaa myˆs ilmoitustaulun kuvaa ja kuvausta
    public void CheckForMissions(int completion, string s)
    {
        if (!listOfMissions)            //Kun tullaan uuteen sceneen, p‰ivitet‰‰n teht‰vien kuvat, jotka ovat vain t‰ss‰ sceness‰
        {
            //hasValuesBeenUpdated on alkuarvona false ja muuttuu true, kun lista on ensimm‰isen kerran p‰ivitetty
            //N‰in, jos pelaaja vaihtaa scene‰, listan suoritus arvoja ei nollata, mutta sceness‰ olevat kuvat pit‰‰ hakea
            UpdateMissionImages(hasValuesBeenUpdated);
        }

        //K‰yd‰‰n l‰pi kaikki teht‰v‰t
        for (int i = 0; i < missions.Length; i++)
        {
            //Onko teht‰v‰ jo tehty?
            if (missions[i].currentStage < missions[i].maxCompletionStage)
            {
                //K‰yd‰‰n teht‰v‰n kaikki vaiheet l‰pi
                for (int v = 0; v < missions[i].missionNames.Length; v++)
                {
                    //Katsotaan vastaako tunnus jotakin teht‰v‰‰?
                    if (s.Equals(missions[i].missionNames[v]))
                    {
                        //Kasvatetaan teht‰v‰n suoritusta, vaihdetaan nimi suoritetuksi ja aloitetaan show progress
                        //for loop voidaan lopettaa
                        missions[i].currentStage += completion;
                        missions[i].missionNames[v] = missions[i].missionNames[v] + "Complete";

                        StopAllCoroutines();
                        StartCoroutine(ShowProgress(missions[i]));

                        //Salaisen aaven logiikka (^.~)7
                        //Tuo ekstra menu jutun n‰kyviin
                        if (s.Contains("aave"))
                        {
                            missions[i].valueBackground.transform.parent.parent.gameObject.SetActive(true);
                        }
                        //Katsoo onko kyseess‰ viimeinen teht‰v‰
                        //Muutetaan menua, jos true ja lopetetaan suoritus
                        if (s.Contains("parasPelaaja"))
                        {
                            missions[i].valueBackground.transform.parent.GetComponent<Image>().sprite = missions[i].missionSprite;
                            missions[i].descriptionText.text = "Hienoa! Suoritit kaikki teht‰v‰t! K‰y juttelemassa Tommille h‰nen toimistossaan. H‰nell‰ saattaa olla jotain sinulle!";
                            return;
                        }

                        //Katsoo onko kaikki teht‰v‰t tehty
                        //Jos on, avataan viimeinen teht‰v‰
                        if (CheckIfGameWasCompleted())
                        {
                            StartCoroutine(GameCompleted());
                        }
                        return;
                    }
                }
            }
        }
    }

    //Puuttuvien UI-referenssejen p‰ivitys uuteen sceneen ladattaessa
    //false = ensimm‰inen suorituskerta, joten haetaan kaikki
    //true = seruaavilla suorituskerroilla haetaan vain Ui-elementit
    void UpdateMissionImages(bool value)
    {
        //Etsit‰‰n lista objekti
        listOfMissions = GameObject.Find("MissionsList").GetComponent<MissionsList>();

        //Ensimm‰isen suoritus kerran j‰lkeen suoritetaan t‰st‰ eteenp‰in
        if (value)
        {
            //K‰yd‰‰n teht‰vien lista l‰pi
            if (missions.Length > 0)
            {
                for (int i = 0; i < missions.Length; i++)
                {
                    //P‰ivitet‰‰‰n scene objektin muuttujat
                    missions[i].completionValueUi = listOfMissions.missions[i].completionValueUi;
                    missions[i].valueBackground = listOfMissions.missions[i].valueBackground;
                    missions[i].completionCheckmark = listOfMissions.missions[i].completionCheckmark;
                    missions[i].mainImageSprite = listOfMissions.missions[i].mainImageSprite;

                    if (listOfMissions.missions[i].descriptionText)
                    {
                        missions[i].descriptionText = listOfMissions.missions[i].descriptionText;
                    }

                    //P‰ivitet‰‰n mahdollinen suoritus vaihe ilmoitustaululle
                    if (missions[i].currentStage > 0)
                    {
                        //jos teht‰v‰ on valmis
                        if (missions[i].currentStage >= missions[i].maxCompletionStage)
                        {
                            missions[i].currentStage = missions[i].maxCompletionStage;
                            missions[i].completionCheckmark.SetActive(true);
                            missions[i].valueBackground.gameObject.SetActive(false);

                            missions[i].mainImageSprite.sprite = CompletedSpirteBackground;

                            if (missions[i].missionNames[0].Contains("parasPelaaja"))
                            {
                                missions[i].valueBackground.transform.parent.GetComponent<Image>().sprite = missions[i].missionSprite;
                                missions[i].descriptionText = listOfMissions.missions[i].descriptionText;
                                missions[i].descriptionText.text = "Hienoa! Suoritit kaikki teht‰v‰t! K‰y juttelemassa Tommille h‰nen toimistossaan. H‰nell‰ saattaa olla jotain sinulle!";
                            }
                        }
                        //muuten:
                        else
                        {
                            missions[i].completionValueUi.text = missions[i].currentStage + "/" + missions[i].maxCompletionStage;
                        }
                    }
                }
            }
        }
        //Ensimm‰inen suoritus kerta
        else
        {
            missions = listOfMissions.missions;
        }
        hasValuesBeenUpdated = true;
    }

    //Osa teht‰v‰‰ suoritettu
    //N‰ytet‰‰n kuvake ja hetken p‰‰st‰ piilotetaan se. P‰ivitet‰‰n myˆs menu/skripti
    IEnumerator ShowProgress(MissionClass mission)
    {
        //Liikutetaan kuvaa
        if (LeanTween.isTweening(progressUpdateImage.gameObject))
        {
            LeanTween.cancel(progressUpdateImage.gameObject);
            progressUpdateImage.transform.LeanMove(startPosition.position, 0);
        }

        progressUpdateImage.transform.localScale = new Vector2(1f, 1f);
        LeanTween.move(progressUpdateImage.gameObject, new Vector2(onPosition.position.x, onPosition.position.y), 0.8f).setEase(LeanTweenType.easeOutQuad);

        //Ensin tuodaan kuva n‰kyviin
        progressUpdateImage.gameObject.SetActive(true);
        progressUpdateCheckmark.gameObject.SetActive(false);
        valueBackground.gameObject.SetActive(true);
        progressUpdateValue.text = mission.currentStage + "/" + mission.maxCompletionStage;

        //Jos teht‰v‰ on suoritettu
        if (mission.currentStage >= mission.maxCompletionStage)
        {

            //Sama tehd‰‰n menuun ja luokkaan
            mission.currentStage = mission.maxCompletionStage;
            mission.completionCheckmark.SetActive(true);
            mission.valueBackground.gameObject.SetActive(false);
            mission.mainImageSprite.sprite = CompletedSpirteBackground;

            //tuodaan checkmark esiin ja piilotetaan suoritus vaihe teksti ("10/50"-teksti piiloon ja "OK"-kuva n‰kyviin")
            valueBackground.gameObject.SetActive(false);
            progressUpdateCheckmark.gameObject.SetActive(true);
        }
        else
        {
            //Muuten p‰ivitet‰‰n menun ja esiin tuleva kuvan tekstien arvot
            progressUpdateValue.text = mission.currentStage + "/" + mission.maxCompletionStage;

            mission.completionValueUi.text = mission.currentStage + "/" + mission.maxCompletionStage;
        }

        //Tuodaan kuva hitaasti esiin muutamaksi sekunniksi ja sitten piilotetaan hitaasti
        progressUpdateImage.sprite = mission.missionSprite;
        progressUpdateImage.color = new Color(1f, 1f, 1f, 0);
        valueBackground.color = progressUpdateImage.color;

        for (float i = 0; i < 1; i += Time.deltaTime)
        {
            progressUpdateImage.color = new Color(1f, 1f, 1f, i);
            valueBackground.color = progressUpdateImage.color;
            progressUpdateCheckmark.color = progressUpdateImage.color;
            progressUpdateValue.color = new Color(0f, 0f, 0f, i);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(4f);


        LeanTween.move(progressUpdateImage.gameObject, new Vector2(offPostion.position.x, offPostion.position.y), 1f).setEase(LeanTweenType.easeInQuad);
        LeanTween.scale(progressUpdateImage.gameObject, new Vector2(progressUpdateImage.transform.localScale.x * 0.25f, progressUpdateImage.transform.localScale.y * 0.25f), 2f);
        for (float i = 1; i > 0; i -= Time.deltaTime)
        {
            progressUpdateImage.color = new Color(1f, 1f, 1f, i);
            valueBackground.color = progressUpdateImage.color;
            progressUpdateCheckmark.color = progressUpdateImage.color;
            progressUpdateValue.color = new Color(0f, 0f, 0f, i);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(2f);

        //Piilotetaan kuva ja liikutetaan se alku positioon
        LeanTween.move(progressUpdateImage.gameObject, new Vector2(startPosition.position.x, startPosition.position.y), 0f);
        progressUpdateImage.gameObject.SetActive(false);
    }


    //Katsoo onko kaikki teht‰v‰t tehty ja palauttaa bool arvon
    bool CheckIfGameWasCompleted()
    {
        int completeionStage = 0;
        //Kasvattaa muuttujaa yhdell‰ jokaisesta suoritetusta teht‰v‰st‰
        for (int i = 0; i < missions.Length; i++)
        {
            if(missions[i].currentStage >= missions[i].maxCompletionStage)
            {
                completeionStage++;
            }
            //super duper salainen aave teht‰v‰ ei ole pakollinen ja katsotaan aina suoritetuksi
            else if (missions[i].missionNames[0].Contains("aave"))
            {
                completeionStage++;
            }
        }
        //Jos kaikki teht‰v‰t on suoritettu
        //Suoritetaan GameCompleted() tuossa alhaalla ja palatutetaan true
        if (completeionStage >= missions.Length - 1)
        {
            return true;
        }
        //jos ei ole suoritettu, palautetaan false
        return false;
    }
    //Pienen viiven j‰lkeen suoritetaan viimeinen teht‰v‰, kaikki teht‰v‰t suoritettu
    IEnumerator GameCompleted()
    {
        yield return new WaitForSecondsRealtime(6.5f);
        CheckForMissions(1, "parasPelaaja");
    }

    //Kustuttaessa k‰y teht‰v‰t l‰pi ja palauttaa true, jos haluttu teht‰v‰ on suoritettu
    public bool CheckIfAMissonHasBeenDone(string s)
    {
        for (int i = 0; i < missions.Length; i++)
        {
            for (int v = 0; v < missions[i].missionNames.Length; v++)
            {
                if (missions[i].missionNames[v].Contains(s))
                {
                    if(missions[i].currentStage >= missions[i].maxCompletionStage || missions[i].missionNames[v].Contains("Complete"))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
