using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int CurrentScore { get; private set; } = 0;
    public event Action<int> OnScoreChanged;

    private int _lastVisitedPulpitId = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterPulpitVisit(int pulpitInstanceId)
    {
        // Avoid incrementing if the player is still on or re-enters the same pulpit
        if (pulpitInstanceId == _lastVisitedPulpitId) return;

        _lastVisitedPulpitId = pulpitInstanceId;
        CurrentScore++;
        OnScoreChanged?.Invoke(CurrentScore);
        Debug.Log($"[ScoreManager] Scored! New Score: {CurrentScore}");
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        _lastVisitedPulpitId = -1;
        OnScoreChanged?.Invoke(CurrentScore);
    }
}