using UnityEngine;

//Skripti, joka ilmoittaa p‰‰skriptille (AmbianceScript), kun pelaaja tulee huoneeseen tai l‰htee huoneesta 
public class AmbianceTrigger : MonoBehaviour
{
    public int myIndex;                     //T‰m‰n huoneen indeksi
    public int[] myNeighbours;              //T‰m‰n huoneen naapreiden indeksit
    public AmbianceScript ambManager;       //P‰‰skripti

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ambManager.EnterARoom(this);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ambManager.ExitARoom(this);
        }
    }
}
