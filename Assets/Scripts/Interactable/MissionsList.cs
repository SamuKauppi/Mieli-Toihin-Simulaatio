using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

//T�m�n skriptin teht�v�n� on pit�� teht�v�t muistissa,
//jotta missonSkripti voi hakea teht�v�t t��lt�
//ja missionSkriptiin voi viitata persitentManagerin kautta mist� vain.
//Skripti pystyy my�s piilottamaan ja tuomaan esiin tehdyt teht�v�t ilmoitustauluvalikossa
public class MissionsList : MonoBehaviour
{
    public MissionClass[] missions;                                 //Teht�v�t

    public List<Vector3> missionPositions = new List<Vector3>();    //Teht�vien positiot

    MissionClass[] missionsFromManager;                             //Teht�v�t, jotka haetaan managerista

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

    //Piilotetaan suoritetut teht�v�t
    //eli k�yd��n kaikki teht�v�t l�pi
    //Jos se on suoritettu seuraavia teht�vi� nostetaan suoritettujen teht�vien m��r�n verran yl�sp�in
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
    //Tuodaan esiin kaikki teht�v�t ja laitetaan ne alkuper�iseen positioon
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
