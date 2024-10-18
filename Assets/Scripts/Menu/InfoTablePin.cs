using UnityEngine;
using UnityEngine.UI;

//Vaihtaa ilmoitustaulun pinnejen väriä pelin latautuessa
public class InfoTablePin : MonoBehaviour
{
    public Sprite[] pins;
    void OnEnable()
    {
        int i = Random.Range(0, pins.Length);
        transform.GetComponent<Image>().sprite = pins[i];
    }

}
