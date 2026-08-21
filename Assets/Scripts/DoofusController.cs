using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DoofusController : MonoBehaviour
{
    public static event Action OnDoofusFell;

    [Header("Fall Boundary")]
    [SerializeField] private float _fallThresholdY = -4f;

    private Rigidbody _rb;
    private Vector3 _movementInput;
    private float _speed;
    private bool _isGameOver;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        _speed = GameConfig.Instance != null ? GameConfig.Instance.PlayerSpeed : 3f;
    }

    private void Update()
    {
        if (_isGameOver) return;

        // Check if Doofus fell off
        if (transform.position.y < _fallThresholdY)
        {
            _isGameOver = true;
            OnDoofusFell?.Invoke();
            Debug.Log("[DoofusController] Doofus fell! Game Over.");
            return;
        }

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        _movementInput = new Vector3(moveX, 0f, moveZ).normalized;
    }

    private void FixedUpdate()
    {
        if (_isGameOver) return;
        MoveCharacter();
    }

    private void MoveCharacter()
    {
        #if UNITY_6000_0_OR_NEWER
        Vector3 currentYVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
        _rb.linearVelocity = (_movementInput * _speed) + currentYVelocity;
        #else
        Vector3 currentYVelocity = new Vector3(0f, _rb.velocity.y, 0f);
        _rb.velocity = (_movementInput * _speed) + currentYVelocity;
        #endif
    }

    /// <summary>
    /// Called by UIManager on Restart. Clears the fallen/game-over latch
    /// and zeroes pending input so movement resumes cleanly from the
    /// respawned position without a stale hangover from the last run.
    /// </summary>
    public void ResetState()
    {
        _isGameOver = false;
        _movementInput = Vector3.zero;
    }
}
