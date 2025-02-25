using UnityEngine;
using UnityEngine.UI;

//Skripti, joka on lopputekstien napissa ja vastaa kaikesta logiikasta lopputeksteissä
public class EndcreditsScript : MonoBehaviour
{
    public Animator textScrollAnimator;
    public float originalSpeed;

    //Laittaa lopputekstin napeille ominaisuuksia ja määrittää originalSpeed
    void Start()
    {
        Button endButton = GetComponent<Button>();
        endButton.onClick.AddListener(() => PersistentManager.Instance.sManager.anim.SetTrigger("fade"));
        PersistentManager.Instance.sManager.NextLevelToBeLoaded(0);
        PersistentManager.Instance.curManager.DefaultMouse();
        textScrollAnimator.speed = originalSpeed;
    }

    //Nopeuttaa animaatiota, kun välilyönti on pohjassa
    //Ja palauttaa sen alkuperäiseen (originalSpeed), kun päästetään irti
    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            textScrollAnimator.speed = 1;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            textScrollAnimator.speed = originalSpeed;
        }
    }
}
