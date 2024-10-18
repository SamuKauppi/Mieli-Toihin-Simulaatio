using UnityEngine;

//Vastaa syötävien asioiden luomisesta ja poistamisesta kun hahmo haluaa juoda/syödä
public class DrinkScript : MonoBehaviour
{
    public Transform[] edibleObject;        //Ruoka objektit
    
    //Luo halutun edibleObjects[]-listasta objektin hahmon käteen
    public void SpawnADrink(Transform parent, AnimationEvents events, string s)
    {
        Transform temp;
        for (int i = 0; i < edibleObject.Length; i++)
        {
            if (edibleObject[i].name.Equals(s))
            {
                temp = Instantiate(edibleObject[i], parent.position, Quaternion.identity);
                temp.eulerAngles = new Vector3(-90f, 100f, 0f);
                events.cupInHand = temp;
            }
        }
    }

    //Tuhoaa halutun gameObjektin
    public void ShatterCup(Transform t)
    {
        Destroy(t.gameObject);
    }
}
