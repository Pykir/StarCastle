using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    private PlayerBehavior current_player;

    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text tickingScoreText;
    [SerializeField] private TMP_Text livesText;

    [SerializeField] private HUDPip dashPipPrefab;
    [SerializeField] private Transform dashPipRow;
    private List<HUDPip> dashPips = new();

    [SerializeField] private HUDPip healthPipPrefab;
    [SerializeField] private Transform healthPipRow;
    private List<HUDPip> healthPips = new();

    [SerializeField] private HUDPip shieldPipPrefab;
    [SerializeField] private Transform shieldPipRow;
    private List<HUDPip> shieldPips = new();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (current_player == null) return;

        UpdateHealthUI();
        UpdateShieldUI();
        UpdateDashPips();
    }

    public void SetNewPlayer(PlayerBehavior player)
    {
        if (player == null)
        {
            ClearPipsList(dashPips);
            ClearPipsList(healthPips);
            ClearPipsList(shieldPips);
        }
        current_player = player;
    }

    public void SetScore(int score)
    {
        scoreText.text = $"Score: {score}";
    }

    public void SetLives(int lives)
    {
        livesText.text = $"Lives: {lives}";
    }

    public void SetTickingScore(int score)
    {
        tickingScoreText.text = $"Bonus: {score}";
    }

    private void UpdateHealthUI()
    {
        ResizeSegments(healthPips, healthPipPrefab, healthPipRow, current_player._health.MaxHealth);
        int current_health = current_player._health.health;
        for(int i = 0; i < healthPips.Count; i++)
        {
            float fill;
            if (i < current_health)
            {
                fill = 1f;
            } else
            {
                fill = 0f;
            }
            healthPips[i].SetFill(fill);
        }
    }

    private void UpdateShieldUI()
    {
        ResizeSegments(shieldPips, shieldPipPrefab, shieldPipRow, current_player._health.MaxForcefields);
        int current_shields = current_player._health.ForcefieldCount;
        for(int i = 0; i < shieldPips.Count; i++)
        {
            float fill;
            if (i < current_shields)
            {
                fill = 1f;
            } else
            {
                fill = 0f;
            }
            shieldPips[i].SetFill(fill);
        }
    }

    private void UpdateDashPips()
    {
        ResizeSegments(dashPips, dashPipPrefab, dashPipRow, current_player.MaxDashCharges);
        int ready = current_player.CurrentDashCharges;
        for(int i = 0; i < dashPips.Count; i++)
        {
            float fill;
            if (i < ready)
            {
                fill = 1f;
            } else if (i == ready)
            {
                fill = current_player.DashRechargeProgress;
            } else
            {
                fill = 0f;
            }
            dashPips[i].SetFill(fill);
        }
    }

    private void ResizeSegments(List<HUDPip> segments, HUDPip prefab, Transform parent, int capacity)
    {
        while (segments.Count < capacity)
        {
            segments.Add(Instantiate(prefab, parent));
        }

        while (segments.Count > capacity)
        {
            int last = segments.Count - 1;
            HUDPip segment = segments[last];

            segments.RemoveAt(last);
            segment.gameObject.SetActive(false);
            Destroy(segment.gameObject);
        }
    }

    private void ClearPipsList(List<HUDPip> pips)
    {
        foreach (HUDPip pip in pips) 
            {
                pip.SetFill(0f);
            }
    }
}
