using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Suorittaa erillisen ohjeen logiikan
//P‰‰valikosta valitsee: Ohjeet
public class GuideScript : MonoBehaviour
{
    public Sprite[] guideImages;           //Array spritet
    public Image targetGraphic;            //Template image, jonka sprite‰ muutetaan

    [TextArea (3, 15)]
    public string[] flavourText;           //Kuvien tekstit
    public TextMeshProUGUI targetText;     //Teksti objekti, jonka teksti‰ muutetaan

    public GameObject background;          //Tausta, joka laitetaan p‰‰lle tai pois

    int counter;                           //Lasketaan mik‰ kuva on nyt
    public TextMeshProUGUI CounterText;    //Teksti objekti, johon laitetaan counter arvo

    //Laitetaan seuraava kuva
    //paramteri lis‰t‰‰n counter ja ladataan vastaava kuva
    //Ui-buttonit kustuvat t‰t‰ metodia
    public void NextImage(int i)
    {
        counter += i;

        if (counter < 0)
        {
            counter = guideImages.Length - 1;
        }
        else if (counter >= guideImages.Length)
        {
            counter = 0;
        }

        CounterText.text = (counter + 1) + "/" + guideImages.Length;

        targetGraphic.sprite = guideImages[counter];
        targetText.text = flavourText[counter];
    }

    //Laitetaan koko ohje menu p‰‰lle tai pois
    public void ShowGraphic(bool value)
    {
        background.SetActive(value);

        if (!value)
            targetGraphic.sprite = null;
        else
        {
            counter = 0;
            targetGraphic.sprite = guideImages[counter];
            targetText.text = flavourText[counter];
        }
    }

}
