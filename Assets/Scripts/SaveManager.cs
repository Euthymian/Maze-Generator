using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    private const string SaveFileName = "maze-save.json";
    private const float MessageDuration = 2f;

    [SerializeField] private MazeGenerator maze;
    [SerializeField] private EnemyController enemy;
    [SerializeField] private MazeScore score;
    [SerializeField] private TMP_Text statusLabel; 

    private static GameData pendingLoad;
    private float hideMessageAt;

    private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private void Awake()
    {
        if (maze == null || enemy == null || score == null)
        {
            Debug.LogError("Assign the maze, enemy, and score on Save Manager.", this);
            enabled = false;
            return;
        }

        if (statusLabel != null)
            statusLabel.text = string.Empty;
    }

    private IEnumerator Start()
    {
        if (pendingLoad == null)
            yield break;

        GameData data = pendingLoad;
        pendingLoad = null;

        yield return null;
        Apply(data);
        ShowMessage("Game loaded");
    }

    private void Update()
    {
        if (statusLabel != null && statusLabel.text.Length > 0 && Time.unscaledTime >= hideMessageAt)
            statusLabel.text = string.Empty;
    }

    public void Save()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null)
        {
            ShowMessage("Nothing to save yet");
            return;
        }

        var data = new GameData
        {
            score = score.Score,
            mazeSeed = maze.Seed,
            playerPos = player.transform.position,
            yaw = player.Yaw,
            pitch = player.Pitch
        };
        enemy.SaveTo(data);

        try
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
            Debug.Log($"Saved game to {SavePath}");
            ShowMessage("Game saved");
        }
        catch (Exception e)
        {
            Debug.LogError($"Could not save game: {e.Message}");
            ShowMessage("Save failed");
        }
    }

    public void Load()
    {
        if (!File.Exists(SavePath))
        {
            ShowMessage("No saved game");
            return;
        }

        GameData data;
        try
        {
            data = JsonUtility.FromJson<GameData>(File.ReadAllText(SavePath));
        }
        catch (Exception e)
        {
            Debug.LogError($"Could not load game: {e.Message}");
            ShowMessage("Load failed");
            return;
        }

        if (data == null)
        {
            ShowMessage("Save file is damaged");
            return;
        }

        pendingLoad = data;
        MazeGenerator.NextSeed = data.mazeSeed;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Apply(GameData data)
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            player.Teleport(data.playerPos, data.yaw, data.pitch);

        enemy.LoadFrom(data);
        score.SetScore(data.score);
    }

    private void ShowMessage(string message)
    {
        Debug.Log(message);
        if (statusLabel == null)
            return;

        statusLabel.text = message;
        hideMessageAt = Time.unscaledTime + MessageDuration;
    }
}