using UnityEngine;
using UnityEngine.UI;

public class BetweenRoundManager : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private Button _continueButton;

    private int _intermissionId;
    private bool _canContinue;

    private void OnEnable()
    {
        _canContinue = false;
        _continueButton.interactable = false;

        _gameManager.IntermissionStarted += HandleIntermissionStarted;
        _gameManager.State += HandleStateChanged;
    }

    private void OnDisable()
    {
        if (_gameManager != null)
        {
            _gameManager.IntermissionStarted -= HandleIntermissionStarted;
            _gameManager.State -= HandleStateChanged;
        }

        _canContinue = false;
        _continueButton.interactable = false;
    }

    private void HandleIntermissionStarted(int id)
    {
        _intermissionId = id;

        // between round stuff goes here

        _canContinue = true;
        _continueButton.interactable = true;
        _continueButton.Select();
    }

    private void HandleStateChanged(Utilities.GameState state)
    {
        if (state == Utilities.GameState.Intermission)
            return;

        _canContinue = false;
        _continueButton.interactable = false;
    }

    public void ContinueButtonClick()
    {
        if (!_canContinue ||
            _gameManager.GameState != Utilities.GameState.Intermission)
        {
            return;
        }

        _canContinue = false;
        _continueButton.interactable = false;

        _gameManager.CompleteIntermission(_intermissionId);
    }
}