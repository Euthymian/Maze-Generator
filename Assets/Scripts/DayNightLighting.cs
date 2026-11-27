using UnityEngine;

[RequireComponent(typeof(Light))]
public sealed class DayNightLighting : MonoBehaviour
{
    private static readonly int NightBlend = Shader.PropertyToID("_NightBlend");
    private static readonly int SunDirection = Shader.PropertyToID("_SunDirection");
    private static readonly int MoonDirection = Shader.PropertyToID("_MoonDirection");

    [SerializeField] private bool startInNight;
    [SerializeField, Min(0f)] private float transitionDuration = 0.8f;
    [SerializeField] private Color nightSkyLight = new Color(0.38f, 0.46f, 0.60f);
    [SerializeField] private Color nightHorizonLight = new Color(0.29f, 0.34f, 0.44f);
    [SerializeField] private Color nightGroundLight = new Color(0.16f, 0.19f, 0.28f);
    [SerializeField] private Color moonlightColor = new Color(0.72f, 0.83f, 1f);
    [SerializeField] private Vector3 moonlightRotation = new Vector3(42f, 36f, 0f);
    [SerializeField, Range(0f, 1f)] private float moonlightIntensity = 0.21f;
    [SerializeField, Range(0f, 1f)] private float nightReflectionMultiplier = 0.35f;

    private Light directionalLight;
    private Material originalSkybox;
    private Material skybox;
    public bool IsNight { get; private set; }
    private float nightProgress;
    private Color dayAmbientSkyColor;
    private Color dayAmbientEquatorColor;
    private Color dayAmbientGroundColor;
    private Color dayLightColor;
    private float dayLightIntensity;
    private float dayReflectionIntensity;
    private Quaternion daylightRotation;
    private Quaternion nightRotation;

    private void OnEnable()
    {
        originalSkybox = RenderSettings.skybox;
        if (originalSkybox == null || !originalSkybox.HasProperty(NightBlend))
        {
            Debug.LogError("Assign the DayNightSkybox material in the scene's Lighting settings.", this);
            return;
        }

        directionalLight = GetComponent<Light>();
        dayAmbientSkyColor = RenderSettings.ambientSkyColor;
        dayAmbientEquatorColor = RenderSettings.ambientEquatorColor;
        dayAmbientGroundColor = RenderSettings.ambientGroundColor;
        dayLightColor = directionalLight.color;
        dayLightIntensity = directionalLight.intensity;
        dayReflectionIntensity = RenderSettings.reflectionIntensity;
        daylightRotation = transform.rotation;
        nightRotation = Quaternion.Euler(moonlightRotation);

        skybox = new Material(originalSkybox);
        skybox.SetVector(SunDirection, daylightRotation * Vector3.back);
        skybox.SetVector(MoonDirection, nightRotation * Vector3.back);
        RenderSettings.skybox = skybox;

        IsNight = startInNight;
        nightProgress = IsNight ? 1f : 0f;
        ApplyLighting(nightProgress);
    }

    private void Update()
    {
        if (skybox == null)
            return;

        float target = IsNight ? 1f : 0f;
        if (nightProgress == target)
            return;

        nightProgress = transitionDuration > 0f
            ? Mathf.MoveTowards(nightProgress, target, Time.unscaledDeltaTime / transitionDuration)
            : target;
        ApplyLighting(Mathf.SmoothStep(0f, 1f, nightProgress));
    }

    public void ToggleDayNight()
    {
        if (skybox != null)
            IsNight = !IsNight;
    }

    private void OnDisable()
    {
        if (skybox == null)
            return;

        ApplyLighting(0f);
        IsNight = false;
        RenderSettings.skybox = originalSkybox;
        Destroy(skybox);
        skybox = null;
    }

    private void ApplyLighting(float blend)
    {
        skybox.SetFloat(NightBlend, blend);
        RenderSettings.ambientSkyColor = Color.Lerp(dayAmbientSkyColor, nightSkyLight, blend);
        RenderSettings.ambientEquatorColor = Color.Lerp(dayAmbientEquatorColor, nightHorizonLight, blend);
        RenderSettings.ambientGroundColor = Color.Lerp(dayAmbientGroundColor, nightGroundLight, blend);
        RenderSettings.reflectionIntensity = dayReflectionIntensity * Mathf.Lerp(1f, nightReflectionMultiplier, blend);
        directionalLight.color = Color.Lerp(dayLightColor, moonlightColor, blend);
        directionalLight.intensity = dayLightIntensity * Mathf.Lerp(1f, moonlightIntensity, blend);
        transform.rotation = Quaternion.Slerp(daylightRotation, nightRotation, blend);
    }
}
