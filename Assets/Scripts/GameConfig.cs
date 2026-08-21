using System;
using System.IO;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public float speed;
}

[Serializable]
public class PulpitData
{
    public float min_pulpit_destroy_time;
    public float max_pulpit_destroy_time;
    public float pulpit_spawn_time;
}

[Serializable]
public class GameConfigData
{
    public PlayerData player_data;
    public PulpitData pulpit_data;
}

public class GameConfig : MonoBehaviour
{
    public static GameConfig Instance { get; private set; }

    [Header("Loaded Config Values")]
    public float PlayerSpeed { get; private set; } = 3f;
    public float MinPulpitDestroyTime { get; private set; } = 4f;
    public float MaxPulpitDestroyTime { get; private set; } = 5f;
    public float PulpitSpawnTime { get; private set; } = 2.5f;

    private const string JsonFileName = "doofus_diary.json";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadConfiguration();
    }

    private void LoadConfiguration()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, JsonFileName);

        if (File.Exists(filePath))
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);
                GameConfigData data = JsonUtility.FromJson<GameConfigData>(jsonString);

                if (data != null)
                {
                    if (data.player_data != null)
                    {
                        PlayerSpeed = data.player_data.speed;
                    }

                    if (data.pulpit_data != null)
                    {
                        MinPulpitDestroyTime = data.pulpit_data.min_pulpit_destroy_time;
                        MaxPulpitDestroyTime = data.pulpit_data.max_pulpit_destroy_time;
                        PulpitSpawnTime = data.pulpit_data.pulpit_spawn_time;
                    }

                    Debug.Log($"[GameConfig] Successfully loaded config. Speed: {PlayerSpeed}, Spawn: {PulpitSpawnTime}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameConfig] Failed to parse JSON: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"[GameConfig] Config file not found at: {filePath}. Using default values.");
        }
    }
}