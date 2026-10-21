using UnityEngine;

public class Screenwrap : MonoBehaviour
{
    private Camera _camera;
    private float screenWidth;
    private float screenHeight;
    private bool needWrap = false;
    private Rigidbody2D _rb;
    private bool hasRB = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _camera = Camera.main;
        hasRB = TryGetComponent<Rigidbody2D>(out _rb);
        if (_rb != null)
        {
            hasRB = true;
        }
    }

    void FixedUpdate()
    {
        if (hasRB)
        {
            Vector2 wrappedPosition = CheckScreenWrap();
            if (needWrap) _rb.position = wrappedPosition;
        }
    }

    void LateUpdate()
    {   
        if (hasRB) return;
        Vector2 wrappedPosition = CheckScreenWrap();
        if (needWrap) transform.position = new Vector3 (wrappedPosition.x, wrappedPosition.y, transform.position.z);
    }

    private Vector2 CheckScreenWrap()
    {
        needWrap = false;
        screenWidth = _camera.orthographicSize * _camera.aspect;
        screenHeight = _camera.orthographicSize;

        Vector2 pos = hasRB ? _rb.position : (Vector2) transform.position;

        if (pos.x > screenWidth)
        {
            needWrap = true;
            pos.x -= screenWidth * 2f;
        } else if (pos.x < -screenWidth) {
            needWrap = true;
            pos.x += screenWidth * 2f;
        }

        if (pos.y > screenHeight)
        {
            needWrap = true;
            pos.y -= screenHeight * 2f;
        } else if (pos.y < -screenHeight)
        {
            needWrap = true;
            pos.y += screenHeight * 2f;
        }

        return pos;
    }
}
