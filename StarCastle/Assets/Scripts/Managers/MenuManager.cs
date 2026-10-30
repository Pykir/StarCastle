using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject PausePanel;
    [SerializeField] private GameObject IntermissionPanel;
    [SerializeField] private GameObject GameOverPanel;

    private AudioSource menuAudioSource;

    private bool _isReloading;

    void Awake()
    {
        PausePanel.SetActive(false);
        IntermissionPanel.SetActive(false);
        GameOverPanel.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.State += onStateChange;
        onStateChange(GameManager.Instance.GameState);

        menuAudioSource = GetComponent<AudioSource>();
        menuAudioSource.ignoreListenerPause = true;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void onStateChange(Utilities.GameState gameState)
    {
        bool isPaused = gameState == Utilities.GameState.Pause;
        bool isIntermission =
            gameState == Utilities.GameState.Intermission;
        bool isGameOver =
            gameState == Utilities.GameState.GameOver;

        PausePanel.SetActive(isPaused);
        IntermissionPanel.SetActive(isIntermission);
        GameOverPanel.SetActive(isGameOver);

        Cursor.visible = isPaused || isIntermission || isGameOver;
    }

    public void ResumeGame()
    {
        if (GameManager.Instance.GameState == Utilities.GameState.Pause) 
        {
            GameManager.Instance.GameState = Utilities.GameState.Play;
        }
    }

    public void RetryRun()
    {
        if (_isReloading)
            return;

        var state = GameManager.Instance.GameState;

        if (state != Utilities.GameState.GameOver &&
            state != Utilities.GameState.Pause)
        {
            return;
        }

        _isReloading = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.State -= onStateChange;
    }
}
