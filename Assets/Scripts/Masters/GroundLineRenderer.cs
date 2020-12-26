using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundLineRenderer : MonoBehaviour
{
    public float maxYOffset = 1.5f;

    List<Vector3> pointPositions;
    LineRenderer lineRenderer;
    Transform mainCamTransform;
    float dstFromCam;
    int size;


    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        mainCamTransform = Camera.main.transform;
        dstFromCam = Constants.instance.wrapDst;
        pointPositions = new List<Vector3>();

        size = (int)dstFromCam * 2;

        InitializeLineRenderer();
    }

    void Update()
    {
        float farRightPosX = lineRenderer.GetPosition(lineRenderer.positionCount - 1).x;
        float farLeftPosX = lineRenderer.GetPosition(0).x;
        float mainCamPosX = mainCamTransform.position.x;

        // Player is moving to right
        if (farRightPosX - mainCamPosX < dstFromCam - 1)
        {
            pointPositions.RemoveAt(0);
            pointPositions.Add(new Vector3(farRightPosX + 1, GetYOffset(farRightPosX + 1)));
            UpdateLineRenderer();
        }
        else if (mainCamPosX - farLeftPosX < dstFromCam - 1)
        {
            pointPositions.RemoveAt(lineRenderer.positionCount - 1);
            pointPositions.Insert(0, new Vector3(farLeftPosX - 1, GetYOffset(farLeftPosX - 1)));
            UpdateLineRenderer();
        }
    }

    void InitializeLineRenderer()
    {
        int initialPos = -(int)dstFromCam;
        lineRenderer.positionCount = size;
        for (int i = 0; i < size; i++)
        {
            float yOffset = GetYOffset(initialPos);//  Random.Range(0, 1.5f);
            //sin(2 * x) + sin(pi * x)   /// 6t^5 - 15t^4 + 10t^3
            //6 * Mathf.Pow(initialPos, 5) - 15 * Mathf.Pow(initialPos, 4) + 10 * Mathf.Pow(initialPos, 3);
            lineRenderer.SetPosition(i, new Vector3(initialPos, yOffset));
            pointPositions.Add(new Vector3(initialPos, yOffset));
            initialPos++;
        }
    }

    void UpdateLineRenderer()
    {
        for(int i = 0; i < lineRenderer.positionCount; i++)
        {
            lineRenderer.SetPosition(i, pointPositions[i]);
        }
    }

    float GetYOffset(float xPos)
    {
        float pointyness = 1.5f;
        xPos = Mathf.Abs(xPos);
        return Mathf.Clamp(Mathf.Sin(pointyness * xPos) + Mathf.Sin(Mathf.PI * xPos / 6) + 0.7f, 0, 5);//  Random.Range(0, 1.5f);// Random.Range(0, maxYOffset);
    }
}
