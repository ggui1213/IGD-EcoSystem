using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Bird : MonoBehaviour
{
    private enum BirdState
    {
        Hungry,
        Flying,
        Reproduce
    }
    private BirdState CurrentState;
    
    // Hungry Related
    [SerializeField] private float maxHungryTime = 8f;
    [SerializeField] private float maxStarveTime = 30f;
    [SerializeField] private float hungryMoveSpeed = 6f;
    private float starveTime, hungryTime;
    private Fish targetFish;
    // Move Related
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDuration = 1f;
    private float stopTimer;
    private bool targetPositionDecided;
    private Vector3 targetPosition;
    // Sex Related
    private static List<Bird> Birds = new();
    public Bird mate;
    private bool reproducedThisPeriod = false;
    [SerializeField] private int childCountEachTime = 1;
    [SerializeField] private float searchForSexMoveSpeed = 4f;
    // Lifetime control
    [SerializeField] private float maxLifeTime = 40f;
    [SerializeField] private float matureTime = 4f;
    private float lifeTime;
    private Vector3 matureScale;
    

    private void Awake()
    {
        CurrentState = BirdState.Flying;
        Birds.Add(this);
        matureScale = transform.localScale;
    }

    private void OnDestroy()
    {
        Birds.Remove(this);
    }

    private void Update()
    {
        lifeTime += Time.deltaTime;
        if (lifeTime > maxLifeTime)
            Destroy(gameObject);
        if (lifeTime <= matureTime)
            transform.localScale = Vector3.Lerp(Vector3.zero,matureScale,lifeTime / matureTime);
        if (reproducedThisPeriod && !GameManager.Instance.IsBirdReproducing)
            reproducedThisPeriod = false;
        switch (CurrentState)
        {
            case BirdState.Hungry:
                starveTime += Time.deltaTime;
                if (starveTime > maxStarveTime)
                    Destroy(gameObject);
                if (!targetFish)
                {
                    targetFish = FindClosestFish();
                }
                if (targetFish)
                {
                    Vector3 direction = (targetFish.transform.position - transform.position).normalized;
                    transform.position += direction * (hungryMoveSpeed * Time.deltaTime);
                    if (Vector3.Distance(transform.position, targetFish.transform.position) < 0.3f)
                        Eat();
                }
                break;

            case BirdState.Flying:
                hungryTime += Time.deltaTime;
                if (hungryTime >= maxHungryTime)
                {
                    CurrentState = BirdState.Hungry;
                    break;
                }
                if (!targetPositionDecided)
                {
                    targetPosition = GameManager.Instance.GetBirdRandomPosition();
                    targetPositionDecided = true;
                }
                transform.position += (targetPosition - transform.position).normalized * (moveSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                {
                    stopTimer += Time.deltaTime;
                    if (stopTimer >= stopDuration)
                    {
                        stopTimer = 0;
                        targetPosition = GameManager.Instance.GetBirdRandomPosition();
                        targetPositionDecided = true;
                    }
                }
                if (GameManager.Instance.IsBirdReproducing && !reproducedThisPeriod && lifeTime >= matureTime)
                    CurrentState = BirdState.Reproduce;
                break;

            case BirdState.Reproduce:
                if (!GameManager.Instance.IsBirdReproducing)
                {
                    CurrentState = BirdState.Flying;
                    break;
                }
                hungryTime += Time.deltaTime;
                if (hungryTime >= maxHungryTime)
                {
                    CurrentState = BirdState.Hungry;
                    break;
                }
                if (!mate && Birds.Count > 1)
                {
                    do
                    {
                        mate = Birds[Random.Range(0, Birds.Count)];
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
        Rect range = GameManager.Instance.BirdMoveRange;
        
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, range.xMin, range.xMax);
        pos.y = Mathf.Clamp(pos.y, range.yMin, range.yMax);
        transform.position = pos;
    }


    private void Eat()
    {
        Destroy(targetFish.gameObject);
        starveTime = 0f;
        hungryTime = 0f;
        targetFish = null;
        CurrentState = GameManager.Instance.IsBirdReproducing ? BirdState.Reproduce : BirdState.Flying;
        targetPositionDecided = false;
        targetPosition = GameManager.Instance.GetBirdRandomPosition();
    }
    
    private Fish FindClosestFish()
    {
        Fish closest = null;
        float minDist = float.MaxValue;
        foreach (var s in Fish.Fishes)  // Make shrimps public or create a public getter
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
            if (Birds.Count >= GameManager.Instance.MaxBird)
                break;
            Instantiate(gameObject, transform.position, transform.rotation).GetComponent<Bird>().Update();
        }

        // Reset reproduction variables on the parent.
        CurrentState = BirdState.Flying;
        targetPositionDecided = false;
        hungryTime = 0f;
        starveTime = 0f;
        mate = null;

        reproducedThisPeriod = true;
        
        targetPosition = GameManager.Instance.GetBirdRandomPosition();
    }
}
