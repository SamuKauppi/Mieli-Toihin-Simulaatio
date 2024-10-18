using TMPro;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]

//Teht‰v‰ luokka
//Sis‰lt‰‰ kaiken tiedon mit‰ yhteen teht‰v‰‰n tarvitaan
public class MissionClass 
{
    public string[] missionNames;               //Kaikkien mahdollisten teht‰vien nimet
    public int currentStage;                    //Nykyinen suoritusvaihe
    public int maxCompletionStage;              //Maksimi suoritus vaihe

    public TextMeshProUGUI completionValueUi;   //Suoritus tason teksti
    public GameObject valueBackground;

    public GameObject completionCheckmark;      //Valmis merkki, kun teht‰v‰ on valmis

    public Sprite missionSprite;                //Teht‰v‰n sprite

    public TextMeshProUGUI descriptionText;     //Teht‰v‰‰ kuvaava teksti (voidaan k‰ytt‰‰ vaihtamiseen)

    public Image mainImageSprite;
}
