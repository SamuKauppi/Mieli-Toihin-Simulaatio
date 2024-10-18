using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

//Scripti, joka vastaa kysymysnappejen järjestämisestä ja esiin tuomisesta
//(Käytetään vain alkukysymykseen)
public class QuestionScript : MonoBehaviour
{
    public Button textPrefab;                           //Prefab nappi objekti
    public Transform parent;                            //Spawnattavien objektejen parent
    List<Button> buttonList = new List<Button>();       //lista nappi objekteista                             
    public float offSet;                                //Offset reunoista
    Button temp;                                        //Tilapäinen button jota käyeteään mmm. listaan lisäämiseen
    public AloitusScripti startScript;                  //Viittaus tutoriaali scriptiin

    //Luo vaihtoehtoja
    //Ensin poistaa mahdolliset vanhat vaihtoehdot
    //sitten luo uudet vastaukset
    public void SpawnAQuestion(List<string> text)
    {
        DestroyMyButtons();
        float posOffset = (Screen.width - offSet) / text.Count;
        float currentPos = posOffset * 0.5f;

        for (int i = 0; i < text.Count; i++)
        {
            temp = Instantiate(textPrefab, new Vector2(currentPos, textPrefab.transform.position.y), Quaternion.identity, parent);
            temp.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text[i];
            currentPos += posOffset;
            buttonList.Add(temp);
            DetermineTypeOfButton(text[i]);
        }
    }

    //Metodi, joka poistaa napit 
    public void DestroyMyButtons()
    {
        for (int i = 0; i < buttonList.Count; i++)
        {
            if (buttonList[i] != null)
            {
                Destroy(buttonList[i].gameObject);
            }
        }
        buttonList.Clear();
    }

    //Määrittää mitä vaihtoehdot tekee
    //Niiden pitää olla tänne määritetty valmiiksi
    void DetermineTypeOfButton(string name)
    {
        switch (name)
        {
            case "Kahvi":
                temp.onClick.AddListener(() => startScript.NextStepWithParameter(19));
                break;
            case "Tee":
                temp.onClick.AddListener(() => startScript.NextStepWithParameter(20));
                break;
            case "Vesi":
                temp.onClick.AddListener(() => startScript.NextStepWithParameter(21));
                break;
            case "Ohjattu kierros":
                temp.onClick.AddListener(() => startScript.StartTuotorial());
                break;
            case "Vapaa tutkiminen":
                temp.onClick.AddListener(() => startScript.SkipTutorial());
                break;
            case "Katso esittely":
                temp.onClick.AddListener(() => PersistentManager.Instance.sManager.NextLevelToBeLoaded(1));
                temp.onClick.AddListener(() => PersistentManager.Instance.sManager.GetComponent<Animator>().SetTrigger("fade"));
                temp.onClick.AddListener(() => PersistentManager.Instance.dManager.DisplayNextSentence());
                temp.onClick.AddListener(() => PersistentManager.Instance.sManager.slider.value = 0f);

                break;
            default:
                break;
        }
        temp.onClick.AddListener(() => DestroyMyButtons());
    }
}
