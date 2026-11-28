using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class ConsoleManager : MonoBehaviour
{
    [SerializeField] private GameObject consolePanel;
    [SerializeField] private TMP_InputField consoleInput;
    [SerializeField] private ScrollRect outputScroll;
    [SerializeField] private TMP_Text consoleText;
    [SerializeField] private Camera backgroundCamera;
    [SerializeField] private Color differentColor;
    [SerializeField] private GameManager gameManager;

    private InputSystem_Actions controls;
    private InputAction open;
    private InputAction close;
    private Color originalCameraColor;
    private Material backgroundMaterial;
    private bool isOpen;
    private bool isColorDifferent;

    private void Awake()
    {
        controls = new InputSystem_Actions();
        
        if (consolePanel != null)
            consolePanel.SetActive(false);
        
        if (consoleInput != null)
            consoleInput.onSubmit.AddListener(RunCommand);

        if (backgroundCamera != null)
            originalCameraColor = backgroundCamera.backgroundColor;
    }

    private void OnEnable()
    {
        open = controls.Console.Open;
        close = controls.Console.Close;
        
        open.Enable();
        close.Enable();
    }

    private void OnDisable()
    {
        open.Disable();
        close.Disable();
    }

    private void Update()
    {
        if (open != null && !isOpen && open.WasPressedThisFrame())
            OpenConsole();
        else if (close != null && isOpen && close.WasPressedThisFrame())
            CloseConsole();
    }

    private void OpenConsole()
    {
        if (consolePanel == null)
            return;
        
        isOpen = true;
        consolePanel.SetActive(true);

        if (consoleInput is not null)
        {
            consoleInput.text = "";
            consoleInput.ActivateInputField();
        }

    }
    
    private void CloseConsole()
    {
        isOpen = false;

        consoleInput?.DeactivateInputField();

        consolePanel?.SetActive(false);
    }
    
    private void RunCommand(string input)
    {
        string command = input.Trim().ToLowerInvariant();

        if (command.Length > 0)
        {
            WriteLine($"> {command}");

            switch (command)
            {
                case "bg":
                    ToggleBackground();
                    break;
                
                case "restart":
                    if (gameManager == null)
                    {
                        WriteLine("Assign the GameManager in the Inspector.");
                        break;
                    }

                    gameManager.RestartGame();
                    CloseConsole();
                    return;

                case "clear":
                    ClearOutput();
                    break;

                default:
                    WriteLine("Unknown command. Type 'bg' or 'clear'.");
                    break;
            }
        }

        if (consoleInput != null)
        {
            consoleInput.text = "";
            consoleInput.ActivateInputField();
        }

    }
    
    private void WriteLine(string message)
    {
        if (outputScroll == null ||
            outputScroll.content == null ||
            consoleText == null)
        {
            Debug.LogWarning("Assign the output Scroll View and text prefab.");
            return;
        }

        TMP_Text line = Instantiate(consoleText, outputScroll.content);
        line.text = message;

        Canvas.ForceUpdateCanvases();
        outputScroll.verticalNormalizedPosition = 0f;
    }
    
    private void ToggleBackground()
    {
        if (backgroundCamera == null)
        {
            WriteLine("Assign the Background Camera in the Inspector.");
            return;
        }

        isColorDifferent = !isColorDifferent;
        backgroundCamera.backgroundColor =
            isColorDifferent ? differentColor : originalCameraColor;

        WriteLine(isColorDifferent
            ? "Background set to red."
            : "Background restored.");
    }
    
    private void ClearOutput()
    {
        if (outputScroll == null || outputScroll.content == null)
            return;

        foreach (Transform child in outputScroll.content)
            Destroy(child.gameObject);
    }
    
    private void OnDestroy()
    {
        if (consoleInput != null)
            consoleInput.onSubmit.RemoveListener(RunCommand);

        controls?.Dispose();
    }
}
