using UnityEngine;

[System.Serializable]
//Dialogi luokka
public class Dialouge  
{
    //nimi
    public string[] characterNames;

    public Color nameBackgroundImage;

    //lista tekstejä, joilla on hieman isompi tekstikenttä
    [TextArea(3, 10)]
    public string[] sentences;

    public AudioClip[] voices;
}
