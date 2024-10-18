using UnityEngine;

//Dialogin trigger. Kutsutaan kun halutaan aloittaa dialogi
public class DialougeTrigger : MonoBehaviour
{
    public Dialouge dialouge;                   //Nykyinen dialogi
    Dialouge originalDialogue;                  //Alkuperäinen dialogi

    //Vain tietokonepelaaja dialogiin liittyvää
    public AIScript aiPlayer;                   //tietokone pelaaja (jos skripti on siinä kiinni)
    public bool notInTutorial;                  //Onko triggeri tutoriaalissa vai ei? (true = ei tutoriaalissa)
    public int priorityType;                    //Minkä extra dialogin skripti valitsee? (esim. 2 = valitsee satunnaisen dialogin ja 3 = valitsee järjestyksessä)
    public DialogueExtra AlternativeDialogue;   //Luokka, joka sisältää vaihtoehtoiset dialogit, tehtävät niiden saamiseen ja kuinka monta niistä on suoritettu
   
    //Määritetään alkuperäinen dialogi
    private void Start()
    {
        originalDialogue = dialouge;
    }

    //Aloittaa dialogin
    //Jos notInTutorial = true, niin kutsutaan DetermineADialogue()
    public void TriggerDialouge()
    {
        //Jos tietokone aloittaa dialogin, pitää kertoa dialogimanageriin
        //Sekä katsoo taritseeko dialogi vaihtaa
        if (notInTutorial)
        {
            PersistentManager.Instance.dManager.AIScriptInitilization(aiPlayer);
            dialouge = DeterimineADialogue();
        }
        PersistentManager.Instance.dManager.StartDialouge(dialouge);
    }

