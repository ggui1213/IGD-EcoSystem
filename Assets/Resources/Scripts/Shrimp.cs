using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Shrimp : MonoBehaviour
{
    private enum ShrimpState
    {
        Hungry,
        Swimming,
        Reproduce
    }
    private ShrimpState CurrentState;
    
    // Hungry Related
    [SerializeField] private float maxHungryTime = 8f;
    [SerializeField] private float maxStarveTime = 5f;
    [SerializeField] private float hungryMoveSpeed = 1.5f;
    private float starveTime, hungryTime;
    private Seaweed targetSeaweed;
    // Move Related
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float stopDuration = 0.5f;
    private float stopTimer;
    private bool targetPositionDecided;
    private Vector3 targetPosition;
    // Sex Related
    public static List<Shrimp> shrimps = new();
    public Shrimp mate;
    private bool reproducedThisPeriod = false;
    [SerializeField] private int childCountEachTime = 3;
    [SerializeField] private float searchForSexMoveSpeed = 2f;
    // Lifetime control
    [SerializeField] private float maxLifeTime = 15f;
    [SerializeField] private float matureTime = 3f;
    private float lifeTime;
    private Vector3 matureScale;
    
    //getting eaten
    public static bool GetRandomShrimp(out Shrimp shrimp)
    {
        shrimp = null;
        if (shrimps.Count == 0)
            return false;
        shrimp = shrimps[Random.Range(0, shrimps.Count)];
        return true;
    }

    private void Awake()
    {
        CurrentState = ShrimpState.Swimming;
        shrimps.Add(this);
        matureScale = transform.localScale;
    }

    private void OnDestroy()
    {
        shrimps.Remove(this);
    }

    private void Update()
    {
        lifeTime += Time.deltaTime;
        if (lifeTime > maxLifeTime)
            Destroy(gameObject);
        if (lifeTime <= matureTime)
            transform.localScale = Vector3.Lerp(Vector3.zero,matureScale,lifeTime / matureTime);
        if (reproducedThisPeriod && !GameManager.Instance.IsShrimpReproducing)
            reproducedThisPeriod = false;
        switch (CurrentState)
        {
            case ShrimpState.Hungry:
                starveTime += Time.deltaTime;
                if (starveTime > maxStarveTime)
                    Destroy(gameObject);
                if (!targetSeaweed)
                {
                    if (!Seaweed.GetRandomSeaweed(out targetSeaweed))
                        break;
                }
                if (targetSeaweed)
                {
                    Vector3 direction = (targetSeaweed.transform.position - transform.position).normalized;
                    transform.position += direction * (hungryMoveSpeed * Time.deltaTime);
                    if (Vector3.Distance(transform.position, targetSeaweed.transform.position) < 0.1f)
                        Eat();
                }
                break;

            case ShrimpState.Swimming:
                hungryTime += Time.deltaTime;
                if (hungryTime >= maxHungryTime)
                {
                    CurrentState = ShrimpState.Hungry;
                    break;
                }
                if (!targetPositionDecided)
                {
                    targetPosition = GameManager.Instance.GetShrimpRandomPosition();
                    targetPositionDecided = true;
                }
                transform.position += (targetPosition - transform.position).normalized * (moveSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
                {
                    stopTimer += Time.deltaTime;
                    if (stopTimer >= stopDuration)
                    {
                        stopTimer = 0;
                        targetPosition = GameManager.Instance.GetShrimpRandomPosition();
                        targetPositionDecided = true;
                    }
                }
                if (GameManager.Instance.IsShrimpReproducing && !reproducedThisPeriod && lifeTime >= matureTime)
                    CurrentState = ShrimpState.Reproduce;
                break;

            case ShrimpState.Reproduce:
                if (!GameManager.Instance.IsShrimpReproducing)
                {
                    CurrentState = ShrimpState.Swimming;
                    break;
                }
                hungryTime += Time.deltaTime;
                if (hungryTime >= maxHungryTime)
                {
                    CurrentState = ShrimpState.Hungry;
                    break;
                }
                if (!mate && shrimps.Count > 1)
                {
                    do
                    {
                        mate = shrimps[Random.Range(0, shrimps.Count)];
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
        Rect range = GameManager.Instance.ShrimpMoveRange;

        // Since this is a 2D game, we assume z = 0.
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, range.xMin, range.xMax);
        pos.y = Mathf.Clamp(pos.y, range.yMin, range.yMax);
        transform.position = pos;
    }


    private void Eat()
    {
        starveTime = 0f;
        hungryTime = 0f;
        targetSeaweed = null;
        CurrentState = GameManager.Instance.IsShrimpReproducing ? ShrimpState.Reproduce : ShrimpState.Swimming;
        targetPositionDecided = false;
    }

    private bool hasReproduced = false; // Prevent multiple reproductions in a single encounter

    private void Reproduce()
    {
        if (hasReproduced)
            return;

        hasReproduced = true;

        Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
        for (int i = 0; i < childCountEachTime; i++)
        {
            if (shrimps.Count >= GameManager.Instance.MaxShrimps)
                break;
            Instantiate(gameObject, transform.position, transform.rotation).GetComponent<Shrimp>().reproducedThisPeriod = true;
        }

        // Reset reproduction variables on the parent.
        CurrentState = ShrimpState.Swimming;
        targetPositionDecided = false;
        hungryTime = 0f;
        starveTime = 0f;
        mate = null;

        reproducedThisPeriod = true;
    }
}
