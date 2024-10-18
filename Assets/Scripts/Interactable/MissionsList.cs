using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

//Tämän skriptin tehtävänä on pitää tehtävät muistissa,
//jotta missonSkripti voi hakea tehtävät täältä
//ja missionSkriptiin voi viitata persitentManagerin kautta mistä vain.
//Skripti pystyy myös piilottamaan ja tuomaan esiin tehdyt tehtävät ilmoitustauluvalikossa
public class MissionsList : MonoBehaviour
{
    public MissionClass[] missions;                                 //Tehtävät

    public List<Vector3> missionPositions = new List<Vector3>();    //Tehtävien positiot

    MissionClass[] missionsFromManager;                             //Tehtävät, jotka haetaan managerista

    float scrollLength;
    float defaultLength;
    public RectTransform scrollArea;

    //Haetaan missions[]-arraylistasta 2D-positiot missionPositions listaan
    private void Start()
    {
        for (int i = 0; i < missions.Length; i++)
        {
            missionPositions.Add(missions[i].mainImageSprite.rectTransform.anchoredPosition3D);
        }
        defaultLength = scrollArea.rect.height;
    }

    //Metodi, joka suoritetaan Ui-buttonista ja toimii togglena
    public void ToggleMissonVisibility(bool value)
    {
        scrollLength = defaultLength;
        if (!value)
        {
            ShowCompletedMissions();
        }
        else
        {
            HideCompletedMissions();
        }
    }

    //Piilotetaan suoritetut tehtävät
    //eli käydään kaikki tehtävät läpi
    //Jos se on suoritettu seuraavia tehtäviä nostetaan suoritettujen tehtävien määrän verran ylöspäin
    public void HideCompletedMissions()
    {
        missionsFromManager = PersistentManager.Instance.missionManager.missions;

        int wasLastoneMoved = 0;

        for (int i = 0; i < missionsFromManager.Length; i++)
        {
            if (missionsFromManager[i].currentStage >= missionsFromManager[i].maxCompletionStage)
            {
                missionsFromManager[i].mainImageSprite.gameObject.SetActive(false);
                wasLastoneMoved++;
                scrollLength -= 130f;
            }
            else
            {
                if (!missionsFromManager[i].missionNames[0].Equals("aave")) //secret ^.~7 ei liiku
                {
                    missionsFromManager[i].mainImageSprite.rectTransform.anchoredPosition = missionPositions[i - wasLastoneMoved];
                }
            }
        }
        scrollArea.sizeDelta = new Vector2(scrollArea.rect.width, scrollLength);
    }
    //Tuodaan esiin kaikki tehtävät ja laitetaan ne alkuperäiseen positioon
    public void ShowCompletedMissions()
    {
        scrollArea.sizeDelta = new Vector2(scrollArea.rect.width, scrollLength);

        missionsFromManager = PersistentManager.Instance.missionManager.missions;
        for (int i = 0; i < missionsFromManager.Length; i++)
        {
            if (!missionsFromManager[i].missionNames[0].Equals("aave")) //secret ^.~7 ei liiku
            {
                missionsFromManager[i].mainImageSprite.gameObject.SetActive(true);
                missionsFromManager[i].mainImageSprite.rectTransform.anchoredPosition = missionPositions[i];
            }
            else
            {
                if (missionsFromManager[i].currentStage >= missionsFromManager[i].maxCompletionStage)
                {
                    missionsFromManager[i].mainImageSprite.gameObject.SetActive(true);
                }
            }
        }
    }
}
