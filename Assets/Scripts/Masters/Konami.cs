using UnityEngine;

public class Konami : MonoBehaviour
{
    Data data;
    KeyCode[] keyCodes = new KeyCode[] {
        KeyCode.UpArrow,
        KeyCode.UpArrow,
        KeyCode.DownArrow,
        KeyCode.DownArrow,
        KeyCode.LeftArrow,
        KeyCode.RightArrow,
        KeyCode.LeftArrow,
        KeyCode.RightArrow,
        KeyCode.B,
        KeyCode.A
    };
    int keyCodeIndex = 0;

    public delegate void OnKonami();
    public static event OnKonami onKonamiEnabled;

    private void Start()
    {
        data = Data.Instance;
    }

    void Update()
    {
        if (keyCodeIndex >= keyCodes.Length && !data.konamiEnabled)
        {
            // Konami surprise!!
            onKonamiEnabled?.Invoke();
            data.konamiEnabled = true;
            keyCodeIndex = 0;
        }
        else
        {
            if (keyCodeIndex < keyCodes.Length && Input.GetKeyDown(keyCodes[keyCodeIndex]))
            {
                keyCodeIndex++;
            }
            else if (Input.anyKeyDown)
            {
                keyCodeIndex = 0;
            }
        }
    }
}
