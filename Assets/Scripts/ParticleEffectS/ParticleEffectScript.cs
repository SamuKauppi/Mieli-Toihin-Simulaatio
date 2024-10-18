using UnityEngine;

//Partikkeliefekti
//SetActive ja Leantween animointia
//ParticleEffectManager vastaa positiosta ja milloin tämä on päällä
public class ParticleEffectScript : MonoBehaviour
{
    public Transform arrowObject;   //Nuoli, joka pomppii ylös alas

    Vector3 startPos;               //Nuolen alku kohta
    float endPos;                   //Nuolen y loppukohta (eli kohta, jossa se koskee maahan)
    float prevPos;                  //Edellinen loppukohta

    //Aloitetaan pomppimis animaatio
    void Start()
    {
        startPos = transform.localPosition;
        startPos.y = transform.localPosition.y + 1.5f;
        arrowObject.position = startPos;
        endPos = transform.localPosition.y;
        prevPos = endPos;

        LeanTween.moveY(arrowObject.gameObject, endPos, 0.5f).setLoopPingPong().setEase(LeanTweenType.easeInQuad);
    }

    //Pelaaja koski nuolta, kadotetaan
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            gameObject.SetActive(false);
    }

    //Uusi paikka ja uudet kujeet
    //Pomppimis animaatio päälle
    public void StartMyEffectAgain()
    {
        gameObject.SetActive(true);

        endPos = transform.localPosition.y;

        if (prevPos < endPos - 0.25f || prevPos > endPos + 0.25f)
        {
            LeanTween.cancel(arrowObject.gameObject);
            startPos = transform.localPosition;
            startPos.y = transform.localPosition.y + 1.5f;
            arrowObject.position = startPos;
            prevPos = endPos;

            LeanTween.moveY(arrowObject.gameObject, endPos, 0.5f).setLoopPingPong().setEase(LeanTweenType.easeInQuad);
        }
    }

    //kadotetaan peliobjekti
    public void KillMe()
    {
        LeanTween.cancel(gameObject);
        gameObject.SetActive(false);
    }

}
