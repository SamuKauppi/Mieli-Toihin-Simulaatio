using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

//RoomList skriptiss‰ k‰ytett‰v‰ luokka 
//Si‰lt‰‰ gameobjektit mitk‰ on t‰ss‰ huoneessa (m‰‰ritell‰‰n editorissa)
//Ja naapureiden indeksit arraylistassa
public class ObjectsInRoom 
{
    public GameObject[] objectsInThisRoom;

    public int[] m_neighboursIndex;
}
