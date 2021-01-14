using UnityEngine;

public class Konami : MonoBehaviour
{
    Data data = Data.Instance;
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

    void Update()
    {
        if (keyCodeIndex >= keyCodes.Length)
        {
            // Konami surprise!!
            print("Konami surpirse");
            data.konamiEnabled = true;
        }
        else
        {
            if (Input.GetKeyDown(keyCodes[keyCodeIndex]))
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
