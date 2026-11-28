using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player2AI : MonoBehaviour
{
    [SerializeField] private Rigidbody ball;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float minX = -5f;
    [SerializeField] private float maxX = 5f;
    
    [SerializeField] private float centerX = 0f;
    
    [SerializeField] private float approachZSign = 1f;

    
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // Track the ball when it is coming toward Player 2.
        // Otherwise, drift back toward the center.
        float targetX = centerX;

        if (ball.linearVelocity.z * approachZSign > 0f)
            targetX = ball.position.x;

        targetX = Mathf.Clamp(targetX, minX, maxX);

        float newX = Mathf.MoveTowards(
            rb.position.x,
            targetX,
            speed * Time.fixedDeltaTime
        );

        rb.MovePosition(new Vector3(newX, rb.position.y, rb.position.z));
    }
}
