using System;
using System.Collections;
using UnityEngine;

public class ShieldCircleBehavior : MonoBehaviour
{
    [SerializeField] private GameObject _shieldseg;
    private MissileBehavior _mine;
    [HideInInspector] public Rigidbody2D _mineTarget;
    
    private int _shapeSides = 6;
    private float _radius = 1.0f;
    private float _rotationSpeed = 30f;
    private float _currentScale = 0;
    private float _scaleTime = 1f;
    private float _respawnTime = 2f;
    private float _mineChance = 0.1f;
    private float _mineLifetime = 8f;
    private int _maxMineCount;
    [SerializeField] private float _mineActivationDistance = 2.5f;
    [SerializeField] private float _mineReleaseBoost = 3f;


    private int current_mine_count;
    private int _remainingSegments = 0;
    private GameObject[] segmentArray;
    private MissileBehavior[] mineArray;

    private Rigidbody2D _rb;

    public event Action<ShieldCircleBehavior, float> RegenerationRequested;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }
    
    public void Initialize(int sides, float radius, float speed, float startTime, float respawnTime, MissileBehavior mine, float mineChance, int maxMineCount, Rigidbody2D target, float mineLifetime)
    {
        _shapeSides = sides >= 3 ? sides : 3;
        _radius = radius;
        _rotationSpeed = speed;
        _scaleTime = startTime;
        _respawnTime = respawnTime;
        _mine = mine;
        _mineChance = Mathf.Clamp01(mineChance);
        _maxMineCount = maxMineCount;
        _mineTarget = target;
        _mineLifetime = mineLifetime;

        current_mine_count = 0;
        segmentArray = new GameObject[_shapeSides];
        mineArray = new MissileBehavior[_shapeSides];

        CreateShieldCircle();
        _currentScale = _scaleTime <= 0f ? 1f : 0f;
        transform.localScale = new Vector3(_currentScale, _currentScale, 1f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RotateShieldTick();

        // This is for the scaling effect at the start of the shield's creation
        if (_currentScale < 1)
        {
            _currentScale = Mathf.Clamp01(_currentScale + Time.fixedDeltaTime / _scaleTime);
            transform.localScale = new Vector3(_currentScale, _currentScale, 1f);
        } else
        {
            CheckMineProximity();
        }
    }


    // Creates shield circle
    void CreateShieldCircle()
    {
        float degrees = 0;
        float degree_Increment = 360f / _shapeSides;
        for (int i = 0; i < _shapeSides; i++)
        {
            // computes the vertices of the segment using degrees
            Vector2 current_point = new Vector2(Mathf.Cos(Mathf.Deg2Rad * degrees) * _radius, Mathf.Sin(Mathf.Deg2Rad * degrees) * _radius);
            Vector2 next_point = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (degrees + degree_Increment)) * _radius, Mathf.Sin(Mathf.Deg2Rad * (degrees + degree_Increment)) * _radius);

            // creates the segment, moves it to the middle point between the two vertices
            // rotates it to the degree halfway between the two vertices
            // then it scales it based on radius
            GameObject currentSegment = Instantiate(_shieldseg, transform, false);
            HealthComponent segmentHealth = currentSegment.GetComponent<HealthComponent>();
            _remainingSegments++;
            segmentHealth.Died += OnSegmentDied;
            currentSegment.transform.localPosition = Vector2.Lerp(current_point, next_point, 0.5f);
            currentSegment.transform.localRotation = Quaternion.Euler(0, 0 , degrees + degree_Increment / 2);
            currentSegment.transform.localScale *= _radius * Mathf.Sin(degree_Increment * Mathf.Deg2Rad);
            segmentArray[i] = currentSegment;

            TrySpawnMineAtSlot(i);

            degrees += degree_Increment;
            
        }
    }

    void TrySpawnMineAtSlot(int slot)
    {
        if (segmentArray[slot] == null) return;
        if (mineArray[slot] != null) return;
        if (current_mine_count >= _maxMineCount) return;
        if (UnityEngine.Random.value > _mineChance) return;
        
        MissileBehavior newMine = Instantiate(_mine, transform, false);
        HealthComponent mineHealth = newMine.GetComponent<HealthComponent>();
        current_mine_count++;
        mineHealth.Died += OnMineDied;
        newMine.Initialize(_mineTarget);
        newMine.transform.localPosition = segmentArray[slot].transform.localPosition;
        newMine.transform.localRotation = segmentArray[slot].transform.localRotation;
        mineArray[slot] = newMine;
    }

    private int FindSlotOfSegment(GameObject segment)
    {
        for (int i = 0; i < segmentArray.Length; i++)
        {
            if (segment == segmentArray[i])
            {
                return i;
            }
        }
        return -1;
    }

    void TryLaunchMineAtSlot(int slot)
    {
        MissileBehavior mine = mineArray[slot];
        if (mine == null) return;

        mine.GetComponent<HealthComponent>().Died -= OnMineDied;
        mineArray[slot] = null;
        current_mine_count--;

        mine.transform.SetParent(null, true);
        mine.transform.localScale = _mine.transform.localScale;

        mine.ActivateMine();
        mine.GetComponent<BulletBehavior>().Initialize(_mineTarget.transform.position - mine.transform.position, _mineLifetime, _mineReleaseBoost);
    }

    void OnSegmentDied(HealthComponent segment)
    {
        segment.Died -= OnSegmentDied;
        _remainingSegments--;

        int slot = FindSlotOfSegment(segment.gameObject);
        if (slot >= 0) TryLaunchMineAtSlot(slot);
        
        if(_remainingSegments == 0)
        {
            StartCoroutine(BeginRegeneration());
        }
    }

    void OnMineDied(HealthComponent health)
    {
        for (int i = 0; i < mineArray.Length; i++)
        {
            if (mineArray[i] == null) continue;
            if (health.gameObject == mineArray[i].gameObject)
            {
                health.Died -= OnMineDied;
                mineArray[i] = null;
                current_mine_count--;
                break;
            }
        }
    }
    

    // Turns shield
    void RotateShieldTick()
    {
        _rb.MoveRotation(_rb.rotation + _rotationSpeed * Time.fixedDeltaTime);
    }

    IEnumerator BeginRegeneration()
    {

        yield return new WaitForSeconds(_respawnTime);

        RegenerationRequested?.Invoke(this, _radius);
        Destroy(gameObject);
    }

    void CheckMineProximity()
    {
        if (_mineTarget == null) return;

        for (int i = 0; i < mineArray.Length; i++)
        {
            MissileBehavior mine = mineArray[i];
            if (mine == null) continue;

            if (Vector2.Distance(_mineTarget.position, mine.transform.position) <= _mineActivationDistance)
            {
                TryLaunchMineAtSlot(i);
            }
        }
        
    }
}
