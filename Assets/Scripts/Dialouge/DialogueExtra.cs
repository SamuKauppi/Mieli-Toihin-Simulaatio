using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

//Dialogi luokka, joka pit‰‰ sis‰ll‰‰n arrayn lis‰ dialogeista ja niihin liittyvi‰ muutujia
//Tietokonepelaajat k‰ytt‰v‰t n‰it‰
public class DialogueExtra 
{
    public string[] missionsToBeCompleted;      //Mitk‰ teht‰v‰t muokkaavat dialogia
    public Dialouge[] dialogue;                 //Vaihtoehtoiset dialogit 
    public int completed;                       //Kuinka monta on suoritettu
    public int talkCount;                       //Kuinka monta kertaa hahmolle on juteltu
    public int stagesOfCompleteion;             //Jos halutaan monta eri vaihetta ja jokaisella vaiheella dialogi vaihtuu (0 = tarkoittaa, ett‰ kaikki pit‰‰ suorittaa)
    public AnimationEvents otherCharacter;      //Referenssi toiseen hahmoon, jos halutaan k‰ytt‰‰ sielt‰ jotain
}
