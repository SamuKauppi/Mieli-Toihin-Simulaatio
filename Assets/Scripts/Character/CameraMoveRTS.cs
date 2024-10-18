using UnityEngine;

//Scripti joka vastaa kaikesta kameran liikkeestä peli scenen sisällä
public class CameraMoveRTS : MonoBehaviour
{

    public Transform player;                    //Pelaaja objekti
    CursorScript curManager;                    //Kursori
    RTS_player playerS;                         //Pelaajan skripti
    public Transform playerSubBody;             //Pelaaja objektissa oleva empty objekti (tarkoitus on estää fyysisen objektin kääntely istuessa)
    public Camera cam;                          //Kamera objekti

    public bool isSitting;                      //Katsooko istuuko pelaaja
    public bool reverseSitting;

    public Transform headBone;                  //Pelaaja hahmo objektin pää luu (käytetään fps tilan paikkana, kun pelaaja istuu)

    public Vector3 camLocalPos;                 //Kamera objektin alkusijainti, josta tulee myös RTS kamera moodin positio

    public bool isFpsCameraMoveAllowed;         //Määrittää onko FPS vai RTS moodi käytössä (katsotaan milloin hahmo piilotetaan)

    bool isHidden;                              //Katsoo, että piilotus metodej kutsutaan vain kerran

    public float mouseSensitivityX = 100f;      //X ja Y herkkyys FPS moodissa
    public float mouseSensitivityY = 100f;

    float mouseX;                               //X ja Y akselit FPS moodissa
    float mouseY;

    float xRotation = 0f;                       //FPS moodin X akselin rotaatio

    public bool cameraIsMoving;                 //Katsoo onko kamera objekti vaihtamis animaatiossa
    public bool resetRot;

    bool temporaryLock;                         //kun kamera liikku moodien välillä, muistaa lukityuksen. Estää lukitusten avaamisen oudosti

    //Tallennetaan kameran lokaali positio (camLocalPos)
    //Haetaan referenssit ja vaihdetaan moodi RTS->FPS
    private void Start()
    {
        camLocalPos = cam.transform.localPosition;
        //vaihtaa pelimoodin heti pelin alussa FPS moodin
        playerS = player.GetComponent<RTS_player>();
        curManager = PersistentManager.Instance.curManager;
        playerS.ToggleFPS();
        HideLayermask("PlayerBody");
    }

    //Updatessa suoritetaan:
    //Jos pelaaja on RTS-moodissa, kamera seuraa pelaajaa
    //FPSCamera(), jos kamera ei liiku
    //Katsoo mitä rajoitetaan, kun kamera liikkuu moodejen välillä
    private void Update()
    {
        //Laittaa kameran seuraamaan pelaajaa RTS moodissa
        if (!playerS.isFPS)
        {
            transform.position = player.position;
            cam.transform.LookAt(player.position);
        }

        //FPS kameran kääntäminen
        //Kuvakulmaa käännetään, kun hiirenvasen on pohjassa ja liikkumista ei sallita RTS_player-skriptissä
        if (isFpsCameraMoveAllowed && !cameraIsMoving || resetRot)
        {
            FPSCamera();
        }

        //Katsoo mitä asioita pitää rajoittaa, kun kamerakulma vaihtuu
        //Estää pelaajan kääntämästä kameraa, pausettaa kaikki äänet (ääni bugi) ja piilottaa hahmon mallin kamerasta fps moodissa
        if (!LeanTween.isTweening(cam.gameObject))
        {
            if (cameraIsMoving)     //Avataan kontrollit yhdellä suorituskerralla, kun kamera ei liiku
            {
                //Jos ToggleDisable kustutaan liikkeen aikana jostain muualta, muuttuja controlsWillRemainLocked muuttuu true
                //Ja Toggledisable ei kustuta täältä. Näin kotrolleja ei avata vahingossa
                if (!playerS.controlsWillRemainLocked)      
                {
                    playerS.ToggleDisable(temporaryLock, 0);
                }
                playerS.controlsWillRemainLocked = false;

                playerS.startScript.ToggleHudButtons(true);
                cameraIsMoving = false;
                if (isFpsCameraMoveAllowed)
                    HideLayermask("PlayerBody");
            }

        }
        else
        {
            if (!cameraIsMoving)    //Lukitaan kontrollit yhdellä suorituskerralla, kun kamera liikkuu
            {
                temporaryLock = playerS.disableControls;
                playerS.ToggleDisable(true, 0);
                playerS.startScript.ToggleHudButtons(false);
            }
            cameraIsMoving = true;
            cam.transform.LookAt(player.position);
        }
    }

