using UnityEngine;

public class MissileBehavior : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Rigidbody2D _target_rb;
    [SerializeField] private float _max_speed = 9f;
    [SerializeField] private float _forward_acc = 300f;
    [SerializeField] private float _torque = 20f;
    [SerializeField] private float _max_angular_speed = 180;
    [SerializeField] private float heading_gain = 4f;
    [SerializeField] private float steering_gain = 0.2f;
    [SerializeField] private float _responseTime = 0.25f;
    [SerializeField] private bool _isMine = false;

    private bool attached = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_isMine)
        {
            attached = true;
            _rb.simulated = false;
            GetComponent<Screenwrap>().enabled = false;
        } 
    }

    public void Initialize(Rigidbody2D target)
    {
        _target_rb = target;
    }

    public void ActivateMine()
    {
        attached = false;
        _rb.simulated = true;
        GetComponent<Screenwrap>().enabled = true;
    }

    void FixedUpdate()
    {
        if (_target_rb == null) return;
        if (attached) return;

        // this is what would send the missile straight at target
        Vector2 requestedVelocity = (_target_rb.position - _rb.position).normalized * _max_speed;

        // this is the acceleration needed to push the missile to that velocity over a certain time
        Vector2 requestedAcceleration = (requestedVelocity - _rb.linearVelocity) / _responseTime;


        // this is how fast it wants to turn
        float requestAngle = Mathf.Clamp(Vector2.SignedAngle(transform.right, requestedAcceleration) * heading_gain, -_max_angular_speed, _max_angular_speed);
        
        // this is how much force to push it towards that speed
        float finalTorque = Mathf.Clamp((requestAngle - _rb.angularVelocity) * steering_gain, -_torque, _torque);

        _rb.AddTorque(finalTorque, ForceMode2D.Force);

        // this is the actual possible acceleration due to the direction of the missile
        // projects the wanted acceleration vector onto the current heading, and only thrusts by the amount that is in both directions
        float possibleAcceleration = Vector2.Dot(requestedAcceleration, (Vector2)transform.right);
        float thrustForce = Mathf.Clamp(possibleAcceleration * _rb.mass, 0f, _forward_acc);

        _rb.AddForce(transform.right * thrustForce, ForceMode2D.Force);
    }
}
