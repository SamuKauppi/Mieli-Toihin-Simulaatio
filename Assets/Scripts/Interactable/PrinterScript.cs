using System.Collections;
using UnityEngine;


//3D-tulostimen skripti
//Soittaa ‰‰nt‰ ja siirt‰‰ suutinta
public class PrinterScript : MonoBehaviour
{
    public Transform[] points;  //4 paikkaa, joiden v‰lill‰ nuzzle liikkuu

    public Transform nuzzle;    //Suutin peliobjekti
    Vector3 startPos;           //alkuper‰inen positio
                      
    AudioSource aud;            //AudioSource
    bool isActive;              //Onko kone p‰‰ll‰

    //Haetaan alku positito
    private void OnEnable()
    {
        startPos = nuzzle.localPosition;
        PersistentManager.Instance.aManager.Play("3Dtulostin", gameObject, 5);
    }

    //Aloittaa printtaamisen corutiinilla Print()
    public void StartPrinting()
    {
        if (!isActive)
        {
            isActive = true;
            StopAllCoroutines();
            StartCoroutine(Print());
        }
    }
    //Soittaa 3dtulostimen ‰‰nen ja sen ajan siirt‰‰ suutinta points[] transformejen v‰lill‰
    //Lopuksi kutsuu StopPrinting()
    IEnumerator Print()
    {
        PersistentManager.Instance.aManager.Play("3Dtulostin", gameObject, 5);

        if (!aud)
            aud = GetComponent<AudioSource>();

        for (int i = 0; i < aud.clip.length * 0.95f; i++)
        {
            LeanTween.cancel(nuzzle.gameObject);
            LeanTween.moveLocal(nuzzle.gameObject, new Vector3(Random.Range(points[2].localPosition.x, points[3].localPosition.x), Random.Range(points[0].localPosition.y, points[1].localPosition.y), startPos.z), 0.7f).setEase(LeanTweenType.easeInOutBounce);
            yield return new WaitForSecondsRealtime(1f);
        }
        StopPrinting();
    }
    //Lopettaa printtaamisen ja siirt‰‰ suuttimen alkuper‰iseen positioon
    public void StopPrinting()
    {
        StopAllCoroutines();
        isActive = false;
        LeanTween.moveLocal(nuzzle.gameObject, startPos, 0.5f).setDelay(0.6f);
        aud.Stop();
    }
    //Lopetetaan printtaaminen heti. Kutsutaan OnTriggerEnterEvent-skriptiss‰
    public void StopPrintingNow()
    {
        StopAllCoroutines();
        isActive = false;
        LeanTween.moveLocal(nuzzle.gameObject, startPos, 0f);
        LeanTween.cancel(nuzzle.gameObject);
        aud.Stop();
    }
}