    //Suorittaa kuvakulman kääntämisen fps-moodissa
    void FPSCamera()
    {
        if (Input.GetMouseButton(0) && (playerS.MouseHeld > 0.55f || !playerS.hitsTheGround) || resetRot)
        {
            mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivityX;
            mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivityY;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -80f, 30f);

            cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

            playerSubBody.Rotate(Vector3.up * mouseX);

            //Kursorin vaihtaminen (vaihtaa liikuvaan playerScriptissä)
            curManager.cameraIsRotating = true;
            curManager.cameraCanRotate = false;
            resetRot = false;
        }
        else
        {
            curManager.cameraCanRotate = true;
            curManager.cameraIsRotating = false;
        }
    }

    //Suorittaa kameran fyysisen paikan vaihdon RTS/FPS moodien välillä
    //Kustutaan pelaajan skriptistä
    public void MoveCameraToPos(bool rtsOrFps)
    {
        LeanTween.cancel(cam.gameObject);

        if (rtsOrFps)
        {
            //from FPS to RTS
            playerSubBody.position = player.position;
            playerSubBody.rotation = player.rotation;
            cam.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            transform.localPosition = new Vector3(0f, 0f, 0f);
            transform.parent = null;
            transform.eulerAngles = new Vector3(0f, 170.448f, 0f);
            LeanTween.moveLocal(cam.gameObject, camLocalPos, 1f).setEase(LeanTweenType.easeInOutCirc);
            isFpsCameraMoveAllowed = false;
        }
        else
        {
            //from RTS to FPS
            //Ensin laitetaan campivot pelaajasubodyn lapseksi
            //Laitetaan sen rotatio samaksi, kun pelaajasubodyn
            //Nostetaan campivot pään korkeudelle
            transform.parent = playerSubBody;

            //Katsotaan istuuko hahmo
            transform.rotation = playerSubBody.rotation;
            if(isSitting && !reverseSitting)
            {
                transform.eulerAngles = transform.eulerAngles + 180f * Vector3.up;
            }
            transform.localPosition = new Vector3(0f, 1.6f, 0f);
            cam.transform.localPosition = camLocalPos;

            //Jos pelaaja istuu
            if (isSitting)
            {
                ChangeHeadPos(true);

                if (!reverseSitting)
                {
                    LeanTween.rotate(cam.gameObject, playerSubBody.eulerAngles + 180 * Vector3.up, 1f).setEase(LeanTweenType.easeInOutCirc);
                }
            }
           
            if(!isSitting || reverseSitting)
            {
                LeanTween.rotate(cam.gameObject, playerSubBody.eulerAngles, 1f).setEase(LeanTweenType.easeInOutCirc);
            }

            //Liikutetaan ja käännetään objekti FPS tilaan
            LeanTween.moveLocal(cam.gameObject, Vector3.zero, 1f).setEase(LeanTweenType.easeInOutCirc);
            isFpsCameraMoveAllowed = true;
        }
        transform.localScale = new Vector3(1f, 1f, 1f); //Outoon bugiin liittyvä
        ShowLayermask("PlayerBody");
    }
    //Määritetään kameran positio FPS moodissa
    //Toimii eri tavalla istuessa
    public void ChangeHeadPos(bool value)
    {
        if (value)  //hahmo istuu
        {
            //Siirretään subBody pään kohdalle x ja z
            LeanTween.move(playerSubBody.gameObject, new Vector3(headBone.position.x, playerSubBody.position.y, headBone.position.z), 0f);
            //Siirretään tämä objeckti hieman alas y akselilla
            LeanTween.moveY(gameObject, playerSubBody.position.y + 1.3f, 0f);                               
        }
        else //hahmo seisoo
        {
            LeanTween.move(playerSubBody.gameObject, player.transform.position, 0f);
            LeanTween.moveY(gameObject, playerSubBody.position.y + 1.6f, 0f);
        }
    }

    //Metodit, jotka latasin netistä
    //Näyttää tietyn layermaskin kameralle
    private void ShowLayermask(string s)
    {
        if (isHidden)
        {
            cam.cullingMask |= 1 << LayerMask.NameToLayer(s);
            isHidden = false;
        }
    }
    //Piilottaa tietyn layermaskin kameralle
    private void HideLayermask(string s)
    {
        if (!isHidden)
        {
            cam.cullingMask &= ~(1 << LayerMask.NameToLayer(s));
            isHidden = true;
        }
    }
}

