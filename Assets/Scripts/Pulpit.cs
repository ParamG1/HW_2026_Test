using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Pulpit : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshPro _timerText; // Optional 3D floating text

    public event Action<Pulpit> OnSpawnTriggerTimeReached;
    public event Action<Pulpit> OnPulpitDestroyed;
    public event Action<Pulpit> OnDoofusStepped;

    public float TotalLifetime { get; private set; }
    public float RemainingTime { get; private set; }

    private Material _platformMaterial;
    private Color _originalColor;
    private bool _spawnTriggerFired;

    public void Initialize(float minDestroyTime, float maxDestroyTime, float spawnTime)
    {
        TotalLifetime = UnityEngine.Random.Range(minDestroyTime, maxDestroyTime);
        RemainingTime = TotalLifetime;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            _platformMaterial = rend.material;
            _originalColor = _platformMaterial.color;
        }

        StartCoroutine(LifetimeRoutine(spawnTime));
    }

    private IEnumerator LifetimeRoutine(float spawnTime)
    {
        while (RemainingTime > 0f)
        {
            RemainingTime -= Time.deltaTime;
            float elapsedTime = TotalLifetime - RemainingTime;

            // Trigger spawner when elapsed time hits spawn_time
            if (!_spawnTriggerFired && elapsedTime >= spawnTime)
            {
                _spawnTriggerFired = true;
                OnSpawnTriggerTimeReached?.Invoke(this);
            }

            // Visual countdown display
            if (_timerText != null)
            {
                _timerText.text = Mathf.Max(0f, RemainingTime).ToString("F1");
            }

            // Warning visual during the last 1.0 second: fade toward red and shrink slightly
            if (RemainingTime <= 1.0f)
            {
                float t = 1f - Mathf.Clamp01(RemainingTime);
                if (_platformMaterial != null)
                {
                    _platformMaterial.color = Color.Lerp(_originalColor, Color.red, t);
                }
                transform.localScale = Vector3.Lerp(new Vector3(9f, 0.5f, 9f), new Vector3(8.5f, 0.4f, 8.5f), t);
            }

            yield return null;
        }

        OnPulpitDestroyed?.Invoke(this);
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<DoofusController>() != null)
        {
            OnDoofusStepped?.Invoke(this);
        }
    }

    private void OnTriggerEnter(Collider other)
{
    if (other.GetComponent<DoofusController>() != null)
    {
        ScoreManager.Instance?.RegisterPulpitVisit(gameObject.GetInstanceID());
    }
}

}