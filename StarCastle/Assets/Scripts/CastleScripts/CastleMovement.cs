using Unity.VisualScripting;
using UnityEngine;

public class CastleMovement : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Camera _camera;

    [SerializeField] private float _bounds_padding = 2f;
    [SerializeField] private float _acceleration = 5f;
    [SerializeField] private float _correction_acceleration = 8f;
    [SerializeField] private float _move_time = 3;
    [SerializeField] private float _wait_time = 10;

    private bool moving;
    private Vector2 random_direction;
    private float _elapsed_time;
    private float _top_bound;
    private float _bottom_bound;
    private float _left_bound;
    private float _right_bound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _camera = Camera.main;
        moving = false;
        _elapsed_time = 0;

        CheckBounds();
    }

    void Update()
    {
        if(moving)
        {
            if (_elapsed_time > _move_time)
            {
                moving = false;
                _elapsed_time = 0;
            }
        }

        if (!moving)
        {
            if (_elapsed_time > _wait_time)
            {
                moving = true;
                _elapsed_time = 0;
                random_direction = Random.onUnitCircle;
            }
        }

        _elapsed_time += Time.deltaTime;
    }

    void FixedUpdate()
    {
        Vector2 position = _rb.position;

        // if moving, use random_direction, else don't apply any movement
        Vector2 wanderAcceleration = moving ? random_direction * _acceleration : Vector2.zero;

        Vector2 correctionAcceleration = Vector2.zero;

        if (position.x > _right_bound)
        {
            correctionAcceleration.x = -_correction_acceleration;
            wanderAcceleration.x = 0f;

            random_direction.x = -Mathf.Abs(random_direction.x);
        }
        else if (position.x < _left_bound)
        {
            correctionAcceleration.x = _correction_acceleration;
            wanderAcceleration.x = 0f;

            random_direction.x = Mathf.Abs(random_direction.x);
        }

        if (position.y > _top_bound)
        {
            correctionAcceleration.y = -_correction_acceleration;
            wanderAcceleration.y = 0f;

            random_direction.y = -Mathf.Abs(random_direction.y);
        }
        else if (position.y < _bottom_bound)
        {
            correctionAcceleration.y = _correction_acceleration;
            wanderAcceleration.y = 0f;

            random_direction.y = Mathf.Abs(random_direction.y);
        }

        Vector2 totalAcceleration = wanderAcceleration + correctionAcceleration;

        _rb.AddForce(totalAcceleration * _rb.mass, ForceMode2D.Force);
    }

    public void CheckBounds()
    {
        float screenWidth = _camera.orthographicSize * _camera.aspect;
        float screenHeight = _camera.orthographicSize;

        _top_bound = _camera.transform.position.y + screenHeight - _bounds_padding;
        _bottom_bound = _camera.transform.position.y - screenHeight + _bounds_padding;
        _left_bound = _camera.transform.position.x - screenWidth + _bounds_padding;
        _right_bound = _camera.transform.position.x + screenWidth - _bounds_padding;
    }
}
