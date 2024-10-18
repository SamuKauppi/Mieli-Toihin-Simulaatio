using UnityEngine;

//Singleton (DoNotDestroyOnLoad()), joka sisältää tärkeitä managereja äänestä, particle effecteihin
//Voi aina viitata laittamalla "PersistentManager.Instance.(manager minkä haluaa)...
//Lähinnä siirtää dataa scenestä toiseen ja poistaa itsensä Awake()-metodissa, jos on toinen versio tästä
public class PersistentManager : MonoBehaviour
{

    public static PersistentManager Instance { get; private set; }

    public ParticleEfeectManager pManager;
    public DialougeManager dManager;
    public DrinkScript cManager;
    public CursorScript curManager;
    public AudioManager aManager;
    public Scenemanager sManager;
    public MissionsScript missionManager;
    public AmbianceScript ambManager;
    public GuideScript gManager;
    public AreYouSureScript areManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
}
