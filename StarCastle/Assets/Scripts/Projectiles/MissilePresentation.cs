using UnityEngine;

public class MissilePresentation : MonoBehaviour
{
    private MissileBehavior _missile;

    [SerializeField] private float _thrust_flash_speed = 0.02f;
    [SerializeField] private SpriteRenderer _thrustSprite;

    private bool _thrustOn = false;
    private float _thrust_elapsed = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _missile = GetComponent<MissileBehavior>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_missile.moving)
        {
           ThrustFlashing();
        } else
        {
            _thrustOn = false;
            _thrustSprite.enabled = false;
        }
    }

    private void ThrustFlashing()
    {
        if (_thrust_elapsed >= _thrust_flash_speed)
        {
            if (_thrustOn)
            {
                _thrustOn = false;
                _thrustSprite.enabled = false;
            } else
            {
                _thrustOn = true;
                _thrustSprite.enabled = true;
            }
            _thrust_elapsed -= _thrust_flash_speed;
        }
        _thrust_elapsed += Time.deltaTime;
    }
}
