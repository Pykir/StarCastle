using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private Utilities.GameState _game_state;
    public Utilities.GameState GameState
    {
        get => _game_state;

        set
        {
            _game_state = value;
            Time.timeScale = value == Utilities.GameState.Play ? 1f : 0f;
            AudioListener.pause = value == Utilities.GameState.Pause;

            var playerMap = InputSystem.actions.FindActionMap("Player", throwIfNotFound: true);

            if (value == Utilities.GameState.Play)
            {
                playerMap.Enable();
            } else
            {
                playerMap.Disable();
            }

            State?.Invoke(GameState);
        }
    }
    private InputAction pauseAction;
    public event Action<Utilities.GameState> State;

    [SerializeField] private GameUI _gameUI;
    [SerializeField] private PlayerBehavior _playerShip;
    [SerializeField] private Vector2 _spawnPoint;
    [SerializeField] private int player_lives = 10;
    [SerializeField, Min(0f)] private float _respawn_time = 1;
    private PlayerStatsTracker _runPlayerStats;

    [SerializeField] private CastleBehavior _castle;
    [SerializeField] private Vector2 _castleSpawn = Vector2.zero;
    [SerializeField, Min(0f)] private float _respawn_time_castle = 1;
    [SerializeField, Min(0f)] private float _spawnClearance = 0.2f;

    private PlayerBehavior current_player;
    private CastleBehavior current_castle;
    private int score = 0;
    private int tickingScore = 0;
    [SerializeField] private int baseTickingScore = 1000;
    [SerializeField] private int tickingScoreMultiplier = 500;
    [SerializeField] private int scorePerTick = 50;
    [SerializeField] private int difficulty = 0;
    [SerializeField] private bool TESTING_DONT_SPAWN_CASTLE = false;
    [SerializeField] private bool TESTING_DIFFICULTY = false;

    private float _elapsed_time;

    private bool _roundTransitionInProgress;
    private bool _awaitingIntermissionCompletion;
    private int _intermissionID = 0;
    public event Action<int> IntermissionStarted;

    private bool _gameOverPending;

    void Awake()
    {
        _runPlayerStats = GetComponent<PlayerStatsTracker>();

        if (Instance == null)
        {
            Instance = this;
            Debug.Log("New instance initialized...");
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            Debug.Log("Duplicate instance found and deleted...");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameState = Utilities.GameState.Play;
        pauseAction = InputSystem.actions.FindAction("Pause");
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (pauseAction.WasPressedThisFrame())
        {
            if (GameState == Utilities.GameState.Play)
            {
                GameState = Utilities.GameState.Pause;
            } else if (GameState == Utilities.GameState.Pause)
            {
                GameState = Utilities.GameState.Play;
            }
        }

        if (GameState != Utilities.GameState.Play)
        {
            return;
        }
        

        _elapsed_time += Time.deltaTime;
        if (_elapsed_time > 1.0f) 
        {
            tickingScore -= scorePerTick;
            tickingScore = Mathf.Max(tickingScore, 0);
            _elapsed_time -= 1.0f;
        }
        _gameUI.SetTickingScore(tickingScore);
    }

    void StartGame()
    {
        if (!TESTING_DIFFICULTY) difficulty = 0;

        score = 0;
        _gameUI.SetScore(score);
        ResetTickingScore();

        SpawnPlayer();
        SpawnCastle();
    }
    
    void SpawnPlayer()
    {
        current_player = Instantiate(_playerShip, _spawnPoint, Quaternion.identity);
        current_player._health.Died += SubtractLife;
        current_player.InitializeStats(_runPlayerStats);
        
        _gameUI.SetNewPlayer(current_player);
        _gameUI.SetLives(player_lives);
    }

    void SpawnCastle()
    {
        if(TESTING_DONT_SPAWN_CASTLE) return;

        StartCoroutine(SpawnCastleWhenSafe());
    }

    private IEnumerator SpawnCastleWhenSafe()
    {
        float castleRadius = GetBodyRadius(_castle);
        Vector2 spawnPosition;
        // Only the body must clear the player; the shields expand after spawning.
        while (current_player == null ||
            !TryFindSpawnPosition(_castleSpawn, current_player.transform.position,
                castleRadius + GetBodyRadius(current_player) + _spawnClearance,
                castleRadius, out spawnPosition))
        {
            if (_gameOverPending || GameState == Utilities.GameState.GameOver) yield break;
            yield return null;
        }

        current_castle = Instantiate(_castle, spawnPosition, Quaternion.identity);
        current_castle.Initialize(difficulty);
        current_castle._health.Died += CastleDestroyed;
        TryGiveCastleTarget();
    }

    void TryGiveCastleTarget()
    {
        if (current_castle != null)
        {
            current_castle.Retarget(current_player);
        }
    }

    void SubtractLife(HealthComponent health, bool instantRemove)
    {
        _spawnPoint = health.transform.position;
        player_lives--;

        _gameUI.SetNewPlayer(null);
        _gameUI.SetLives(player_lives);

        if (player_lives == 0)
        {
            _gameOverPending = true;
            _roundTransitionInProgress = false;
            _awaitingIntermissionCompletion = false;

            StartCoroutine(FinishGameOver());
            return;
        }

        StartCoroutine(RespawnPlayer());
    }

    private IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(_respawn_time / 2);
        GameState = Utilities.GameState.Respawning;

        yield return new WaitForSecondsRealtime(_respawn_time / 2);

        Vector2 spawnPosition;
        float playerRadius = GetBodyRadius(_playerShip);
        while (!TryFindPlayerRespawn(playerRadius, out spawnPosition))
        {
            // Never fall back to an unsafe point if the viewport is too small.
            yield return null;
        }

        _spawnPoint = spawnPosition;
        SpawnPlayer();
        TryGiveCastleTarget();

        GameState = Utilities.GameState.Play;
    }

    private bool TryFindPlayerRespawn(float playerRadius, out Vector2 position)
    {
        Vector2 castlePosition = current_castle != null
            ? (Vector2)current_castle.transform.position : _spawnPoint;
        float exclusionRadius = current_castle != null
            ? Mathf.Max(current_castle.FullShieldRadius, GetBodyRadius(current_castle))
                + playerRadius + _spawnClearance
            : 0f;

        return TryFindSpawnPosition(_spawnPoint, castlePosition, exclusionRadius,
            playerRadius, out position);
    }

    private static float GetBodyRadius(Component body)
    {
        // Both ship prefabs use polygons. Prefab collider bounds are not reliable
        // before instantiation, so measure their scaled vertices directly.
        PolygonCollider2D polygon = body.GetComponent<PolygonCollider2D>();
        float radius = 0f;
        for (int path = 0; path < polygon.pathCount; path++)
        {
            foreach (Vector2 point in polygon.GetPath(path))
            {
                Vector2 scaledPoint = polygon.transform.TransformVector(point + polygon.offset);
                radius = Mathf.Max(radius, scaledPoint.magnitude);
            }
        }
        return radius;
    }

    private static bool TryFindSpawnPosition(Vector2 preferred, Vector2 obstacle,
        float exclusionRadius, float bodyRadius, out Vector2 position)
    {
        position = preferred;
        Camera camera = Camera.main;
        if (camera == null) return false;

        Vector2 halfSize = new Vector2(camera.orthographicSize * camera.aspect,
            camera.orthographicSize) - Vector2.one * bodyRadius;
        if (halfSize.x < 0f || halfSize.y < 0f) return false;

        Vector2 center = camera.transform.position;
        Rect bounds = Rect.MinMaxRect(center.x - halfSize.x, center.y - halfSize.y,
            center.x + halfSize.x, center.y + halfSize.y);
        return TryFindPositionInBounds(preferred, obstacle, exclusionRadius, bounds, out position);
    }

    private static bool TryFindPositionInBounds(Vector2 preferred, Vector2 obstacle,
        float exclusionRadius, Rect bounds, out Vector2 position)
    {
        position = new Vector2(Mathf.Clamp(preferred.x, bounds.xMin, bounds.xMax),
            Mathf.Clamp(preferred.y, bounds.yMin, bounds.yMax));
        float minimumDistanceSquared = exclusionRadius * exclusionRadius;
        if ((position - obstacle).sqrMagnitude >= minimumDistanceSquared) return true;

        Vector2 direction = preferred - obstacle;
        float angle = direction.sqrMagnitude > 0f ? Mathf.Atan2(direction.y, direction.x) : 0f;
        for (int i = 0; i < 32; i++)
        {
            float candidateAngle = angle + i * Mathf.PI * 2f / 32f;
            Vector2 candidate = obstacle + new Vector2(Mathf.Cos(candidateAngle),
                Mathf.Sin(candidateAngle)) * (exclusionRadius + 0.01f);
            if (candidate.x >= bounds.xMin && candidate.x <= bounds.xMax &&
                candidate.y >= bounds.yMin && candidate.y <= bounds.yMax)
            {
                position = candidate;
                return true;
            }
        }

        // The farthest point in a rectangle is always a corner. This also finds
        // narrow safe areas that the angular samples may miss.
        for (int i = 0; i < 4; i++)
        {
            Vector2 corner = new Vector2((i & 1) == 0 ? bounds.xMin : bounds.xMax,
                (i & 2) == 0 ? bounds.yMin : bounds.yMax);
            if ((corner - obstacle).sqrMagnitude >= minimumDistanceSquared)
            {
                position = corner;
                return true;
            }
        }

        return false;
    }

    void CastleDestroyed(HealthComponent health, bool instantRemove)
    {
        if (_gameOverPending || 
            GameState == Utilities.GameState.GameOver ||
            _roundTransitionInProgress ||
            current_castle == null ||
            health != current_castle._health)
        {
            return;
        }

        _roundTransitionInProgress = true;
        score += 1000;
        score += tickingScore;
        player_lives++;
        _gameUI.SetLives(player_lives);
        _gameUI.SetScore(score);
        
        StartCoroutine(TransitionToIntermission());
    }

    private IEnumerator TransitionToIntermission()
    {
        yield return new WaitForSecondsRealtime(_respawn_time_castle);

        while (_roundTransitionInProgress && GameState != Utilities.GameState.GameOver &&
            (GameState == Utilities.GameState.Pause || GameState == Utilities.GameState.Respawning || current_player == null))
        {
            yield return null;
        }

        if (GameState != Utilities.GameState.Play || !_roundTransitionInProgress) yield break;

        ClearRound();
        current_castle = null;
        BeginIntermission();
        
    }

    void BeginIntermission()
    {
        if (_awaitingIntermissionCompletion) return;
        
        _intermissionID++;
        _awaitingIntermissionCompletion = true;

        GameState = Utilities.GameState.Intermission;
        IntermissionStarted?.Invoke(_intermissionID);
    }

    public void CompleteIntermission(int ID)
    {
        if ((ID == _intermissionID) && _awaitingIntermissionCompletion && (GameState == Utilities.GameState.Intermission))
        {
            _awaitingIntermissionCompletion = false;
            _roundTransitionInProgress = false;

            difficulty++;
            
            current_player.RefreshStats(_runPlayerStats);

            ResetTickingScore();
            SpawnCastle();
            GameState = Utilities.GameState.Play;
        }
    }

    void ResetTickingScore()
    {
        _elapsed_time = 0f;
        tickingScore = baseTickingScore + (difficulty * tickingScoreMultiplier);

        _gameUI.SetTickingScore(tickingScore);
    }

    void ClearRound()
    {
        ClearObjects<CastleBehavior>();
        ClearObjects<ShieldCircleBehavior>();
        ClearObjects<MissileBehavior>();
        ClearObjects<BulletBehavior>();
        ClearObjects<TargetedBarrageBehavior>();
        ClearObjects<ExplosionComponent>();
        ClearObjects<ExplosionVisuals>();
        ClearObjects<ShrapnelComponent>();
    }


    void ClearObjects<T>() where T : Component
    {
        T[] objects = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (T current_object in objects)
        {
            if (current_object == null) continue;
            
            current_object.gameObject.SetActive(false);
            Destroy(current_object.gameObject);
        }
    }

    private IEnumerator FinishGameOver()
    {
        yield return new WaitForSeconds(_respawn_time / 2f);

        GameState = Utilities.GameState.GameOver;
        ClearRound();
    }
}
