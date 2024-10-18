using UnityEngine;
using UnityEngine.UI;

//Vastaa päävalikon nappien logiikasta
//Luo niille logiikkaa Start()-metodissa
//Avaa myös idle ruudun, jos peliin ei kosketa 30sec
public class StartMenuScript : MonoBehaviour
{
    public Button startGameButton;
    public Button guideButton;
    public Button endCreditsButton;

    float time;
    private void Start()
    {
        startGameButton.onClick.AddListener(() => PersistentManager.Instance.sManager.NextLevelToBeLoaded(1));
        startGameButton.onClick.AddListener(() => PersistentManager.Instance.sManager.anim.SetTrigger("fade"));

        guideButton.onClick.AddListener(() => PersistentManager.Instance.gManager.ShowGraphic(true));

        endCreditsButton.onClick.AddListener(() => PersistentManager.Instance.sManager.NextLevelToBeLoaded(3));
        endCreditsButton.onClick.AddListener(() => PersistentManager.Instance.sManager.anim.SetTrigger("fade"));

        PersistentManager.Instance.aManager.Play("linnunlaulu", PersistentManager.Instance.ambManager.ambianceSounds[0], 0);
        PersistentManager.Instance.pManager.tempObject.KillMe();
    }

    private void Update()
    {
        if (Input.anyKeyDown || Input.GetMouseButton(0))
        {
            time = 0;
        }
        else
        {
            time += Time.deltaTime;
            if (time > 60f)
            {
                PersistentManager.Instance.sManager.slider.value = 0f;
                PersistentManager.Instance.sManager.NextLevelToBeLoaded(4);
                PersistentManager.Instance.sManager.anim.SetTrigger("fade");
            }
        }
    }

}
