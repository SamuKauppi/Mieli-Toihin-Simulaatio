using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Avaa "oletko varma kysymyksen", ilmoitustaulun valikosta
public class AreYouSureScript : MonoBehaviour
{
    public Button yesButton;
    public TextMeshProUGUI titleText;
    private Animator m_anim;

    private void Start()
    {
        m_anim = GetComponent<Animator>();
    }

    public void SpawnMenuQuestion(int sceneToLoad, string subject)
    {
        yesButton.onClick.RemoveAllListeners();

        titleText.text = "Oletko varma, että haluat " + subject + "?"; 

        m_anim.SetTrigger("OpenOrClose");

        yesButton.onClick.AddListener(() => PersistentManager.Instance.sManager.NextLevelToBeLoaded(sceneToLoad));
        yesButton.onClick.AddListener(() => PersistentManager.Instance.sManager.GetComponent<Animator>().SetTrigger("fade"));
        yesButton.onClick.AddListener(() => PersistentManager.Instance.dManager.DisplayNextSentence());
        yesButton.onClick.AddListener(() => PersistentManager.Instance.sManager.slider.value = 0f);
        yesButton.onClick.AddListener(() => m_anim.SetTrigger("OpenOrClose"));
    }

}
