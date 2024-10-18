using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//Tuo ilmoitustaulun esiin pelin aikana
//Tämä ja PaintingScript oli aika OOF.mp4
public class Infotable : MonoBehaviour
{
    public RawImage infoTableBase;          //Pohja, joka kasvatetaan kokoruudulle
    public bool isOnScreen;                 //Onko ilmoitustaulu ruudulla vai ei?
    public GameObject tableStuff;           //Main_Menu objekti (sisältää ilmoitustaulun päävalikon)
    public RectTransform can;               //Canvaksen koko

    public PaintingScript paintingScript;   //Taulu skripti. Kerrotaan tänne, kun on auki. Liittyy taulun avaamisen lukitusten toimiseen.
    public RTS_player player;               //Pelaaja referenssi

    //Buttoneja (ominaisuudet määritetään Start()-metodissa)
    public Button introButton;              //Nappi, joka avaa aloituksen
    public Button guideButton;              //Nappi, joka avaa ohjeruudun
    public Button backToMainMenuButton;     //Nappi, joka avaa päävalikon
    public Button endCreditsButton;         //Nappi, joka avaa lopputekstit

    [SerializeField] private GameObject MenuOnButton;   //gameobjektit, jotka vastaa ilmoitustaulun avaamisesta ja sulkemisesta
    [SerializeField] private GameObject MenuOffButton;  //Laitetaan päälle ja pois perustuen onko ilmoitustaulu auki

    //Startissa määritellään napeille logiikkaa
    private void Start()
    {
        //Määritetään "katso aloitus animaatio" napin ominaisuudet pelin alussa.
        //Avaa scenen indeksillä 1 klikattuna
        introButton.onClick.AddListener(() => PersistentManager.Instance.areManager.SpawnMenuQuestion(1, "palata alkuesitykseen"));


        //Määritetään "takaisin päävalikkoon" napin ominaisuudet pelin alussa
        //Avaa scenen indeksillä 0 klikattuna
        backToMainMenuButton.onClick.AddListener(() => PersistentManager.Instance.areManager.SpawnMenuQuestion(0, "palata päävalikkoon"));


        //Määritetään "lisää ohjeistusta" napin ominaisuus pelin alussa
        guideButton.onClick.AddListener(() => PersistentManager.Instance.gManager.ShowGraphic(true));

        //Määritetään "tekijät" napin ominaisuudet
        //Avaa scenen indeksillä 3 klikattuna
        endCreditsButton.onClick.AddListener(() => PersistentManager.Instance.areManager.SpawnMenuQuestion(3, "mennä katsomaan lopputekstejä"));

    }

    //Tuo ilmoitustaulun näkyviin ScalePainting(float width, float height) lol - miksi kirjoitin tähän "lol"? Olinko niin tilted?
    //Lukittaa kontrollit ja yms. ennen kuin skaalaa ilmoitustaulun ruudun kokoiseksi
    public void ShowInfoTable()
    {
        if (isOnScreen)
        {
            return;
        }

        MenuOnButton.SetActive(false);
        MenuOffButton.SetActive(false);

        PersistentManager.Instance.curManager.isInMenu = true;

        PersistentManager.Instance.missionManager.CheckForMissions(1, "ilmoitustaulu");
        if (player)
        {
            player.ToggleDisable(true, 1);
            player.ToggleLockMode(true);
            if (player.isFPS)
            {
                player.CamMove.isFpsCameraMoveAllowed = false;
            }
        }

        isOnScreen = true;
        LeanTween.cancel(infoTableBase.gameObject);
        LeanTween.scale(infoTableBase.gameObject, Vector2.one, 0f);

        infoTableBase.gameObject.SetActive(true);
        ToggleStuff(false);

        RectTransform rt = can.GetComponent<RectTransform>();

        StopAllCoroutines();
        StartCoroutine(ScalePainting(rt.rect.width, rt.rect.height));
    }
    IEnumerator ScalePainting(float width, float height)
    {
        float currentWidth = infoTableBase.GetComponent<RectTransform>().rect.width;
        float currentHeight = infoTableBase.GetComponent<RectTransform>().rect.height;

        while (currentWidth < width || currentHeight < height)
        {
            yield return new WaitForSeconds(0.01f);
            infoTableBase.GetComponent<RectTransform>().sizeDelta = new Vector2(currentWidth, currentHeight);
            if (currentHeight < height)
            {
                currentHeight += height * 0.1f;
            }
            if (currentWidth < width)
            {
                currentWidth += width * 0.1f;
            }
        }
        currentHeight = height;
        currentWidth = width;
        infoTableBase.GetComponent<RectTransform>().sizeDelta = new Vector2(currentWidth, currentHeight);
        ToggleStuff(true);
        isOnScreen = true;
        paintingScript.fromMenu = true;
        MenuOffButton.SetActive(true);
    }

    //Piilottaa infotaulun lul - mikä minussa on vikana?
    //Avaa kontrollit ja yms., mutta skaalaa ilmoitustaulun pieneksi käyttäen LeanTweeniä 
    //Lopuksi suorittaa corutiinin HideInfotableWhenDone(), joka vain piilottaa ilmoitustaulun, jotta se ei ole tiellä
    public void HideInfotable()
    {
        if (!isOnScreen)
        {
            return;
        }

        MenuOnButton.SetActive(false);
        MenuOffButton.SetActive(false);

        PersistentManager.Instance.curManager.isInMenu = false;

        if (player)
        {
            player.ToggleDisable(false, -1);
            player.ToggleLockMode(false);
            if (player.isFPS)
            {
                player.CamMove.isFpsCameraMoveAllowed = true;
            }
        }

        LeanTween.cancel(infoTableBase.gameObject);
        LeanTween.scale(infoTableBase.gameObject, Vector2.zero, 0.5f).setEase(LeanTweenType.easeOutCirc);
        StopAllCoroutines();
        ToggleStuff(false);
        StartCoroutine(HideInfotableWhenDone());
    }
    IEnumerator HideInfotableWhenDone()
    {
        yield return new WaitForSecondsRealtime(0.55f);
        infoTableBase.GetComponent<RectTransform>().sizeDelta = Vector2.one;
        infoTableBase.gameObject.SetActive(false);
        isOnScreen = false;
        paintingScript.fromMenu = false;
        MenuOnButton.SetActive(true);
    }

    //Tuodaan tai kadotetaan Main_Menu kustuttaessa
    //Näin se tulee näkyviin vasta, kun ilmoitustaulu on kokonaan avautunut ja katoaa heti, kun ilmoitustaulu suljetaan
    void ToggleStuff(bool value)
    {
        tableStuff.SetActive(value);
    }

}