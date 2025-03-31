using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Fish : MonoBehaviour
{
    private enum FishState
    {
        Hungry,
        Swimming,
        Reproduce
    }
    private FishState CurrentState;
    
    // Hungry Related
    [SerializeField] private float maxHungryTime = 10f;
    [SerializeField] private float maxStarveTime = 20f;
    [SerializeField] private float hungryMoveSpeed = 4f;
    private float starveTime, hungryTime;
    private Shrimp targetShrimp;
    // Move Related
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDuration = 1f;
    private float stopTimer;
    private bool targetPositionDecided;
    private Vector3 targetPosition;
    // Sex Related
    public static List<Fish> Fishes = new();
    public Fish mate;
    private bool reproducedThisPeriod = false;
    [SerializeField] private int childCountEachTime = 2;
    [SerializeField] private float searchForSexMoveSpeed = 3f;
    // Lifetime control
    [SerializeField] private float maxLifeTime = 30f;
    [SerializeField] private float matureTime = 3f;
    private float lifeTime;
    private Vector3 matureScale;
    

    private void Awake()
    {
        CurrentState = FishState.Swimming;
        Fishes.Add(this);
        matureScale = transform.localScale;
    }

    private void OnDestroy()
    {
        Fishes.Remove(this);
    }

    private void Update()
    {
        lifeTime += Time.deltaTime;
        if (lifeTime > maxLifeTime)
            Destroy(gameObject);
        if (lifeTime <= matureTime)
            transform.localScale = Vector3.Lerp(Vector3.zero,matureScale,lifeTime / matureTime);
        if (reproducedThisPeriod && !GameManager.Instance.IsFishReproducing)
            reproducedThisPeriod = false;
        switch (CurrentState)
        {
            case FishState.Hungry:
                starveTime += Time.deltaTime;
                targetShrimp = FindClosestShrimp();
                if (!targetShrimp)
                {
                        break;
                }
                if (targetShrimp)
                {
                    Vector3 direction = (targetShrimp.transform.position - transform.position).normalized;
                    transform.position += direction * (hungryMoveSpeed * Time.deltaTime);
                    if (Vector3.Distance(transform.position, targetShrimp.transform.position) < 0.3f)
                        Eat();
                }
                break;

            case FishState.Swimming:
                hungryTime += Time.deltaTime;
                if (hungryTime >= maxHungryTime)
                {
                    CurrentState = FishState.Hungry;
                    break;
                }
                if (!targetPositionDecided)
                {
                    targetPosition = GameManager.Instance.GetFishRandomPosition();
                    targetPositionDecided = true;
                }
                transform.position += (targetPosition - transform.position).normalized * (moveSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                {
                    stopTimer += Time.deltaTime;
                    if (stopTimer >= stopDuration)
                    {
                        stopTimer = 0;
                        targetPosition = GameManager.Instance.GetFishRandomPosition();
                        targetPositionDecided = true;
                    }
                }
                if (GameManager.Instance.IsFishReproducing && !reproducedThisPeriod && lifeTime >= matureTime)
                    CurrentState = FishState.Reproduce;
                break;

            case FishState.Reproduce:
                if (!GameManager.Instance.IsFishReproducing)
                {
                    CurrentState = FishState.Swimming;
                    break;
                }
                hungryTime += Time.deltaTime;
                if (hungryTime >= maxHungryTime)
                {
                    CurrentState = FishState.Hungry;
                    break;
                }
                if (!mate && Fishes.Count > 1)
                {
                    do
                    {
                        mate = Fishes[Random.Range(0, Fishes.Count)];
                    }
                    while (mate == this);
                }
                if (mate)
                {
                    transform.position += (mate.transform.position - transform.position).normalized * (searchForSexMoveSpeed * Time.deltaTime);
                    if (Vector3.Distance(transform.position, mate.transform.position) < 0.1f)
                        Reproduce();
                }
                break;

            default:
                throw new ArgumentOutOfRangeException("What the hell is happening?");
        }
        if (starveTime > maxStarveTime)
            Destroy(gameObject);

        // Clamp the shrimp's position to remain inside the defined movement range.
        ClampPosition();
    }

    private void ClampPosition()
    {
        // Get the shrimp movement range from the GameManager.
        Rect range = GameManager.Instance.FishMoveRange;
        
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, range.xMin, range.xMax);
        pos.y = Mathf.Clamp(pos.y, range.yMin, range.yMax);
        transform.position = pos;
    }


    private void Eat()
    {
        Destroy(targetShrimp.gameObject);
        starveTime = 0f;
        hungryTime = 0f;
        targetShrimp = null;
        CurrentState = GameManager.Instance.IsFishReproducing ? FishState.Reproduce : FishState.Swimming;
        targetPositionDecided = false;
    }
    
    private Shrimp FindClosestShrimp()
    {
        Shrimp closest = null;
        float minDist = float.MaxValue;
        foreach (var s in Shrimp.shrimps)  // Make shrimps public or create a public getter
        {
            if (s == this) continue;
            float dist = Vector3.Distance(transform.position, s.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = s;
            }
        }
        return closest;
    }


    private bool hasReproduced = false; // Prevent multiple reproductions in a single encounter

    // ReSharper disable Unity.PerformanceAnalysis
    private void Reproduce()
    {
        if (hasReproduced)
            return;

        hasReproduced = true;

        for (int i = 0; i < childCountEachTime; i++)
        {
            if (Fishes.Count >= GameManager.Instance.MaxFish)
                break;
            Instantiate(gameObject, transform.position, transform.rotation).GetComponent<Fish>().reproducedThisPeriod = true;
        }

        // Reset reproduction variables on the parent.
        CurrentState = FishState.Swimming;
        targetPositionDecided = false;
        hungryTime = 0f;
        starveTime = 0f;
        mate = null;

        reproducedThisPeriod = true;
    }
}
