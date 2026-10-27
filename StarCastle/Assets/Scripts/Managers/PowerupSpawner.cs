
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PowerupSpawner : MonoBehaviour
{
    [SerializeField] private PowerupBehavior _powerup;

    [SerializeField] private int _max_powerups_spawned = 3;
    [SerializeField] private float _min_spawn_interval = 8;
    [SerializeField] private float _max_spawn_interval = 15;
    [SerializeField] private float _powerup_lifetime = 10;

    [SerializeField] private LayerMask _spawn_blockers;
    [SerializeField, Min(0.01f)] private float _clearance_radius = 0.75f;
    [SerializeField, Min(1)] private int _position_attempts = 20;

    [SerializeField] private float _bounds_padding = 1f;
    private Camera _camera;
    private float _top_bound;
    private float _bottom_bound;
    private float _left_bound;
    private float _right_bound;
    
    private List<PowerupBehavior> _existing_powerups;
    private float powerupTimer = 0;
    private float _current_spawn_interval;
    
    

    void Awake()
    {
        _camera = Camera.main;
        _existing_powerups = new List<PowerupBehavior>();
        _current_spawn_interval = Random.Range(_min_spawn_interval, _max_spawn_interval);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.GameState != Utilities.GameState.Play) return;

        _existing_powerups.RemoveAll(powerup => powerup == null);

        if (_existing_powerups.Count >= _max_powerups_spawned)
        {
            powerupTimer = 0f;
            return;
        }

        powerupTimer += Time.fixedDeltaTime;

        if (powerupTimer < _current_spawn_interval) return;

        powerupTimer = 0f;
        _current_spawn_interval = Random.Range(_min_spawn_interval, _max_spawn_interval);

        if (!TryFindEmptyPosition(out Vector3 position)) return;

        PowerupBehavior powerup = Instantiate(_powerup, position, Quaternion.identity);

        powerup.Initialize(Random.Range(0, 2), _powerup_lifetime);
        _existing_powerups.Add(powerup);
    }

    private bool TryFindEmptyPosition(out Vector3 position)
    {
        position = Vector3.zero;
        CheckBounds();

        if (_left_bound >= _right_bound ||
            _bottom_bound >= _top_bound)
            return false;

        for (int i = 0; i < _position_attempts; i++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(_left_bound, _right_bound),
                Random.Range(_bottom_bound, _top_bound)
            );

            Collider2D blocker = Physics2D.OverlapCircle(
                candidate,
                _clearance_radius,
                _spawn_blockers
            );

            if (blocker != null)
                continue;

            position = new Vector3(candidate.x, candidate.y, 0f);
            return true;
        }

        return false;
    }

    public void CheckBounds()
    {
        float screenWidth = _camera.orthographicSize * _camera.aspect;
        float screenHeight = _camera.orthographicSize;

        _top_bound = _camera.transform.position.y + screenHeight - _bounds_padding;
        _bottom_bound = _camera.transform.position.y - screenHeight + _bounds_padding;
        _left_bound = _camera.transform.position.x - screenWidth + _bounds_padding;
        _right_bound = _camera.transform.position.x + screenWidth - _bounds_padding;
    }
}
