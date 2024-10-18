using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Voidaan k‰ytt‰‰ tietyn ‰‰nen soittamiseen OnEnable()-metodissa
//(K‰ytet‰‰n vain grillin ‰‰nen soittamiseen)
public class StartASound : MonoBehaviour
{
    public string nameOfSound;
    public GameObject targetObject;
    public int soundIndex;

    void OnEnable()
    {
        PersistentManager.Instance.aManager.Play(nameOfSound, targetObject, soundIndex);
    }
}
