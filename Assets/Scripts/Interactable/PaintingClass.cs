using UnityEngine.UI;
using UnityEngine;
[System.Serializable]

//Taulun luokka
//Pitää sisällään kaiken tauluin liittyvän
public class PaintingClass 
{
    public string paintingName;
    public float width;
    public float height;
    [TextArea(3, 10)]
    public string paintingInfo;

    public Texture painting;
}
