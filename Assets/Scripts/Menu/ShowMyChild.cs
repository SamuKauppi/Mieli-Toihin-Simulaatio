using UnityEngine;
using UnityEngine.EventSystems;

//Näyttää apudialogin aikana tekstin jos hiiri menee päälle
public class ShowMyChild : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject textfield;

    private void Start()
    {
        if (!textfield)
            textfield = transform.GetChild(0).gameObject;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        textfield.SetActive(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        textfield.SetActive(false);
    }
}
