using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public AudioClip backgroundMusic;
    private AudioSource audioSource;
    public static GameManager Instance { get; private set; }
    
    [SerializeField] public Rect ShrimpMoveRange;
    [SerializeField] public Rect BirdMoveRange;
    [SerializeField] public Rect FishMoveRange;

    [SerializeField] private float ShrimpReproducePeriodLength, ShrimpReproducePeriodInterval;
    [SerializeField] private float FishReproducePeriodLength, FishReproducePeriodInterval;
    [SerializeField] private float BirdReproducePeriodLength, BirdReproducePeriodInterval;
    private float _shrimpReproduceTimer;
    private float _fishReproduceTimer, _birdReproduceTimer;
    public bool IsShrimpReproducing {get; private set;}
    public bool IsFishReproducing {get; private set;}
    public bool IsBirdReproducing {get; private set;}
    
    [SerializeField] private int maxShrimps, maxFish, maxBird;
    public int MaxShrimps => maxShrimps;
    public int MaxFish => maxFish;
    public int MaxBird => maxBird;
    
    
    private void Update()
    {
        _shrimpReproduceTimer += Time.deltaTime;
        _fishReproduceTimer += Time.deltaTime;
        _birdReproduceTimer += Time.deltaTime;
        if (_shrimpReproduceTimer >=
            (IsShrimpReproducing ? ShrimpReproducePeriodLength : ShrimpReproducePeriodInterval))
        {
            IsShrimpReproducing = !IsShrimpReproducing;
            _shrimpReproduceTimer = 0;
        }
        
        if (_fishReproduceTimer >=
            (IsFishReproducing ? FishReproducePeriodLength : FishReproducePeriodInterval))
        {
            IsFishReproducing = !IsFishReproducing;
            _fishReproduceTimer = 0;
        }
        
        if (_birdReproduceTimer >=
            (IsBirdReproducing ? BirdReproducePeriodLength : BirdReproducePeriodInterval))
        {
            IsBirdReproducing = !IsBirdReproducing;
            _birdReproduceTimer = 0;
        }
        
        
    }

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        
        Instance = this;
    }

    private void Start()
    {
        audioSource.Play();
    }

    // Called by Unity to draw gizmos in the scene view.
    private void OnDrawGizmos()
    {
        // Draw the shrimp move range in yellow.
        Gizmos.color = Color.yellow;
        DrawRect(ShrimpMoveRange);

        // Draw the bird move range in cyan.
        Gizmos.color = Color.cyan;
        DrawRect(BirdMoveRange);

        // Draw the fish move range in magenta.
        Gizmos.color = Color.magenta;
        DrawRect(FishMoveRange);
    }

    // Helper method to draw a Rect as a wireframe cube.
    private void DrawRect(Rect rect)
    {
        // Calculate the center position of the rect.
        Vector3 center = new Vector3(rect.x + rect.width / 2, rect.y + rect.height / 2, 0);
        // Create a size vector. Z is set to 0 because this is 2D.
        Vector3 size = new Vector3(rect.width, rect.height, 0);
        // Draw a wireframe cube (which appears as a rectangle in 2D).
        Gizmos.DrawWireCube(center, size);
    }

    public Vector2 GetShrimpRandomPosition()
    {
        return new Vector2(Random.Range(ShrimpMoveRange.xMin,ShrimpMoveRange.xMax),
            Random.Range(ShrimpMoveRange.yMin,ShrimpMoveRange.yMax));
    }
    
    public Vector2 GetFishRandomPosition()
    {
        return new Vector2(Random.Range(FishMoveRange.xMin,FishMoveRange.xMax),
            Random.Range(FishMoveRange.yMin,FishMoveRange.yMax));
    }
    
    public Vector2 GetBirdRandomPosition()
    {
        return new Vector2(Random.Range(BirdMoveRange.xMin,BirdMoveRange.xMax),
            Random.Range(BirdMoveRange.yMin,BirdMoveRange.yMax));
    }



}