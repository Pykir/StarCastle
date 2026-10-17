using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject PausePanel;
    private AudioSource menuAudioSource;

    void Awake()
    {
        PausePanel.SetActive(false);
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
        PausePanel.SetActive(isPaused);
        Cursor.visible = isPaused;
    }

    public void ResumeGame()
    {
        if (GameManager.Instance.GameState == Utilities.GameState.Pause) 
        {
            GameManager.Instance.GameState = Utilities.GameState.Play;
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.State -= onStateChange;
    }
}
