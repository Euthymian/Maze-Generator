using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private const float FirstFootstepDistance = 0.16f;
    private const float FootstepSpacing = 0.85f;

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float stickLookSpeed = 180f; //degrees per second at full tilt
    [SerializeField] private LayerMask wallLayers;
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioClip wallBumpSound;
    
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
    private AudioSource movementAudio;
    private Vector3 lastFootstepPosition;
    private float distanceToFootstep;
    private MazeFlashlight flashlight;
    private BallThrower ballThrower;
    private MazeFog fog;
    private DayNightLighting dayNightLighting;
    
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
        flashlight = cameraTransform.GetComponentInChildren<MazeFlashlight>(true);
        ballThrower = cameraTransform.GetComponent<BallThrower>();
        fog = cameraTransform.GetComponent<MazeFog>();
        dayNightLighting = FindFirstObjectByType<DayNightLighting>();
        movementAudio = gameObject.AddComponent<AudioSource>();
        movementAudio.playOnAwake = false;
    }

    //called when script enabled
    private void OnEnable()
    {
        movement = inputActions.Player.Movement;
        
        look = inputActions.Player.Look;
        
        toggleWalls = inputActions.Player.ToggleWalls;
        toggleWalls.performed += ToggleWallCollision;
        
        resetToStartingPoint = inputActions.Player.ResetToStartingPoint;
        resetToStartingPoint.performed += ResetToStart;
        
        inputActions.Player.Jump.performed += DoJump;
        inputActions.Player.Enable();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        lastFootstepPosition = rb.position;
        distanceToFootstep = FirstFootstepDistance;
    }

    //called when script disabled
    private void OnDisable()
    {
        inputActions.Player.Disable();
        toggleWalls.performed -= ToggleWallCollision;
        inputActions.Player.Jump.performed -= DoJump;
        resetToStartingPoint.performed -= ResetToStart;
    }

    private void OnDestroy()
    {
        inputActions.Dispose();
    }
    
    private void ResetToStart(InputAction.CallbackContext obj)
    {
        rb.linearVelocity = Vector3.zero;
        rb.position = spawnPosition;
        transform.position = spawnPosition;
        lastFootstepPosition = spawnPosition;
        distanceToFootstep = FirstFootstepDistance;
        
        yaw = spawnYaw;
        pitch = 0f;
        cameraTransform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        ResetPressed?.Invoke();
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
        Debug.Log("Jump");
    }

    private void Update()
    {
        Vector2 input = look.ReadValue<Vector2>();

        bool usingStick = look.activeControl?.device is Gamepad;
        Vector2 mouse = usingStick
            ? input * (stickLookSpeed * Time.deltaTime)
            : input * mouseSensitivity;
        
        yaw += mouse.x;
        pitch = Mathf.Clamp(pitch - mouse.y, -80f, 80f);
        
        cameraTransform.localRotation = Quaternion.Euler(pitch, yaw, 0f);

        var actions = inputActions.Player;
        if (actions.Flashlight.WasPerformedThisFrame() && flashlight != null && flashlight.isActiveAndEnabled)
            flashlight.ToggleFlashlight();
        if (actions.ThrowBall.WasPerformedThisFrame() && ballThrower != null && ballThrower.isActiveAndEnabled)
            ballThrower.ThrowBall();
        if (actions.Fog.WasPerformedThisFrame() && fog != null && fog.isActiveAndEnabled)
            fog.ToggleFog();
        if (actions.DayNight.WasPerformedThisFrame() && dayNightLighting != null && dayNightLighting.isActiveAndEnabled)
            dayNightLighting.ToggleDayNight();
    }
    
    //called every physics update
    private void FixedUpdate()
    {
        Vector2 v2 = movement.ReadValue<Vector2>();
        PlayFootsteps(v2.sqrMagnitude > 0.01f);

        Quaternion facing = Quaternion.Euler(0f, yaw, 0f);
        Vector3 move = facing * new Vector3(v2.x, 0f, v2.y) * moveSpeed;

        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }

    private void PlayFootsteps(bool moving)
    {
        Vector3 displacement = rb.position - lastFootstepPosition;
        lastFootstepPosition = rb.position;
        displacement.y = 0f;

        Bounds bounds = playerCollider.bounds;
        if (!moving || displacement.sqrMagnitude < 0.000001f ||
            !Physics.Raycast(bounds.center, Vector3.down, bounds.extents.y + 0.06f,
                             ~wallLayers.value, QueryTriggerInteraction.Ignore))
        {
            distanceToFootstep = FirstFootstepDistance;
            return;
        }

        distanceToFootstep -= displacement.magnitude;
        if (distanceToFootstep <= 0f)
        {
            movementAudio.PlayOneShot(footstepSound, 0.7f);
            distanceToFootstep += FootstepSpacing;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isActiveAndEnabled && (wallLayers.value & (1 << collision.gameObject.layer)) != 0)
            movementAudio.PlayOneShot(wallBumpSound, 0.85f);
    }
}
