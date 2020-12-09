using UnityEngine;

public class ShieldRotate : MonoBehaviour
{

    Utilities utilities;
    public float rotationSpeed = 1f;

    private void Start()
    {
        utilities = Utilities.instance;
    }

    void Update()
    {
        if (utilities?.gameState == Utilities.GameState.STOPPED) return;
        transform.Rotate(Vector3.back, rotationSpeed * Time.deltaTime);
    }
}
