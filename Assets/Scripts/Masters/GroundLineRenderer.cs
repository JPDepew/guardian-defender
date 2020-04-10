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
            pointPositions.Add(new Vector3(farRightPosX + 1, GetYOffset()));
            UpdateLineRenderer();
        }
        else if (mainCamPosX - farLeftPosX < dstFromCam - 1)
        {
            pointPositions.RemoveAt(lineRenderer.positionCount - 1);
            pointPositions.Insert(0, new Vector3(farLeftPosX - 1, GetYOffset()));
            UpdateLineRenderer();
        }
    }

    void InitializeLineRenderer()
    {
        int initialPos = -(int)dstFromCam;
        lineRenderer.positionCount = size;
        for (int i = 0; i < size; i++)
        {
            float yOffset = Random.Range(0, 1.5f);
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

    float GetYOffset()
    {
        return Random.Range(0, maxYOffset);
    }
}
