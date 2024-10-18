using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Vastaa kursorin muutoksesta ja muut skriptit vertaa tämän muutujia
public class CursorScript : MonoBehaviour
{
    public Texture2D[] cursors;             //Lista eri kursorieista
    public int index;                       //int johon tallennetaan nykyinen kursori

    //boolean määrittää mikä kursori on
    public bool cameraIsRotating;   //Kameraa käännetään
    public bool cameraCanRotate;    //Kameraa voidaan kääntää
    public bool playerIsMoving;     //Pelaaja voi liikkua
    public bool mouseIsOver;        //Hiiri on asian päällä
    public bool isInMenu;           //Onko pelaaja menussa

    //Varmistaa, että kursori on normaali Start()-metodissa
    private void Start()
    {
        ChangeCursor(0);
    }

    //Update()-metodissa päivitetään kursori
    //boolean muuttujia käytetään sen määrittämiseen'
    //Ja klikkaus laittaa klikkaus hiiren
    private void Update()
    {
        if (Time.frameCount % 2 == 0)
        {
            if (playerIsMoving && !isInMenu)
            {
                ChangeCursor(2);
            }
            else if (cameraIsRotating && !isInMenu)
            {
                ChangeCursor(5);
            }
            else if (mouseIsOver && !isInMenu)
            {
                ChangeCursor(3);
            }
            else if (cameraCanRotate && !isInMenu)
            {
                ChangeCursor(1);
            }
            else if (Input.GetMouseButtonDown(0))
            {
                ChangeCursor(4);
            }
            else
            {
                ChangeCursor(0);
            }
        }
    }

    //Metodi, jota voidaan käyttää normaalin hiiren palauttamiseen
    public void DefaultMouse()
    {
        mouseIsOver = false;
        cameraIsRotating = false;
        playerIsMoving = false;
        cameraCanRotate = false;
    }


    //Vaihtaa kursoria indeksin kohdalle
    //Täältä suoritetaan corutiini Click()
    void ChangeCursor(int i)
    {
        //Jos nykyinen kursori on sama kuin uusi, ei tehdä mitään
        //Tai edellinen oli 4 eli klikkaus. Uuden kursorin voi laittaa vasta, kun klikkaus vaihtuu itsestään pois 
        if (i == index || index == 4)
        {
            return;
        }
        //Klikkaus tapahtuu ja kursori on klikattu x ajan verran
        else if (i == 4)
        {
            StartCoroutine(Click());
            return;
        }
        //Ruutua käännetään ja hiiri on asian päällä, kursoria ei muuteta
        else if (index == 5 && cameraIsRotating)
        {
            return;
        }

        StopAllCoroutines();
        Cursor.SetCursor(cursors[i], Vector2.zero, CursorMode.Auto);
        index = i;
    }
    //Klikkaus
    //Hetken päästä kursori palautetaan normaaliksi
    IEnumerator Click()
    {
        yield return new WaitForSecondsRealtime(0.05f);
        Cursor.SetCursor(cursors[4], Vector2.zero, CursorMode.Auto);
        index = 4;
        yield return new WaitForSecondsRealtime(0.1f);
        Cursor.SetCursor(cursors[0], Vector2.zero, CursorMode.Auto);
        index = 0;
        yield return new WaitForSecondsRealtime(0.05f);
        DefaultMouse();
    }
}
