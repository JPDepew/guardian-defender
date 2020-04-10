using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshSortingLayer : MonoBehaviour
{
    public int sortingLayer = 0;

    MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sortingLayerID = sortingLayer;
    }
}
