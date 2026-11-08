using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float mouseSensitivity = 0.1f;
    
    //create private internal references
    private InputActions inputActions;
    private InputAction movement;
    private InputAction look;
    Rigidbody rb;
    
    private float yaw;
    private float pitch;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); //get rigidbody, responsible for enabling collision with other colliders
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // so doesnt pass through wallss
        inputActions = new InputActions(); //create new InputActions
        yaw = transform.eulerAngles.y;
    }

    //called when script enabled
    private void OnEnable()
    {
        movement = inputActions.Player.Movement; //get reference to movement action
        movement.Enable();
        
        look = inputActions.Player.Look;
        look.Enable();

        //create a DoJump callback function
        //DoJump automatically called when Jump binding performed
        inputActions.Player.Jump.performed += DoJump;
        inputActions.Player.Jump.Enable();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    //called when script disabled
    private void OnDisable()
    {
        movement.Disable();
        look.Disable();
        inputActions.Player.Jump.performed -= DoJump;
        inputActions.Player.Jump.Disable();
    }

    private void DoJump(InputAction.CallbackContext obj)
    {
        Debug.Log("Jump"); //called when jump performed
    }

    private void Update()
    {
        Vector2 mouse = look.ReadValue<Vector2>() * mouseSensitivity;
        
        yaw += mouse.x;
        pitch = Mathf.Clamp(pitch - mouse.y, -80f, 80f);
        
        cameraTransform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
    
    //called every physics update
    private void FixedUpdate()
    {
        Vector2 v2 = movement.ReadValue<Vector2>(); //extract 2d input data

        //move relative to where the player is facing, not world north
        Quaternion facing = Quaternion.Euler(0f, yaw, 0f);
        Vector3 move = facing * new Vector3(v2.x, 0f, v2.y) * moveSpeed;

        //set velocity directly so speed doesn't build up, keep y so gravity still works
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }
}