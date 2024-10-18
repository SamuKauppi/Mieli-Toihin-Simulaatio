using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Tämä scripti laittaa halutun 2D-grafiikan näkyviin näytölle
//Grafiikka pitää määrittää editorissa
public class PaintingScript : MonoBehaviour
{
    public RTS_player player;               //Viittaus pelaaja scriptiin

    public RawImage paintingTemplate;       //Pohja Ui-image, jonka kokoa ja spriteä muokataan         

    public float heightOffset;              //height ja width offset, jotka jättävät hieman tilaa molemmille puolille näyttöä
    public float widthOffset;

    public Canvas can;                      //Viittaus kanvakseen, jota käyteään näytön leveyden ja pituuden määrittämiseen
    public PaintingClass[] paintings;       //Lista kaikista maalauksista. Sisältää jokaisen taulun alkuperäisen nimen, pituuden ja leveyden
    public bool isOnScreen;                 //Katsotaan onko taulu näytöllä vai ei
    public Image border;                    //Raami. Väri vaihdetaan satunnaiseksi, kun taulu tuodaan esiin
    public TextMeshProUGUI paintingText;    //Teksti, johon kirjoitetaan teitoa taulusta

    public GameObject ClickArea;            //Musta hieman läpinäkyvä image, joka on taulun takana. Kun sitä klikataan, taulu katoaa

    [SerializeField] private GameObject topRightButtons;    //Yläkulman napit pitää kadottaa, kun taulu on näkyvillä. Ne tulee muuten eteen

    public bool fromMenu;                   //Katsoo tuleeko nykyinen taulu menusta ja kadottaa ClickArea:n
                                            //Jos true, ollaan menussa ja kontrolleja ei avata.


    //Tuo halutun tekstuurin näytölle ui-rawimgeen
    //Haluttu tekstuuri pitää olla valmiiksi määrtelty editorissa listaan paintings (tärkeää on kuva ja mittasuhteet)
    //En saanut Unityä hakemaan kuvan mittasuhteita suoraan kuvasta oikein, joten ne määritellään ne editorissa
    public void ShowImageOnScreen(string nameOfMaterial)
    {
        //Lopettaa mahdolliset corutiinit ja katsoo onko taulu jo näytöllä
        if (isOnScreen)
        {
            return;
        }
        StopAllCoroutines();

        //Koska vihaan itseäni, mutebuttonit tulee taulun eteen, jos taulu iso, joten kadotetaan ne
        PersistentManager.Instance.sManager.HideMuteButtons(false);
        topRightButtons.SetActive(false);

        //Määrittää taulun karmille uuden satunnaisen värin
        border.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

        //Laittaa templaten näkyviin ja resettaa sen mahdolliset skaalaukset
        paintingTemplate.gameObject.SetActive(true);
        LeanTween.cancel(paintingTemplate.gameObject);
        LeanTween.scale(paintingTemplate.gameObject, Vector2.one, 0f);

        //Määritellään kuvan mittasuhteet ensin vertaamalla materiaalin nimeä listassa oleviin nimiin
        float width = 0;
        float height = 0;
        nameOfMaterial = nameOfMaterial.Replace(" (Instance)", "");

        //Haetaan listasta kuva nimellä
        for (int i = 0; i < paintings.Length; i++)
        {
            if (paintings[i].paintingName.Contains(nameOfMaterial))
            {
                paintingTemplate.texture = paintings[i].painting;
                width = paintings[i].width;
                height = paintings[i].height;
                paintingText.text = paintings[i].paintingInfo;
                PersistentManager.Instance.missionManager.CheckForMissions(1, paintings[i].paintingName);
                break;
            }
        }
        //Jos kuvaa ei löytynyt, tulostetaan error ja lopetetaan metodi
        if(width == 0 || height == 0)
        {
            Debug.Log("error: painting not found " + nameOfMaterial);
            paintingTemplate.gameObject.SetActive(false);
            return;
        }

        //Haetaan Canvaksen koko minus offset (offset antaa hieman tilaa ruudun ja taulun väliin)
        //Muokataan transformin kokoa ja sitten skaalataan ui imagea kunnes sen koko on sama, kuin transformin 
        RectTransform Canvasrt = can.GetComponent<RectTransform>();
        transform.GetComponent<RectTransform>().sizeDelta = new Vector2(Canvasrt.rect.width - widthOffset, Canvasrt.rect.height - heightOffset);
        RectTransform rt = transform.GetComponent<RectTransform>();

        //Kasvatetaaan tai vähennetään kuvista suoraan saatua kokoa kunnes ne varmasti mahtuvat Canvaksen sisään
        if (width > rt.rect.width || height > rt.rect.height)
        {
            while (width > rt.rect.width || height > rt.rect.height)
            {
                width *= 0.95f;
                height *= 0.95f;
            }
        }
        else
        {
            while (width < rt.rect.width || height < rt.rect.width)
            {
                width *= 1.05f;
                height *= 1.05f;
            }
        }
        //Varmistetaan että koot on positiivisia (oli bugi jolloin ne oli negatiivisia)
        width = Mathf.Abs(width);
        height = Mathf.Abs(height);

        StartCoroutine(ScalePainting(width, height));
    }

    //IEnumeratorin sisällä kasvatetaan Ui-imagen rect koordinaatteja kunnes se on halutun kokoinen
    IEnumerator ScalePainting(float width, float height)
    {
        float currentWidth = paintingTemplate.GetComponent<RectTransform>().rect.width;
        float currentHeight = paintingTemplate.GetComponent<RectTransform>().rect.height;

        while (currentWidth < width || currentHeight < height)
        {
            yield return new WaitForSeconds(0.01f);
            paintingTemplate.GetComponent<RectTransform>().sizeDelta = new Vector2(currentWidth, currentHeight);
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
        paintingTemplate.GetComponent<RectTransform>().sizeDelta = new Vector2(currentWidth, currentHeight);
        isOnScreen = true;
        ClickArea.SetActive(true);
    }

    //Piiloitetaan kuva LeanTweenin avulla (koodi animaatio)
    //Katsoo onko peli menussa vai ei ja sen perusteella avaa kontrollit
    public void HidePainting()
    {
        if (!isOnScreen)
        {
            return;
        }

        if (!fromMenu)
        {
            player.ToggleLockMode(false);
            player.ToggleDisable(false, 0);
            player.StopMovement(true);
            if (player.isFPS)
            {
                player.CamMove.isFpsCameraMoveAllowed = true;
            }
        }
        ClickArea.SetActive(false);

        PersistentManager.Instance.sManager.HideMuteButtons(true);
        topRightButtons.SetActive(true);

        LeanTween.cancel(paintingTemplate.gameObject);
        LeanTween.scale(paintingTemplate.gameObject, Vector2.zero, 0.5f);
        StopCoroutine(ScalePainting(0, 0));
        StartCoroutine(HidePaintingWhenDone());
    }
    //Kun taulu on kadonnut, avataan kontrollit hetken päästä
    IEnumerator HidePaintingWhenDone()
    {
        yield return new WaitForSecondsRealtime(0.35f);
        paintingTemplate.gameObject.SetActive(false);
        paintingTemplate.GetComponent<RectTransform>().sizeDelta = Vector2.one;
        yield return new WaitForSeconds(0.35f);
        isOnScreen = false;
    }
}
