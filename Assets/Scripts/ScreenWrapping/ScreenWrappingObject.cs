using UnityEngine;

public class ScreenWrappingObject : MonoBehaviour
{
    public float wrapDstMultiplier = 1;
    protected Constants constants;
    Data data;

    protected bool shouldWrap = true;
    protected bool konami = false;

    private Transform mainCam;
    private float wrapDst = 100;

    protected virtual void Awake()
    {
        constants = Constants.instance;
        data = Data.Instance;
        mainCam = Camera.main.transform;
        if (data && data.konamiEnabled)
        {
            KonamiAction();
        }
    }

    protected virtual void Start()
    {
        if (constants)
        {
            wrapDst = constants.wrapDst * wrapDstMultiplier;
        }
        Konami.onKonamiEnabled += KonamiAction;
    }

    private void OnDestroy()
    {
        Konami.onKonamiEnabled -= KonamiAction;
    }

    protected virtual void Update()
    {
        if (Utilities.instance.gameState == Utilities.GameState.STOPPED) return;
        if (!shouldWrap) return;
        // The camera is too far to the right
        if (mainCam.position.x - transform.position.x > wrapDst)
        {
            transform.position = new Vector3(mainCam.position.x + wrapDst - 1, transform.position.y, transform.position.z);
        }
        // Cam is too far to left
        else if (mainCam.position.x - transform.position.x < -wrapDst)
        {
            transform.position = new Vector3(mainCam.position.x - wrapDst + 1, transform.position.y, transform.position.z);
        }
    }

    public virtual void KonamiAction()
    {
        konami = true;
    }
}