    //Palauttaa dialogin. Dialgoi haetaan DialogueExtra luokasta perustuen priorityType muuttujaan
    //Valitsee aina ensimmäisellä kerralla alkuperäisen dialogin
    //0 = tekee monimutkaisen laskun perustuen kuinka monta tehtävää on suoritettu
    //1 = valitsee satunnaisen dialogin perustuen kuinka monta tehtävää on tehty
    //2 = valitsee satunnaisen dialogin
    //3 = valitsee dialogin järjestyksessä
    Dialouge DeterimineADialogue()
    {
        int tempIndex;
        switch (priorityType)
        {
            // 0 = Valitsee dialogin perustuen kuinka monta tehtävää on suoritettu
            //Vaihtoehtoisten dialogjen määrä pitää valita muuttujalla stagesOfCompleteion
            case 0:
                //Jos kaikki tehtävät on suoritettu, palautetaan viimeinen dialogi vaihtoehto
                if (AmountOfCompleteion() + 1 == AlternativeDialogue.missionsToBeCompleted.Length)
                {
                    return AlternativeDialogue.dialogue[AlternativeDialogue.dialogue.Length - 1];
                }
                else if (AlternativeDialogue.stagesOfCompleteion != 0)
                {
                    //Jaetaan tehtävien määrä stagesOfCompleteion. Näin tiedetään kuinka monta tehtävää pitää suorittaa seuraavaan vaiheeseen
                    tempIndex = AlternativeDialogue.missionsToBeCompleted.Length / AlternativeDialogue.stagesOfCompleteion;
                    int tempCompleteion = AmountOfCompleteion();
                    int i = 1;  //Kerroin

                    while (true)
                    {
                        //Katsotaan onko suoritettuja tehtäviä tarpeeksi seuraavaan dialogiin
                        //Jos on, kasvatetaan kerrointa yhdelä
                        if (tempCompleteion < tempIndex * i)
                        {
                            //Kerroin alkoi luvulla 1, jotta sillä voidaan kertoa
                            i -= 2;

                            //Jos Kerroin on -1 eli ei yhtään tehtävää ole tehty, palautetaan alkuperäinen dialogi
                            if (i < 0)
                            {
                                return originalDialogue;
                            }
                            //Jos kerroin ei ylitä dialogien määrää tai valitse viimeistä dialogia
                            else if (i < AlternativeDialogue.dialogue.Length - 1)
                            {
                                return AlternativeDialogue.dialogue[i];
                            }
                            //Jos ylittää, palautetaan toiseksi viimeinen dialogi
                            else
                            {
                                return AlternativeDialogue.dialogue[AlternativeDialogue.dialogue.Length - 2];
                            }
                        }
                        i++;
                    }
                }
                break;

            // 1 = Valitsee satunnaisen dialogin suoritetuista tehtävistä 
            // (ensimmäisellä suorituskerralla aina alkuperäinen ja ei koskaan toista kahta samaa peräkkäin)
            case 1:
                while (true)
                {
                    tempIndex = Random.Range(-1, AlternativeDialogue.missionsToBeCompleted.Length);
                    if (AlternativeDialogue.stagesOfCompleteion != tempIndex)
                    {
                        AlternativeDialogue.stagesOfCompleteion = tempIndex;
                        AlternativeDialogue.talkCount++;

                        if (tempIndex == -1 || AlternativeDialogue.talkCount <= 0)
                        {
                            return originalDialogue;
                        }
                        else
                        {
                            if (PersistentManager.Instance.missionManager.CheckIfAMissonHasBeenDone(AlternativeDialogue.missionsToBeCompleted[tempIndex]))
                            {
                                return AlternativeDialogue.dialogue[tempIndex];
                            }
                        }
                    }
                }

            // 2 = Valitsee satunnaisen dialogin 
            // (ensimmäisellä suorituskerralla aina alkuperäinen ja ei koskaan toista kahta samaa peräkkäin)
            // Ainoa ero yllä olevaan on, että ei katsota suoritettuja tehtäviä
            case 2:
                int temps = 0;
                while (true)
                {
                    tempIndex = Random.Range(-1, AlternativeDialogue.dialogue.Length);
                    if (AlternativeDialogue.stagesOfCompleteion != tempIndex)
                    {
                        AlternativeDialogue.stagesOfCompleteion = tempIndex;
                        AlternativeDialogue.talkCount++;

                        if (tempIndex == -1 || AlternativeDialogue.talkCount <= 1)
                        {
                            return originalDialogue;
                        }
                        else
                        {
                            return AlternativeDialogue.dialogue[tempIndex];
                        }
                    }
                    temps++;
                    if (temps == 100)
                    {
                        Debug.Log("fail");
                        break;
                    }
                }
                break;

            //3 = valitsee dialgoin järjestyksessä
            //AlternativeDialogue.completed arvo -1 valitsee alkuperäisen dialogin ja koska sitä kasvatetaan metodin alussa +1
            //Sen pitää olla alkuarvossa -2
            //Kun arvo kasvaa yli dialogejen pituuden, niin completed saa arvon -1 ja palautetaan alkuperäinen dialogi
            case 3:

                AlternativeDialogue.completed++;
                if (AlternativeDialogue.completed >= AlternativeDialogue.dialogue.Length || AlternativeDialogue.completed < 0)
                {
                    AlternativeDialogue.completed = -1;
                    return originalDialogue;
                }
                return AlternativeDialogue.dialogue[AlternativeDialogue.completed];

            default:
                return originalDialogue;
        }
        return originalDialogue;
    }


    //Katsoo kuinka monta tehtävää on suoritettu ja palauttaa suoritusmäärän indeksin
    //Esim 3 suoritettu, palauttaa 2. Näin, jos ei yhtään tehtävää ei ole suoritettu palautus on -1
    int AmountOfCompleteion()
    {
        AlternativeDialogue.completed = -1;
        for (int i = 0; i < AlternativeDialogue.missionsToBeCompleted.Length; i++)
        {
            if (PersistentManager.Instance.missionManager.CheckIfAMissonHasBeenDone(AlternativeDialogue.missionsToBeCompleted[i]))
            {
                AlternativeDialogue.completed++;
            }
        }
        return AlternativeDialogue.completed;
    }
}
