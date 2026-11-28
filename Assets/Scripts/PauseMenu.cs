using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    
    private InputActions inputActions;
    private InputAction pause;
    
    private bool isPaused = false;
    
    private void Awake()
    {
        inputActions = new InputActions();
    }

    private void OnEnable()
    {
        pause = inputActions.Player.Pause;

        pause.performed += TogglePause;
        pause.Enable();
    }

    private void OnDisable()
    {
        pause.performed -= TogglePause;
        pause.Disable();
    }

    private void TogglePause(InputAction.CallbackContext context)
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
