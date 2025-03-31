using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Seaweed : MonoBehaviour
{
    private static List<Seaweed> seaweeds = new();
    public static bool GetRandomSeaweed(out Seaweed seaweed)
    {
        seaweed = null;
        if (seaweeds.Count == 0)
            return false;
        seaweed = seaweeds[Random.Range(0, seaweeds.Count)];
        return true;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        seaweeds.Add(this);
    }

    private void OnDestroy()
    {
        seaweeds.Remove(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
