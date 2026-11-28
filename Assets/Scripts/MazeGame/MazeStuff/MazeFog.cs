using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public sealed class MazeFog : MonoBehaviour
{
    [SerializeField] private bool startWithFog;

    public bool FogEnabled { get; private set; }

    private void OnEnable()
    {
        FogEnabled = startWithFog;
    }

    private void Update()
    {
        if (Keyboard.current?.fKey.wasPressedThisFrame == true)
            FogEnabled = !FogEnabled;
    }
}
