using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//Skripti, joka lataa halutun scenen
//Valitettavasti scenen lataaminen tapahtuu animaatioeventistä ja koska animaatioeventistä ei voi kutsua metodia paramterillä,
//Haluttu scene kustutaan ensin metodilla: NextLevelToBeLoaded(int i) ja sitten animaatioevent voi avata halutun scenen
public class Scenemanager : MonoBehaviour
{
    public int level;                   //Pidetään muistissa, mikä scene avataan
    public Slider slider;               //Lataus slider
    public Animator loadingText;        //lataus teksti
    public Animator anim;               //Latausruudun animaatio

    [SerializeField] private GameObject toggleScreenOff;
    [SerializeField] private GameObject toggleScreenOn;

    public bool isMuted;                //Katsoo onko pelin äänet muted
    public GameObject muteParents;      //Jos mute napit pitää kadottaa kokonaan (esim. taulua avatessa)

    //Referenssit
    private void Start()
    {
        anim = GetComponent<Animator>();
        slider.value = 0f;
    }

    //Vaihtaa pelin fullscreeniin
    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }
    //Mute/Unmute pelin äänet
    public void ToggleMute()
    {
        if (!isMuted)
        {
            AudioListener.volume = 0f;
        }
        else
        {
            AudioListener.volume = 1f;
        }
        isMuted = !isMuted;
    }
    //Koska vihaan itseäni, mutebuttonit tulee taulun eteen, jos on iso taulu, joten kadotetaan ne
    public void HideMuteButtons(bool value)
    {
        muteParents.SetActive(value);
    }
    //Päivittää fullscreen napit Update()-metodissa
    void Update()
    {
        if (Time.frameCount % 30 == 0)
        {
            if (Screen.fullScreen)
            {
                toggleScreenOff.SetActive(true);
                toggleScreenOn.SetActive(false);
            }
            else
            {
                toggleScreenOff.SetActive(false);
                toggleScreenOn.SetActive(true);
            }
        }
    }

    //Mikä scene avataan seuraavaksi
    public void NextLevelToBeLoaded(int i)
    {
        level = i;
    }

    //Animaatioevent, joka avaa scenen corutiinissa LoadSceneAsync(int levelIndex)
    public void LoadNextLevel()
    {
        StopAllCoroutines();
        StartCoroutine(LoadSceeneAsync(level));
        loadingText.SetBool("startDotting", true);
    }
    IEnumerator LoadSceeneAsync(int levelIndex)
    {
        slider.gameObject.SetActive(true);
        AsyncOperation operation = SceneManager.LoadSceneAsync(levelIndex);

        while (!operation.isDone)
        {
            slider.value = operation.progress;
            yield return null;
        }
        slider.value = slider.maxValue;
        anim.SetTrigger("isDone");
        loadingText.SetBool("startDotting", false);
    }
}
