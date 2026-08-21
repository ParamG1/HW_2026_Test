using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Pulpit : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshPro _timerText;

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

        // Trigger the spawn entrance animation
        StartCoroutine(AnimateSpawnRoutine());
        StartCoroutine(LifetimeRoutine(spawnTime));
    }

    private IEnumerator AnimateSpawnRoutine()
    {
        float elapsed = 0f;
        float duration = 0.4f;
        Vector3 targetPosition = transform.position;
        Vector3 startPosition = targetPosition + (Vector3.down * 10f); // Start 10 units below

        while (elapsed < duration)
        {
            // Ease-out lerp for a smooth arrival
            float t = elapsed / duration;
            float easedT = 1f - Mathf.Pow(1f - t, 3f); 
            transform.position = Vector3.Lerp(startPosition, targetPosition, easedT);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
    }

    private IEnumerator LifetimeRoutine(float spawnTime)
    {
        while (RemainingTime > 0f)
        {
            RemainingTime -= Time.deltaTime;
            float elapsedTime = TotalLifetime - RemainingTime;

            if (!_spawnTriggerFired && elapsedTime >= spawnTime)
            {
                _spawnTriggerFired = true;
                OnSpawnTriggerTimeReached?.Invoke(this);
            }

            if (_timerText != null)
            {
                _timerText.text = Mathf.Max(0f, RemainingTime).ToString("F1");
            }

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

        // 1. Fire the destroyed event early so the spawner can immediately spawn a replacement
        OnPulpitDestroyed?.Invoke(this);

        // 2. Animate the platform falling into the abyss
        float fallElapsed = 0f;
        float fallDuration = 0.5f;
        Vector3 currentPos = transform.position;
        Vector3 targetPos = currentPos + (Vector3.down * 15f);

        while (fallElapsed < fallDuration)
        {
            float t = fallElapsed / fallDuration;
            // Ease-in lerp to simulate gravity
            transform.position = Vector3.Lerp(currentPos, targetPos, t * t);
            fallElapsed += Time.deltaTime;
            yield return null;
        }

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