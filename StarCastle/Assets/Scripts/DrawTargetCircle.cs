using UnityEngine;

public class DrawTargetCircle : MonoBehaviour
{
    private LineRenderer _line;

    [SerializeField] private int _steps = 100;
    [SerializeField] private float _radius = 1.0f;

    private float _time_step;
    private float _angle_per_step;
    private bool _drawing = false;

    private int _current_step = 0;
    private float _elapsed_time = 0f;

    void Awake()
    {
        _line = GetComponent<LineRenderer>();
        StartDrawCircle(2.0f, 3f);
    }

    public void StartDrawCircle(float time, float radius)
    {
        _time_step = time / _steps;
        _radius = radius;
        _line.positionCount = 0;
        _line.loop = false;

        _angle_per_step = 4f * Mathf.PI / (_steps - 1);
        _current_step = 0;
        _elapsed_time = 0f;

        _drawing = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_current_step >= _steps) 
        {
            _drawing = false;
            
        }
        if (!_drawing) return;
        

        _elapsed_time += Time.deltaTime;

        while ((_elapsed_time >= _time_step) && (_current_step < _steps))
        {
            _elapsed_time -= _time_step;
            _line.positionCount = _current_step + 1;
            
            float currentAngle = _current_step * _angle_per_step;
            float x = Mathf.Cos(currentAngle) * _radius;
            float y = Mathf.Sin(currentAngle) * _radius;

            _line.SetPosition(_current_step, new Vector3(x, y, 0f));

            _current_step++;
        }
        
    }

}
