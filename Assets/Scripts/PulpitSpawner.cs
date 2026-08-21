using System.Collections.Generic;
using UnityEngine;

public class PulpitSpawner : MonoBehaviour
{
    public static PulpitSpawner Instance { get; private set; }

    [Header("Prefab Reference")]
    [SerializeField] private GameObject _pulpitPrefab;

    private const float PlatformSize = 9f; // Pulpit size is 9x9
    private readonly List<Pulpit> _activePulpits = new List<Pulpit>();
    private bool _hasSpawnedFirst;

    // 4 cardinal directions (North, South, East, West)
    private readonly Vector3[] _directions = new Vector3[]
    {
        new Vector3(0f, 0f, PlatformSize),   // North (+Z)
        new Vector3(0f, 0f, -PlatformSize),  // South (-Z)
        new Vector3(PlatformSize, 0f, 0f),   // East (+X)
        new Vector3(-PlatformSize, 0f, 0f)   // West (-X)
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // UIManager now controls the first spawn (it happens when Play is
        // pressed via ResetSpawner). If UIManager isn't present in the scene
        // for some reason, fall back to the old auto-spawn-on-Start behavior
        // so the spawner still works standalone.
        if (UIManager.Instance == null)
        {
            SpawnPulpit(Vector3.zero);
            _hasSpawnedFirst = true;
        }
    }

    /// <summary>
    /// Called by UIManager when a run begins (fresh Play or Restart).
    /// Clears any pulpits left over from a previous run and spawns
    /// a fresh starting pulpit at the origin.
    /// </summary>
    public void ResetSpawner()
    {
        for (int i = _activePulpits.Count - 1; i >= 0; i--)
        {
            Pulpit p = _activePulpits[i];
            if (p == null) continue;

            p.OnSpawnTriggerTimeReached -= HandlePulpitSpawnTrigger;
            p.OnPulpitDestroyed -= HandlePulpitDestroyed;
            Destroy(p.gameObject);
        }
        _activePulpits.Clear();

        SpawnPulpit(Vector3.zero);
        _hasSpawnedFirst = true;
    }

    private void SpawnPulpit(Vector3 position)
    {
        if (_pulpitPrefab == null)
        {
            Debug.LogError("[PulpitSpawner] Pulpit Prefab is not assigned in the Inspector!");
            return;
        }

        GameObject obj = Instantiate(_pulpitPrefab, position, Quaternion.identity);
        Pulpit pulpit = obj.GetComponent<Pulpit>();

        float minDestroy = GameConfig.Instance != null ? GameConfig.Instance.MinPulpitDestroyTime : 4f;
        float maxDestroy = GameConfig.Instance != null ? GameConfig.Instance.MaxPulpitDestroyTime : 5f;
        float spawnTime = GameConfig.Instance != null ? GameConfig.Instance.PulpitSpawnTime : 2.5f;

        pulpit.Initialize(minDestroy, maxDestroy, spawnTime);

        pulpit.OnSpawnTriggerTimeReached += HandlePulpitSpawnTrigger;
        pulpit.OnPulpitDestroyed += HandlePulpitDestroyed;

        _activePulpits.Add(pulpit);
    }

    private void HandlePulpitSpawnTrigger(Pulpit currentPulpit)
    {
        currentPulpit.OnSpawnTriggerTimeReached -= HandlePulpitSpawnTrigger;

        // Ensure maximum 2 pulpits exist at any given time
        if (_activePulpits.Count >= 2) return;

        Vector3 nextPosition = GetRandomAdjacentPosition(currentPulpit.transform.position);
        SpawnPulpit(nextPosition);
    }

    private Vector3 GetRandomAdjacentPosition(Vector3 currentPos)
    {
        List<Vector3> availablePositions = new List<Vector3>();

        foreach (Vector3 dir in _directions)
        {
            Vector3 potentialPos = currentPos + dir;

            // Prevent placing the pulpit on top of an already existing pulpit
            bool isOccupied = false;
            foreach (Pulpit active in _activePulpits)
            {
                if (Vector3.Distance(active.transform.position, potentialPos) < 1f)
                {
                    isOccupied = true;
                    break;
                }
            }

            if (!isOccupied)
            {
                availablePositions.Add(potentialPos);
            }
        }

        if (availablePositions.Count > 0)
        {
            return availablePositions[Random.Range(0, availablePositions.Count)];
        }

        return currentPos + _directions[Random.Range(0, _directions.Length)];
    }

    private void HandlePulpitDestroyed(Pulpit pulpit)
    {
        pulpit.OnPulpitDestroyed -= HandlePulpitDestroyed;
        _activePulpits.Remove(pulpit);
    }
}
