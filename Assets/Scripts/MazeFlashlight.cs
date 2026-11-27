using UnityEngine;

[RequireComponent(typeof(Light))]
public sealed class MazeFlashlight : MonoBehaviour
{
    private const int CookieSize = 128;

    [SerializeField] private bool startWithFlashlight;
    [SerializeField] private Shader beamShader;

    private Light spotlight;
    private RenderTexture beamCookie;

    private void Awake()
    {
        spotlight = GetComponent<Light>();
    }

    private void OnEnable()
    {
        SetFlashlight(startWithFlashlight);
    }

    public void ToggleFlashlight()
    {
        SetFlashlight(!spotlight.enabled);
    }

    private void OnDisable()
    {
        spotlight.enabled = false;
    }

    private void OnDestroy()
    {
        if (beamCookie != null)
        {
            if (spotlight != null)
                spotlight.cookie = null;
            beamCookie.Release();
            Destroy(beamCookie);
        }
    }

    private void SetFlashlight(bool turnOn)
    {
        if (turnOn && beamCookie == null)
        {
            if (beamShader == null)
            {
                Debug.LogError("Assign the flashlight beam shader.", this);
                return;
            }

            beamCookie = new RenderTexture(CookieSize, CookieSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                name = "Flashlight Beam",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            var beamMaterial = new Material(beamShader);
            Graphics.Blit(Texture2D.whiteTexture, beamCookie, beamMaterial);
            Destroy(beamMaterial);
            spotlight.cookie = beamCookie;
        }

        spotlight.enabled = turnOn;
    }
}
