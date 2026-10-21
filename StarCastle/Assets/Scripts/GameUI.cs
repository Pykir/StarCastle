using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameUI : MonoBehaviour
{
[SerializeField] private TMP_Text scoreText;
[SerializeField] private TMP_Text tickingScoreText;
[SerializeField] private TMP_Text healthText;
[SerializeField] private TMP_Text livesText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetScore(int score)
    {
        scoreText.text = $"Score: {score}";
    }

    public void SetHealth(int health)
    {
        healthText.text = $"Health: {health}";
    }

    public void SetLives(int lives)
    {
        livesText.text = $"Lives: {lives}";
    }

    public void SetTickingScore(int score)
    {
        tickingScoreText.text = $"Bonus: {score}";
    }
}
