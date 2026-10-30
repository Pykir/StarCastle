using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public Utilities.MainMenuState _menuState = Utilities.MainMenuState.MainMenu; 

    [SerializeField] GameObject _mainMenu;
    [SerializeField] GameObject _tutorial;
    [SerializeField] GameObject _credits;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_menuState == Utilities.MainMenuState.MainMenu)
        {
            _mainMenu.SetActive(true);
            _tutorial.SetActive(false);
            _credits.SetActive(false);
        } else if (_menuState == Utilities.MainMenuState.HowToPlay)
        {
            _mainMenu.SetActive(false);
            _tutorial.SetActive(true);
            _credits.SetActive(false);
        } else if (_menuState == Utilities.MainMenuState.Credits)
        {
            _mainMenu.SetActive(false);
            _tutorial.SetActive(false);
            _credits.SetActive(true);
        }
    }

    public void GoToTutorial()
    {
        _menuState = Utilities.MainMenuState.HowToPlay;
    }

    public void GoToCredits()
    {
        _menuState = Utilities.MainMenuState.Credits;
    }
    
    public void BackToMainMenu()
    {
        _menuState = Utilities.MainMenuState.MainMenu;
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
