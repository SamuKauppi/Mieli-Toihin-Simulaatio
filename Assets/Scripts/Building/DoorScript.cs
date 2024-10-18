using UnityEngine;
using UnityEngine.AI;

//Vastaa ovien avaamisesta
public class DoorScript : MonoBehaviour
{
    public bool isDoorLocked;           //Onko ovi lukittu vai ei?
    public bool isDoorOpen;             //Onko ovi auki vai ei?
    public int openDirection = 1;       //Avaamis suunta (1 tai -1)
    public Transform childDoor;         //Kohde, jota käännetään (Jos null otetaan lapsi)
    public int howManyInHitBox;         //Kuinka monta hahmoa oven hitboxin sisällä on
    Vector3 startRotation;              //Oven alku rotaatio

    //Startissa haetaan mahdollinen lapsi ovi objekti, jos sitä ei ole
    void Start()
    {
        //Määritellään ovi
        //Jos se on tyhjä otetaan lapsi. Jos ei ole lasta, otetaan tämä objekti
        if (!childDoor)
        {
            int children = transform.childCount;
            if (children > 0)
            {
                childDoor = transform.GetChild(0);
            }
            else
            {
                childDoor = transform;
            }

            startRotation = childDoor.eulerAngles;
        }
    }

    //Kun hahmo koskee ovea, niin kutsutaan avtaan tai suljetaan ovi
    private void OnTriggerEnter(Collider other)
    {
        if (!transform.CompareTag("interactable"))
        {
            howManyInHitBox++;
            OpenOrCloseDoor(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!transform.CompareTag("interactable"))
        {
            howManyInHitBox--;
            OpenOrCloseDoor(false);
        }
    }


    //Oven avaamis metodi
    //jos ovi ei ole lukittu ovi avautuu tai sulkeutuu tarpeen mukaan.
    //Ovi ei sulkeudu, jos sen hitboxin sisällä on hahmoja
    public void OpenOrCloseDoor(bool open)
    {
        if (LeanTween.isTweening(childDoor.gameObject))
        {
            LeanTween.cancel(childDoor.gameObject);
        }

        if (!isDoorLocked)
        {
            if (open)
            {
                LeanTween.rotateY(childDoor.gameObject, startRotation.y + 90f * openDirection, 0.5f).setEase(LeanTweenType.easeOutCirc);
                isDoorOpen = true;
            }
            else if(howManyInHitBox < 1)
            {
                LeanTween.rotateY(childDoor.gameObject, startRotation.y, 0.5f).setEase(LeanTweenType.easeOutCirc);
                isDoorOpen = false;
            }
        }
    }


    //Poistaa tai lisää oveen mahdollisen lukituksen
    //Samalla poistaa lapsiobjektista mahdollisen NavMeshObstacle() pois päältä
    public void UnlockDoor(bool isLocked)
    {
        isDoorLocked = !isLocked;
        if (transform.GetChild(0).GetComponent<NavMeshObstacle>())
        {
            transform.GetChild(0).GetComponent<NavMeshObstacle>().enabled = !isLocked;
        }
    }

    //Avaa tai sulkee oven perustuen mikä se on jo
    public void ToggleDoor()
    {
        OpenOrCloseDoor(!isDoorOpen);
    }
}
