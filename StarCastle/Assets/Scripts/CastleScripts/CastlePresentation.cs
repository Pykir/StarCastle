using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class CastlePresentation : MonoBehaviour
{
    private CastleBehavior _castle;
    public Rigidbody2D _target;
    
    [SerializeField] private SpriteRenderer _cannon;
    [SerializeField] private SpriteRenderer _charge_indicator;
    [SerializeField] private SpriteRenderer _cannonFire;
    [SerializeField] private float _cannonFireTime = 0.5f;
    [SerializeField, Min(0f)] private float _cannonTurnSpeed = 120f;
    [SerializeField, Min(0.1f)] private float _charge_animation_time = 1f;
    
    private float _pulse_time;
    private float _elapsed;
    private bool _charging;
    private Color _charge_indicator_base_color;
    private Color _cannon_base_color; 
    [SerializeField] Color _cannon_charged_color = new Color(1f, 0.25f, 0.05f, 1f);
    
    private AudioSource _aSource;

    [SerializeField] AudioClip missileLaunchClip;
    [SerializeField] AudioClip cannonChargeClip;
    [SerializeField] AudioClip cannonFireClip;

    void Awake()
    {
        _castle = GetComponent<CastleBehavior>();
        _aSource = GetComponent<AudioSource>();

        _cannonFire.enabled = false;
        _cannon_base_color = _cannon.color;
        _charge_indicator_base_color = _charge_indicator.color;
        _charge_indicator.color = Color.clear;
    }

    void OnEnable()
    {
        _castle.CannonCharging += HandleCannonCharging;
        _castle.CannonFired += HandleCannonFired;
        _castle.MissileFired += HandleMissileLaunch;
    }

    void OnDisable()
    {
        _castle.CannonCharging -= HandleCannonCharging;
        _castle.CannonFired -= HandleCannonFired;
        _castle.MissileFired -= HandleMissileLaunch;

        StopAllCoroutines();
        _charging = false;
        _cannon.color = _cannon_base_color;
        _cannonFire.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        TurnTowardsPlayer();

        if (_charging)
        {
            _elapsed += Time.deltaTime;
            float progress = _pulse_time <= 0f ? 1f : Mathf.Clamp01(_elapsed / _pulse_time);
            _cannon.color = Color.Lerp(_cannon_base_color, _cannon_charged_color, progress);

            _charge_indicator.color = Color.Lerp(Color.clear, _charge_indicator_base_color, progress);
            if (progress >= 1f) _charging = false;
        }
    }

    void TurnTowardsPlayer()
    {
        if (_target == null) return;
        if (_cannonFire.enabled) return; 
        
        Vector2 direction = _target.position - (Vector2)_cannon.transform.position;

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        float angle = Mathf.MoveTowardsAngle(_cannon.transform.eulerAngles.z, targetAngle, _cannonTurnSpeed * Time.deltaTime);

        _cannon.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void HandleMissileLaunch()
    {
        _aSource.PlayOneShot(missileLaunchClip);
    }

    void HandleCannonCharging(float time)
    {
        if (time > _charge_animation_time)
        {
            _pulse_time = _charge_animation_time;
            _elapsed = _charge_animation_time - time;
            StartCoroutine(PlayCannonChargeSoundIn(time - _charge_animation_time));
        } else
        {
            _pulse_time = time;
            _elapsed = 0;
            StartCoroutine(PlayCannonChargeSoundIn(_pulse_time));
        }
        
        _charging = true;
        _cannon.color = _cannon_base_color;

        
    }

    void HandleCannonFired()
    {
        _charging = false;
        _cannon.color = _cannon_base_color;
        _charge_indicator.color = Color.clear;

        _aSource.PlayOneShot(cannonFireClip);
        StartCoroutine(CannonFire());
    }

    IEnumerator CannonFire()
    {
        _cannonFire.enabled = true;

        yield return new WaitForSeconds(_cannonFireTime);

        _cannonFire.enabled = false;
    }

    IEnumerator PlayCannonChargeSoundIn(float time)
    {
        yield return new WaitForSeconds(time);

        //_aSource.PlayOneShot(cannonChargeClip);
    }
}
