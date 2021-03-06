﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data : MonoBehaviour
{
    public int score { get; set; }
    public bool konamiEnabled = false;
    public int konamiBossHealth = 50000;

    public static Data Instance
    {
        get;
        set;
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void ResetAll()
    {
        score = 0;
        konamiEnabled = false;
    }
}
