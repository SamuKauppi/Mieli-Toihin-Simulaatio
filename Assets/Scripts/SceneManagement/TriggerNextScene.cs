using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Scripti, joka vastaa alkuanimaation animaatioeventeistä ja inputeista animaation aikana
//Toimii samalla näytönsäästäjä scenen skriptinä
public class TriggerNextScene : MonoBehaviour
{

    //Referenssejä
    public DoorScript door;                 //Viittaus konesaliin johtavaan lasioveen (avataan dramaattisen efektin takia hieman myöhemmin)
    public Animator manager;                //Viittaus mustaan imageen, jonka animaatio avaa seuraavan scenen (Managers prefab)
    public Animator textObject;             //Viittaus tekstiobjektiin, jossa lukee "Voit painaa välilyöntiä ohittaaksesi aloituksen"
    public Animator textObject2;            //Viittaus tekstiobjektiin, jossa lukee "Liikuta ruutua"
    Animator m_anim;                        //Tämän objektin animaattori
    private float time;                     //Mittaa aikaa kauan animaatio on kestänyt        

    //Äänet
    AudioSource mySource;                   //Audio source, josta soitetaan puheet
    public AudioSource startSong;           //Pelin alku musiikin source. Laitetaan koodista päälle, jotta pelin alku lagia voidaan vähentää

    public AudioClip[] startSpeaks;         //Timon puhe pätkittynä
    int soundCounter;                       //Luku, joka pitää kirja,a mikä pätkä puheesta pitää laittaa soimaan seuraavaksi

    public TextMeshProUGUI subtitleText;    //Textiobjekti tekstityksille
    [TextArea(3, 10)]
    public string[] subtitles;              //Tekstitykset timon puheelle

    //Kameran kääntämiseen
    public float mouseSensitivityX = 100f;  //X ja Y herkkyys FPS moodissa
    public float mouseSensitivityY = 100f;

    float mouseX;                           //Hiiren X ja Y akselin liikkeet
    float mouseY;

    public float xRotation;                        //Kameran X rotaatio (pivotpoint ottaa likkeen suoraan mouseX)

    public bool allowedToWatch;             //Laitetaan päälle animaatio eventissä. Estää, että pelaaja ei käännä kameraa animaation alussa
    bool isSkipping;                        //Skipataanko alku animaatio

    public Transform cam;                   //Kamera. Pyöritetään X-akselilla
    public Transform camPivot;              //Kameran pivot. Pyöritetään Y-akselilla

    //Alku lagin estämiseen
    float timer;                            //Ajastin. Jos framerate ei putoa alle 45 3 sekunnin aikana, niin laitetaan animaatio pyörimään                
    float deltaTime;                        //Käytetään frameraten mittaamiseen
    float animSpeed;                        //Animaattorin alkuperäinen nopeus

    public Image StartUiImage;              //Musta Image, joka on Aloitus-scenessä. Kadotetaan, kun animaatio menee tarpeeksi pitkälle

    public bool DisableEverything;          //Tätä skriptiä käytetään myös näytönsäästäjä scenessä, jossa ei haluta suorittaa animaatiovenettejä.

    //Haetaan referenssit ja suoritetaan CheckWhenToStartGame()-corutiini
    private void Start()
    {
        m_anim = GetComponent<Animator>();
        manager = PersistentManager.Instance.sManager.anim;

        if (DisableEverything)
        {
            AudioListener.volume = 0f;
            return;
        }

        mySource = GetComponent<AudioSource>();
        animSpeed = m_anim.speed;
        PersistentManager.Instance.curManager.DefaultMouse();
        PersistentManager.Instance.curManager.isInMenu = false;
        StartCoroutine(CheckWhenToStartGame());

    }
    //Käyetään lagin vähentämiseen
    //Animaatio jatkuu vasta, kun peli pyörii tasaisella frameratella
    //Animaatio pysäytetään laittamalla sen nopeus = 0
    IEnumerator CheckWhenToStartGame()
    {
        m_anim.speed = 0;
        while (timer < 3f)
        {
            if (timer > 0.25f)
            {
                if (CheckFps() >= 25f)
                {
                    timer += Time.deltaTime;
                }
                else
                {
                    timer = 0;
                    m_anim.speed = 0;
                }
            }
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        startSong.Play();
        m_anim.speed = animSpeed;
    }
    //Palauttaa fps määrän
    float CheckFps()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        return Mathf.Ceil(fps);
    }

