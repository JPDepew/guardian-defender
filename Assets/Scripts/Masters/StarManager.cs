using System.Collections;
using UnityEngine;

public class StarManager : MonoBehaviour
{
    public GameObject star;
    public int minZPos = 50;
    public int maxZPos = 100;
    public int minYPos = -20;
    public int maxYPos = 20;
    public int starCount = 100;
    public float wrapDstMultiplier = 2;

    Constants constants;

    // Start is called before the first frame update
    void Start()
    {
        constants = Constants.instance;
        StartCoroutine(InstantiateStars());
    }

    IEnumerator InstantiateStars()
    {
        int count = 0;
        while (count < starCount)
        {
            int instantiateXAtATime = 5;

            for (int i = 0; i < instantiateXAtATime; i++)
            {
                float xPos = Random.Range(transform.position.x - constants.wrapDst * wrapDstMultiplier, transform.position.x + constants.wrapDst * wrapDstMultiplier);
                float yPos = Random.Range(minYPos, maxYPos);
                float zPos = Random.Range(minZPos, maxZPos);
                Instantiate(star, new Vector3(xPos, yPos, zPos), Quaternion.identity, transform);
                count++;
            }
            yield return null;
        }
    }
}
