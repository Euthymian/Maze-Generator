using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player2Controller : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float minX = -5f;
    [SerializeField] private float maxX = 5f;
    
    private InputSystem_Actions player2Controls;
    private InputAction move;
    private Rigidbody rb;

    private void Awake()
    {
        player2Controls = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        move = player2Controls.Player2.Move;
        move.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
    }

    private void FixedUpdate()
    {
        Vector2 input = -move.ReadValue<Vector2>();
        Debug.Log(input.y);

        float newX = Mathf.Clamp(
            rb.position.x + input.y * speed * Time.fixedDeltaTime,
            minX,
            maxX
        );

        rb.MovePosition(new Vector3(newX, rb.position.y, rb.position.z));

    }
}
