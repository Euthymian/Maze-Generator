using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MazeFog))]
public sealed class MazeMusic : MonoBehaviour
{
    [SerializeField] private AudioClip dayMusic;
    [SerializeField] private AudioClip nightMusic;
    [SerializeField] private bool startWithMusic = true;
    [SerializeField, Range(0f, 1f)] private float distantVolume = 0.12f;
    [SerializeField, Range(0f, 1f)] private float nearbyVolume = 0.65f;
    [SerializeField, Min(0.1f)] private float audibleDistance = 15f;

    private AudioSource music;
    private MazeFog fog;
    private DayNightLighting lighting;
    private EnemyController enemy;
    private bool musicEnabled;

    private void Awake()
    {
        fog = GetComponent<MazeFog>();
        music = gameObject.AddComponent<AudioSource>();
        music.playOnAwake = false;
        music.loop = true;
        music.spatialBlend = 0f;
        musicEnabled = startWithMusic;
    }

    private void Start()
    {
        lighting = FindFirstObjectByType<DayNightLighting>();
        enemy = FindFirstObjectByType<EnemyController>();
    }

    private void LateUpdate()
    {
        if (Keyboard.current?.mKey.wasPressedThisFrame == true)
            musicEnabled = !musicEnabled;

        AudioClip track = lighting != null && lighting.IsNight ? nightMusic : dayMusic;
        if (music.clip != track)
        {
            music.Stop();
            music.clip = track;
        }

        float proximity = enemy != null && enemy.IsAlive
            ? 1f - Mathf.InverseLerp(1f, 1f + audibleDistance, Vector3.Distance(transform.position, enemy.transform.position))
            : 0f;
        float fogMultiplier = fog.isActiveAndEnabled && fog.FogEnabled ? 0.5f : 1f;
        music.volume = Mathf.Lerp(distantVolume, nearbyVolume, proximity) * fogMultiplier;

        if (musicEnabled && track != null && !music.isPlaying)
            music.Play();
        else if (!musicEnabled && music.isPlaying)
            music.Stop();
    }

    private void OnDisable()
    {
        music.Stop();
    }
}
