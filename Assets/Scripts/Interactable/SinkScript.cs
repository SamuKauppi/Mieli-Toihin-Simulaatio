using System.Collections;
using UnityEngine;


//Tuhlaa vett‰ hanan kautta
//Soittaa ‰‰ni efektin ja koodianimoi kahta vesi objektia
public class SinkScript : MonoBehaviour
{   
    bool isOn;                          //Onko hana p‰‰ll‰
    public GameObject flowingWater;     //Vesi-objekti, joka tulee hanasta 
    public GameObject sinkWater;        //Vesi-objekti, joka tulee altaasta
    float startPos;                     //allasveden alku kohta
    AudioSource aud;                    //AudioSource
    int soundIndex;                     //ƒ‰ni-indeksi (haetaan Interactable)

    //Haetaan ‰‰ni-indeki ja allasveden alku positio
    private void Start()
    {
        soundIndex = GetComponentInParent<Interactable>().soundIndex;

        startPos = sinkWater.transform.localPosition.y;
    }
    
    //Avaa tai sulkee hanan perustuen onko se p‰‰ll‰ vai ei
    //Jos hana avataan, niin kustutaan StopSink()
    //Hanan itsest‰‰n avautuminen estet‰‰n StopAllCorutines()-metodilla
    //Animoi molempia vesi-objekteja. Hanaa skaalataan ylh‰‰lt‰ alas ja allasta siirret‰‰n alhaalta ylˆs
    public void ActivateSink()
    {
        StopAllCoroutines();
        LeanTween.cancel(flowingWater);
        LeanTween.cancel(sinkWater);
        if (!isOn)  //K‰ynnist‰‰
        {
            LeanTween.scaleY(flowingWater, 0.15f, 0.2f);
            LeanTween.moveLocalY(sinkWater, startPos + 0.2f, 1.5f);
            PersistentManager.Instance.aManager.Play("hanaauki", gameObject, soundIndex);
            if (gameObject.activeInHierarchy)
                StartCoroutine(StopSink());
        }
        else //Sulkee
        {
            LeanTween.scaleY(flowingWater, 0f, 0.01f);
            LeanTween.moveLocalY(sinkWater, startPos, 1.5f);
            aud.Stop();
        }

        if (!aud)
            aud = GetComponent<AudioSource>();
        isOn = !isOn;
    }
    //Jos hana on p‰‰ll‰, niin se suljetaan 4sec p‰‰st‰
    IEnumerator StopSink()
    {
        yield return new WaitForSecondsRealtime(4f);
        ActivateSink();
    }
}
