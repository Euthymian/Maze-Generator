using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class MazeFog : MonoBehaviour
{
    [SerializeField] private bool startWithFog;

    public bool FogEnabled { get; private set; }

    private void OnEnable()
    {
        FogEnabled = startWithFog;
    }

    public void ToggleFog()
    {
        FogEnabled = !FogEnabled;
    }
}
