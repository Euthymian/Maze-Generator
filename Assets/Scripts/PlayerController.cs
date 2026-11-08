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
    [SerializeField] private float stickLookSpeed = 180f; //degrees per second at full tilt
    [SerializeField] private LayerMask wallLayers;
    
    private InputActions inputActions;
    private InputAction movement;
    private InputAction look;
    private InputAction toggleWalls;
    Rigidbody rb;
    private Collider playerCollider;
    private InputAction resetToStartingPoint;
    private Vector3 spawnPosition;
    private float spawnYaw;
    
    private float yaw;
    private float pitch;
    private bool canWalkThroughWalls;
    
    public static event System.Action ResetPressed;

    private void Start()
    {
        yaw = transform.eulerAngles.y;
        spawnPosition = transform.position;
        spawnYaw = yaw;
    }
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); //get rigidbody, responsible for enabling collision with other colliders
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // so doesnt pass through walls
        playerCollider = GetComponent<Collider>();
        inputActions = new InputActions(); //create new InputActions
    }

    //called when script enabled
    private void OnEnable()
    {
        movement = inputActions.Player.Movement; //get reference to movement action
        movement.Enable();
        
        look = inputActions.Player.Look;
        look.Enable();
        
        toggleWalls = inputActions.Player.ToggleWalls;
        toggleWalls.performed += ToggleWallCollision;
        toggleWalls.Enable();
        
        //ResetToStart automatically called when Home or gamepad Start is pressed
        resetToStartingPoint = inputActions.Player.ResetToStartingPoint;
        resetToStartingPoint.performed += ResetToStart;
        resetToStartingPoint.Enable();

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
        toggleWalls.performed -= ToggleWallCollision;
        toggleWalls.Disable();
        inputActions.Player.Jump.performed -= DoJump;
        inputActions.Player.Jump.Disable();
        resetToStartingPoint.performed -= ResetToStart;
        resetToStartingPoint.Disable();
    }
    
    private void ResetToStart(InputAction.CallbackContext obj)
    {
        //move back to the start and stop any movement
        rb.linearVelocity = Vector3.zero;
        rb.position = spawnPosition;
        transform.position = spawnPosition;

        //back to the default view
        yaw = spawnYaw;
        pitch = 0f;
        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        ResetPressed?.Invoke(); //tell the enemy (and anything else listening) to reset
    }
    
    private void ToggleWallCollision(InputAction.CallbackContext obj)
    {
        canWalkThroughWalls = !canWalkThroughWalls;

        //excludeLayers makes this collider ignore anything on those layers
        if (canWalkThroughWalls)
            playerCollider.excludeLayers = wallLayers;
        else
            playerCollider.excludeLayers = 0;

        Debug.Log(canWalkThroughWalls ? "Walking through walls: ON" : "Walking through walls: OFF");
    }


    private void DoJump(InputAction.CallbackContext obj)
    {
        Debug.Log("Jump"); //called when jump performed
    }

    private void Update()
    {
        Vector2 input = look.ReadValue<Vector2>();

//mouse gives pixels moved this frame, stick gives -1..1 tilt, so they need different handling
        bool usingStick = look.activeControl?.device is Gamepad;
        Vector2 mouse = usingStick
            ? input * (stickLookSpeed * Time.deltaTime)
            : input * mouseSensitivity;
        
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