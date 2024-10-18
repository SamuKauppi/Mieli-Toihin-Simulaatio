using UnityEngine;

//Hallitsee partikkeli efektiä scenessä
//eli ympyrä, joka seuraa pelaajan hiirtä ja liikkumis indikaattori
public class ParticleEfeectManager : MonoBehaviour
{
    public ParticleEffectScript tempObject;     //Parikkeli efekti
    public Transform ringObject;                //Ympyrä, joka seuraa pelaajan hiirtä

    //Laittaa parikkeli efektin haluttuun kohtaan 
    public void SpawnAParticleEffect(Vector3 hitpoint)
    {
        tempObject.gameObject.SetActive(true);

        tempObject.transform.position = hitpoint;
        tempObject.StartMyEffectAgain();
    }

    //Näyttää tai kadottaa ympyräobjektin 
    public void ShowCircleOnly(Vector3 hitpoint, bool value)
    {
        ringObject.gameObject.SetActive(value);
        ringObject.position = hitpoint;
    }
}
