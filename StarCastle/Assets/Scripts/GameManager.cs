using System;
using System.Collections;
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
            Pause?.Invoke(GameState);
        }
    }
    private InputAction pauseAction;
    public event Action<Utilities.GameState> Pause;

    [SerializeField] private GameUI _gameUI;
    [SerializeField] private PlayerBehavior _playerShip;
    [SerializeField] private Vector2 _spawnPoint;
    [SerializeField] private int player_lives = 10;
    [SerializeField] private float _respawn_time = 1;

    [SerializeField] private CastleBehavior _castle;
    [SerializeField] private Vector2 _castleSpawn = Vector2.zero;
    [SerializeField] private float _respawn_time_castle = 1;

    private PlayerBehavior current_player;
    private CastleBehavior current_castle;
    private int score = 0;
    private int difficulty = 0;

    void Awake()
    {
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
        if (GameState == Utilities.GameState.Respawning)
        {
            return;
        }
        if (pauseAction.WasPressedThisFrame())
        {
            GameState = GameState == Utilities.GameState.Play ? 
                Utilities.GameState.Pause : Utilities.GameState.Play;
        }
    }

    void StartGame()
    {
        score = 0;
        difficulty = 0;
        SpawnPlayer();
        SpawnCastle();
    }
    
    void SpawnPlayer()
    {
        current_player = Instantiate(_playerShip, _spawnPoint, Quaternion.identity);
        current_player._health.Died += SubtractLife;
        current_player._health.Damaged += LoseHealth;
        _gameUI.SetHealth(current_player._health.health);
        _gameUI.SetLives(player_lives);
    }

    void SpawnCastle()
    {
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

    void SubtractLife(HealthComponent health)
    {
        _spawnPoint = health.transform.position;
        player_lives--;

        _gameUI.SetHealth(0);
        _gameUI.SetLives(player_lives);

        StartCoroutine(RespawnPlayer());
    }

    private IEnumerator RespawnPlayer()
    {
        GameState = Utilities.GameState.Respawning;

        yield return new WaitForSecondsRealtime(_respawn_time);

        SpawnPlayer();
        TryGiveCastleTarget();

        GameState = Utilities.GameState.Play;
    }

    void LoseHealth(HealthComponent health)
    {
        _gameUI.SetHealth(health.health);
    }

    void CastleDestroyed(HealthComponent health)
    {
        score += 1000;
        _gameUI.SetScore(score);
        difficulty++;
        ClearRound();
        StartCoroutine(RespawnCastle());
    }

    void ClearRound()
    {
        ClearObjects<CastleBehavior>();
        ClearObjects<ShieldCircleBehavior>();
        ClearObjects<MissileBehavior>();
        ClearObjects<BulletBehavior>();
        ClearObjects<ExplosionComponent>();
    }

    private IEnumerator RespawnCastle()
    {
        yield return new WaitForSecondsRealtime(_respawn_time_castle);

        SpawnCastle();
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
