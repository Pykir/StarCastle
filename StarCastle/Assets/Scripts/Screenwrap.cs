using System.Text.RegularExpressions;
using UnityEngine;

public class Screenwrap : MonoBehaviour
{
    private Camera _camera;
    private float screenWidth;
    private float screenHeight;
    private bool needWrap = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _camera = Camera.main;
    }
    // Update is called once per frame
    void LateUpdate()
    {
        needWrap = false;
        screenWidth = _camera.orthographicSize * _camera.aspect;
        screenHeight = _camera.orthographicSize;

        Vector2 pos = transform.position;

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
        
        if (needWrap) transform.position = pos;

    }
}
