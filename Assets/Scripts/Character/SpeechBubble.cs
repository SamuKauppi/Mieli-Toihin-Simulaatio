using UnityEngine;

//Skripti vastaa puhekuplasta
public class SpeechBubble : MonoBehaviour
{
    RTS_player player;                          //Viittaus pelaajaan
    AIScript aiPlayer;                          //T‰m‰n puhekuplan tietkonepelaaja

    public Transform cam;                       //Kamera
    public GameObject speechBubbleObject;       //Puhekupla canvas, joka on worldspacessa (Haetaan startissa)
    bool isVisible;                             //Onko puhekupla n‰kyviss‰ vai ei

    public Transform headPos;                   //Tietokonepelaajan p‰‰n transform
        
    //Haetaan startissa puhekupla- ja tietokonepelaaja skripti
    private void Start()
    {
        speechBubbleObject = transform.GetChild(0).gameObject;
        aiPlayer = GetComponentInParent<AIScript>();
    }

    //Puhekupla tuodaan esiin OnTriggerEnter ja kadotetaan OnTriggerExit
    //Samalla vaihdetaan tietokonepelaajan layermaski niin, ett‰ pelaaja lˆyt‰‰ sen 
    //n‰in pelaaja voi keskustella hahmon kanssa vain jos he ovat sen vieress‰
    private void OnTriggerEnter(Collider other)
    {

        //Katsotaan onko kyseess‰ pelaaja
        if (other.CompareTag("Player"))
        {
            //Hateaan pelaaja jos ei ole viitattu
            if (!player)
            {
                player = other.GetComponent<RTS_player>();
            }

            aiPlayer.tag = "interactable";                      //Tehd‰‰n tietokonehahmosta interactable, jotta pelaaja voi jutella
            aiPlayer.gameObject.layer = 11;                     //Layer = interactable
        }
    }
    private void OnTriggerExit(Collider other)
    {

        //Onko kyseess‰ pelaaja
        if (other.CompareTag("Player"))
        {
            speechBubbleObject.SetActive(false);            //Piiloon
            isVisible = false;                              //false

            aiPlayer.tag = "Door";                          //Poistetaan tietkonepelaajasta interactable
            aiPlayer.gameObject.layer = 2;                  //ja layer 

            if (!player)                                    //Jos player on jotenkin null
            {
                player = other.GetComponent<RTS_player>();
            }
        }
    }

    //Kadotetaan tai tuodaan puhekupla esiin
    //Jos pelaaja ei ole null ja pelaaja ei ole fps
    public void ToggleSpeechBubble(bool value)
    {
        if (player)
        {
            if (!player.isFPS)
            {
                transform.position = headPos.position;              //T‰m‰ objekti hahmon p‰‰n paikalle
                aiPlayer.LookAtPlayer(player.transform);            //Tietkonepelaaja katsoo pelaajaa
                speechBubbleObject.SetActive(value);                //N‰kyviin
                isVisible = value;
            }
        }
    }


    //Updatessa K‰‰nnet‰‰n puhekupla kohti kameraa
    //Ja kadotetaan tai tuodaan esiin puhekupla perustuen pelaajan moodiin (FPS = katoaa)
    private void Update()
    {
        if (speechBubbleObject.activeInHierarchy)
            transform.rotation = Quaternion.LookRotation(-cam.forward, cam.up);

        //Puhekupla ei n‰yt‰ hyv‰lt‰ fps moodissa, joten katsotaan onko pelaaja fps vai ei
        //ja kadotetaan fps-moodissa
        if (player && isVisible)
        {
            if(player.isFPS || player.isInConversation)
            {
                speechBubbleObject.SetActive(false);
            }
            else
            {
                speechBubbleObject.SetActive(true);
            }
        }
    }
}
