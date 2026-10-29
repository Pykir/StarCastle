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
        current_player._health.Damaged += LoseHealth;
        current_player.InitializeStats(_runPlayerStats);
        
        _gameUI.SetHealth(current_player._health.health);
        _gameUI.SetLives(player_lives);
    }

    void SpawnCastle()
    {
        if(TESTING_DONT_SPAWN_CASTLE) return;

        current_castle = Instantiate(_castle, _castleSpawn, Quaternion.identity);
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

        _gameUI.SetHealth(0);
        _gameUI.SetLives(player_lives);

        if (player_lives == 0)
        {
            GameState = Utilities.GameState.GameOver;
            ClearRound();
            _roundTransitionInProgress = false;
            _awaitingIntermissionCompletion = false;
            return;
        }

        StartCoroutine(RespawnPlayer());
    }

    private IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(_respawn_time / 2);
        GameState = Utilities.GameState.Respawning;

        yield return new WaitForSecondsRealtime(_respawn_time / 2);

        SpawnPlayer();
        TryGiveCastleTarget();

        GameState = Utilities.GameState.Play;
    }

    void LoseHealth(HealthComponent health)
    {
        _gameUI.SetHealth(health.health);
    }

    void CastleDestroyed(HealthComponent health, bool instantRemove)
    {
        if (GameState == Utilities.GameState.GameOver ||
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

}