    //Update()-metodissa tapahtuu kuvaruunkääntminen ja skippaus
    //Välilyönti = skip
    //Hiiren vasen pohjassa = kuvakulma kääntyy hiiren mukaan
    //Hiiren vasen pois = kuvakulma kääntyy hitaasti takaisin normaaliksi
    //DisableEverything = true, niin mikä tahansa input vaihtaa päävalikkoon
    private void Update()
    {
        time += Time.deltaTime;
        //Kuvaruudun kääntämäminen
        if (allowedToWatch && !DisableEverything)
        {
            mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivityX;
            mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivityY;

            //Hiiren näppäin pohjassa = kuvakulmakääntyy
            if (Input.GetMouseButton(0))
            {
                if (LeanTween.isTweening(cam.gameObject) || LeanTween.isTweening(camPivot.gameObject))
                {
                    LeanTween.cancel(cam.gameObject);
                    LeanTween.cancel(camPivot.gameObject);
                }

                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);

                cam.localRotation = Quaternion.Euler(xRotation, 0, 0);

                camPivot.Rotate(Vector3.up * mouseX, Space.World);

                PersistentManager.Instance.curManager.cameraIsRotating = true;
                PersistentManager.Instance.curManager.cameraCanRotate = false;
            }
            //Muuten kuvakulma pikkuhiljaa menee normaaliksi
            else
            {
                PersistentManager.Instance.curManager.cameraCanRotate = true;
                PersistentManager.Instance.curManager.cameraIsRotating = false;

                if (!LeanTween.isTweening(cam.gameObject))
                {
                    if (cam.localRotation.eulerAngles.sqrMagnitude > 0.15f || cam.localRotation.eulerAngles.sqrMagnitude < -0.15f)
                    {
                        LeanTween.cancel(cam.gameObject);
                        LeanTween.rotateLocal(cam.gameObject, Vector3.zero, 5f).setEase(LeanTweenType.easeOutSine);
                    }
                    if (camPivot.localRotation.eulerAngles.sqrMagnitude > 0.15f || camPivot.localRotation.eulerAngles.sqrMagnitude < -0.15f)
                    {
                        LeanTween.cancel(camPivot.gameObject);
                        LeanTween.rotateLocal(camPivot.gameObject, Vector3.zero, 5f).setEase(LeanTweenType.easeOutSine);
                    }
                }

                if (xRotation == Mathf.Abs(xRotation) || xRotation == -360)
                {
                    xRotation = cam.localEulerAngles.x;
                }
                else
                {
                    xRotation = cam.localEulerAngles.x - 360f;
                }
            }
        }
        //Skippaus
        if (Input.GetKeyDown(KeyCode.Space) && !isSkipping && !DisableEverything)
        {
            TriggerScene(2);
            isSkipping = true;
        }

        if (DisableEverything)
        {
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            {
                m_anim.speed = 0;
                isSkipping = true;
                DisableEverything = false;
                allowedToWatch = false;
                TriggerScene(0);
            }
        }
    }

    //Animaatioevent aloittaa corutinen FadeUiAway()
    public void ShowScreen()
    {
        if (DisableEverything)
        {
            return;
        }
        StartCoroutine(FadeUiAway());
    }
    //Kadottaa alussa olevan mustan imagen pikku hiljaa
    IEnumerator FadeUiAway()
    {
        for (float i = 1; i > 0; i -= 0.01f)
        {
            StartUiImage.color = new Color(0, 0, 0, i);
            yield return null;
        }
        StartUiImage.gameObject.SetActive(false);
    }

    //Avaa seuraavan scenen animaatioeventtinä
    public void TriggerScene(int i)
    {
        if (DisableEverything)
            return;

        PersistentManager.Instance.sManager.slider.value = 0f;
        PersistentManager.Instance.sManager.NextLevelToBeLoaded(i);
        manager.SetTrigger("fade");
        if (!PersistentManager.Instance.sManager.isMuted)
        {
            AudioListener.volume = 1f;
        }
    }

    //Avaa salin lasioven animaation aikana
    public void OpenDoor()
    {
        if (DisableEverything)
            return;

        door.UnlockDoor(true);
        door.OpenOrCloseDoor(true);
    }

    //Soittaa seuraavan puheen osan alkupuheesta animaatioeventtinä
    //Kutsuu myös tekstitysten näyttämisen ShowSubtitle()
    public void PlayNextSound()
    {
        if (DisableEverything)
            return;
        if (soundCounter >= startSpeaks.Length)
        {
            return;
        }
        mySource.clip = startSpeaks[soundCounter];
        mySource.Play();
        StopCoroutine(ShowSubtitle(0f, 0));
        StartCoroutine(ShowSubtitle(mySource.clip.length, soundCounter));
        soundCounter++;
    }
    //Näyttää ja hetken päästä piilottaa subtitlen
    //Lataa tekstin perustuen i parametriin ja kauan teksti näkyy riippuu f parametristä
    IEnumerator ShowSubtitle(float f, int i)
    {
        subtitleText.text = subtitles[i];
        subtitleText.transform.parent.gameObject.SetActive(true);
        yield return new WaitForSeconds(f * 1.1f);
        subtitleText.transform.parent.gameObject.SetActive(false);
    }

    //Näyttää tekstin 1 tai 2
    public void Showtext()
    {
        if (DisableEverything)
            return;
        textObject.SetTrigger("showText");
    }
    public void ShowText2()
    {
        if (DisableEverything)
            return;
        textObject2.SetTrigger("showText");
    }

    //FadeMusicOut animaatioeventistä
    //(146.5 on animaation pituus)
    IEnumerator FadeMusicOut()
    {
        if (!DisableEverything)
        {
            if (time > 146.5f)
            {
                StopCoroutine(FadeMusicOut());
            }

            float lengthOfFade = 146.5f - time;

            float fadeOutModifier = startSong.volume / (lengthOfFade * 3);

            while (startSong.volume > 0f)
            {
                startSong.volume -= startSong.volume * fadeOutModifier;
                yield return new WaitForSecondsRealtime(0.03f);
            }
        }
    }
}
