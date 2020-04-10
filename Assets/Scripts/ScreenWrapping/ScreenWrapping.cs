using UnityEngine;
using System.Collections.Generic;

public class ScreenWrapping : MonoBehaviour
{
    public Transform groundTransform;
    public float distance;
    public float middleOffset = 5;

    private List<Transform> groundTransforms;
    private Transform playerTransform;
    private GameObject rightSide;
    private GameObject leftSide;
    private int listCount;

    void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
        rightSide = GameObject.FindWithTag("rightMapSide");
        leftSide = GameObject.FindWithTag("leftMapSide");

        groundTransforms = new List<Transform>();

        for (int i = 0; i < groundTransform.childCount; i++)
        {
            groundTransforms.Add(groundTransform.GetChild(i));
        }
        listCount = groundTransforms.Count - 1;
    }

    void Update()
    {
        if (playerTransform != null)
        {
            //Debug.Log("Player pos: " + playerTransform.position.x);
            //Debug.Log("Middle pos: " + (GetMiddlePosition() + middleOffset).ToString());
            if (playerTransform.position.x > GetMiddlePosition() + middleOffset)
            {
                Transform oldLeftGround = groundTransforms[0];

                Vector3 newPosition = new Vector3(groundTransforms[listCount].position.x + distance, oldLeftGround.position.y, 0);

                groundTransforms.Remove(oldLeftGround);
                groundTransforms.Add(oldLeftGround);
                oldLeftGround.position = newPosition;
            }
            else if (playerTransform.position.x < GetMiddlePosition() - middleOffset)
            {
                Transform oldRightGround = groundTransforms[listCount];

                Vector3 newPosition = new Vector3(groundTransforms[0].position.x - distance, oldRightGround.position.y, 0);

                groundTransforms.Remove(oldRightGround);
                groundTransforms.Insert(0, oldRightGround);
                oldRightGround.position = newPosition;
            }

            //if (player.transform.position.x > rightSide.transform.position.x)
            //{
            //    Vector3 newPosition = new Vector3(rightSide.transform.position.x + distance, leftSide.transform.position.y, 0);
            //    leftSide.transform.position = newPosition;
            //}
            //else if (player.transform.position.x < rightSide.transform.position.x)
            //{
            //    Vector3 newPosition = new Vector3(rightSide.transform.position.x - distance, leftSide.transform.position.y, 0);
            //    leftSide.transform.position = newPosition;
            //}
            //if (player.transform.position.x > leftSide.transform.position.x)
            //{
            //    Vector3 newPosition = new Vector3(leftSide.transform.position.x + distance, rightSide.transform.position.y, 0);
            //    rightSide.transform.position = newPosition;
            //}
            //else if (player.transform.position.x < leftSide.transform.position.x)
            //{
            //    Vector3 newPosition = new Vector3(leftSide.transform.position.x - distance, rightSide.transform.position.y, 0);
            //    rightSide.transform.position = newPosition;
            //}
        }
        else
        {
            playerTransform = GameObject.FindWithTag("Player")?.transform;
        }
    }

    void SetCorrectIndex()
    {

    }

    float GetMiddlePosition()
    {
        float dst = groundTransforms[groundTransforms.Count - 1].position.x - groundTransforms[0].position.x;

        return groundTransforms[groundTransforms.Count - 1].position.x - dst / 2;
    }
}