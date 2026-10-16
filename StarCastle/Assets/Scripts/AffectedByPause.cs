using UnityEngine;

public class AffectedByPause : MonoBehaviour
{
    Rigidbody2D _rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        _rb.simulated = GameManager.Instance.GameState == Utilities.GameState.Play;
    }
}
