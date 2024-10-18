using UnityEngine;
using UnityEngine.EventSystems;

//Muuttaa muuttujaa pelaaja skirptiss‰ joka kerta, kun hiiri on hud napin p‰‰ll‰
//N‰in pelaaja ei muuta vahingossa, jos h‰n haluaa klikata nappia
//T‰m‰ p‰tee vain pelisceneen, mutta on hyv‰ laittaa kaikkiin hud elementteihin, jotka k‰yv‰t pelisceness‰
public class IsOverHudScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RTS_player player;       //Pelaaja
    bool isPlayerReference;         //Onko pelaajaa

    //Haetaaan pelaaja, jos sit‰ ei ole heti alussa
    private void OnDisable()
    {
        if (!isPlayerReference)
        {
            return;
        }
        else
        {
            if (player.nameOfHudElement.Equals(name))
            {
                player.MouseOverhud("");
            }
        }
    }

    //Hiiri on Ui-elementin p‰‰ll‰ ja p‰ivtet‰‰n tilanne pelaajalle
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isPlayerReference)
        {
            FindPlayerReference();
        }
        if (isPlayerReference)
            player.MouseOverhud(name);
    }
    //Hiiri ei ole en‰‰n Ui-elementin p‰‰ll‰ ja p‰ivtet‰‰n tilanne pelaajalle
    public void OnPointerExit(PointerEventData eventData)
    {
        if (isPlayerReference)
            player.MouseOverhud("");
    }

    //Haetaan pelaaja referenssi, jos sit‰ ei ole
    void FindPlayerReference()
    {
        if (GameObject.Find("Character"))
        {
            player = GameObject.Find("Character").GetComponent<RTS_player>();
            isPlayerReference = true;
        }
        else
        {
            isPlayerReference = false;
        }

    }
}
