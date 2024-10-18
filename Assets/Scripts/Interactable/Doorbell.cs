using UnityEngine;

//Ovikellon skripti
//Tekee välkkymis animaation ja suorittaa seuraavan askeleen tutoriaalissa
public class Doorbell : MonoBehaviour
{

    Animator anim;                              //Ovikellon oma animaattori 
    public RTS_player player;                   //Pelaaja
    public AloitusScripti startScript;          //Viittaus tutoriaali scriptiin     
     
    //Haetaan animaattori
    private void Start()
    { 
        anim = GetComponent<Animator>();
    }

    //Animaatioevent, joka lukitsee pelaajan näppäimet heti kun ovikello soi tutoriaalissa
    //merkitsee vain tutoriaaliin
    public void Bell()
    {
        if (startScript.stepCounter < 13 && startScript.stepCounter != 7)
        {
            player.ToggleDisable(true, 0);
            player.ToggleLockMode(true);
        }
    }

    //Animaatioevent, joka kutsutaan ovikellon värinvälkkymis animaation jälkeen
    //Jos pelaaja on pääsyt tarpeeksi pitkälle tutoriaalissa (startScript.stepCounter < 13), niin peli vain lopettaa ovikello animaation
    //Muuten laukaisee seuraavan vaiheen tutoriaalissa
    public void StopMyBell()
    {
        anim.SetBool("bell", false);
        if (startScript.stepCounter < 13 && startScript.stepCounter != 7)
        {
            startScript.NextStepWithParameter(13);
        }
    }
}
