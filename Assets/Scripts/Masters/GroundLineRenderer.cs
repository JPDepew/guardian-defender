using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundLineRenderer : MonoBehaviour
{
    public float maxYOffset = 1.5f;
    public float basePointyness = 1.5f;
    public float added = 0.7f;
    public float divisor = 6;
    public float offset = 8;
    public int layers = 3;

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
            pointPositions.Add(new Vector3(farRightPosX + 1, GetYPoint(farRightPosX + 1)));
            UpdateLineRenderer();
        }
        else if (mainCamPosX - farLeftPosX < dstFromCam - 1)
        {
            pointPositions.RemoveAt(lineRenderer.positionCount - 1);
            pointPositions.Insert(0, new Vector3(farLeftPosX - 1, GetYPoint(farLeftPosX - 1)));
            UpdateLineRenderer();
        }
    }

    void InitializeLineRenderer()
    {
        int initialPos = -(int)dstFromCam;
        lineRenderer.positionCount = size;
        for (int i = 0; i < size; i++)
        {
            float yPos = GetYPoint(initialPos);
            //sin(2 * x) + sin(pi * x)   /// 6t^5 - 15t^4 + 10t^3
            //6 * Mathf.Pow(initialPos, 5) - 15 * Mathf.Pow(initialPos, 4) + 10 * Mathf.Pow(initialPos, 3);
            lineRenderer.SetPosition(i, new Vector3(initialPos, yPos));
            pointPositions.Add(new Vector3(initialPos, yPos));
            initialPos++;
        }
    }

    void UpdateLineRenderer()
    {
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            lineRenderer.SetPosition(i, pointPositions[i]);
        }
    }

    public float GetWorldYPoint(float xPos)
    {
        return GetYPoint(xPos) + transform.position.y;
    }

    /// <summary>
    /// Calculate the y point no according to the equation, but according to the point on the rendered line
    /// </summary>
    /// <param name="xPos">X position in question</param>
    /// <returns>Y position on line renderer</returns>
    public float GetWorldYPointRounded(float xPos)
    {
        float leftmostXPoint = Mathf.Floor(xPos);
        float leftmostYPoint = GetYPoint(leftmostXPoint);
        float graphXPos = xPos - leftmostXPoint;

        float slope = GetSlope(xPos);

        float yPos = slope * graphXPos + leftmostYPoint;
        if (float.IsNaN(yPos))
        {
            return transform.position.y;
        }
        return yPos + transform.position.y;
    }

    public float GetSlope(float xPos)
    {
        float leftmostXPoint = Mathf.Floor(xPos);
        float leftmostYPoint = GetYPoint(leftmostXPoint);
        float rightmostXPoint = Mathf.Ceil(xPos);
        float rightmostYPoint = GetYPoint(rightmostXPoint);
        return (rightmostYPoint - leftmostYPoint) / (rightmostXPoint - leftmostXPoint);
    }

    public float GetYPoint(float xPos)
    {
        float yOffset = 0;
        for (int i = 0; i < layers; i++)
        {
            yOffset += GetYOffset(xPos + i * offset, basePointyness + 0.5f * i);
        }
        return yOffset;
    }

    float GetYOffset(float xPos, float pointyness)
    {
        xPos = Mathf.Abs(xPos);
        float result = Mathf.Sin(pointyness * xPos) + Mathf.Sin(Mathf.PI * xPos / divisor) + added;
        return Mathf.Clamp(result, 0, 5);
    }
}
