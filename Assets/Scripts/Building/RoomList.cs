using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Skripti, joka m‰‰ritt‰‰ mitk‰ objektit voidaan kadottaa perustuen miss‰ huoneessa pelaaja on
//Metodit EnterRoom(int roomIndex) ja ExitRoom(int roomIndex) suoritetaan OnTriggerEnterEvent-skriptiss‰ (imported-kansio), joka on m‰‰ritelty editorissa
//Toimii hyvin samalla tavalla kuin AmbianceScript(), mutta yksinkertaisempi
public class RoomList : MonoBehaviour
{
    public ObjectsInRoom[] rooms;                                                   //Huone luokka. Pit‰‰ sis‰ll‰‰n objekit, joita muokataan ja naapureiden indeksit

    public List<int> currentRooms = new List<int>();                                //Nykyisten huoneiden indeksit
    public List<int> currentNeighbours = new List<int>();                           //Nykyisten huoneiden naapureiden indeksit

    //Suorittaa CalculateObjects() varmuuden vuoksi 
    private void Start()
    {
        CalculateObjects();
    }

    //Pelaaja menee huoneeseen
    //Indeksi lis‰t‰‰n currentRooms listaan
    //Indeksin kohdalta huone listasta haetaan naapureiden indeksit
    //CalculateObjects()
    public void EnterARoom(int roomIndex) 
    {
        currentRooms.Add(roomIndex);

        currentNeighbours.AddRange(rooms[roomIndex].m_neighboursIndex);

        CalculateObjects();     //Lasketaan mitk‰ objektit tulee p‰‰lle
    }

    //Pelaaja poistuu huoneesta
    //Poistetaan indeksi ja tyhjennet‰‰n naapurit
    //Sitten lis‰t‰‰n j‰ljell‰ olevien indeksien naapurit listaan.
    //CalculateObjects()
    public void ExitARoom(int roomIndex)
    {
        currentRooms.Remove(roomIndex);

        currentNeighbours.Clear();

        if (currentRooms.Count == 0)
        {
            currentNeighbours.AddRange(rooms[rooms.Length - 1].m_neighboursIndex);
        }
        for (int i = 0; i < currentRooms.Count; i++)
        {
            currentNeighbours.AddRange(rooms[currentRooms[i]].m_neighboursIndex);
        }
        CalculateObjects();     //Lasketaan mitk‰ objektit tulee p‰‰lle
    }


    //Laskee mitk‰ objektit rooms[]-arraylistassa tulee p‰‰lle ja mitk‰ kadotetaan perustuen indekseihin
    //Jos rooms[]-indeksi on currentRooms- tai currentNeighbours-listassa, niin tuodaan objekti esiin
    //Muuten kadotetaan
    public void CalculateObjects()
    {
        //Poistetaan mahdolliset kopio numerot naapureista
        currentNeighbours.Distinct().ToList();

        //Poistetaan kaikki huone indeksit naapureita
        foreach (int id in currentRooms)
        {
            currentNeighbours.Remove(id);
        }

        //K‰yd‰‰n l‰pi huoneet
        //Jos huoneen indeksi on huoneissa = p‰‰lle
        //Jos huoneen indeksi on naapureissa = p‰‰lle
        //Muuten = pois
        for (int i = 0; i < rooms.Length; i++)
        {
            if (currentRooms.Contains(i))
            {
                for (int v = 0; v < rooms[i].objectsInThisRoom.Length; v++)
                {
                    if (!rooms[i].objectsInThisRoom[v].activeSelf)
                    {
                        rooms[i].objectsInThisRoom[v].SetActive(true);
                    }
                }
            }
            else if (currentNeighbours.Contains(i))
            {
                for (int v = 0; v < rooms[i].objectsInThisRoom.Length; v++)
                {
                    if (!rooms[i].objectsInThisRoom[v].activeSelf)
                    {
                        rooms[i].objectsInThisRoom[v].SetActive(true);
                    }
                }
            }
            else
            {
                for (int v = 0; v < rooms[i].objectsInThisRoom.Length; v++)
                {
                    if (rooms[i].objectsInThisRoom[v].activeSelf)
                    {
                        rooms[i].objectsInThisRoom[v].SetActive(false);
                    }
                }
            }
        }
    }
}
