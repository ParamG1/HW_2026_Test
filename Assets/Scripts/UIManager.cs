using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the Start screen and Game Over screen, and owns the game's
/// high-level state (not-started / playing / game-over). Restarting
/// is done via canvas toggling + component reset, no scene reload,
/// per the assignment's Level 3 requirement.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public enum GameState { StartMenu, Playing, GameOver }
    public GameState CurrentState { get; private set; } = GameState.StartMenu;

    [Header("Canvases")]
    [SerializeField] private CanvasGroup _startCanvas;
    [SerializeField] private CanvasGroup _gameOverCanvas;
    [SerializeField] private CanvasGroup _hudCanvas; // ScoreText canvas, hidden until play starts

    [Header("Start Screen")]
    [SerializeField] private Button _playButton;

    [Header("Game Over Screen")]
    [SerializeField] private Button _restartButton;
    [SerializeField] private TextMeshProUGUI _finalScoreText;
    [SerializeField] private TextMeshProUGUI _bestScoreText;
    [SerializeField] private Image _warningRing; // signature element, filled/colored like Pulpit's warning lerp

    [Header("Respawn References")]
    [SerializeField] private Transform _doofusTransform;
    [SerializeField] private Rigidbody _doofusRigidbody;
    [SerializeField] private Vector3 _doofusStartPosition = new Vector3(0f, 1f, 0f);

    [Header("Panel Animation")]
    [SerializeField] private RectTransform _gameOverPanel;
    [SerializeField] private float _panelPopDuration = 0.28f;

    private const string BestScoreKey = "doofus_best_score";
    private int _bestScore;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
    }

    private void OnEnable()
    {
        DoofusController.OnDoofusFell += HandleDoofusFell;
        if (_playButton != null) _playButton.onClick.AddListener(HandlePlayPressed);
        if (_restartButton != null) _restartButton.onClick.AddListener(HandleRestartPressed);
    }

    private void OnDisable()
    {
        DoofusController.OnDoofusFell -= HandleDoofusFell;
        if (_playButton != null) _playButton.onClick.RemoveListener(HandlePlayPressed);
        if (_restartButton != null) _restartButton.onClick.RemoveListener(HandleRestartPressed);
    }

    private void Start()
    {
        EnterStartMenu();
    }

    // ---------------- State transitions ----------------

    private void EnterStartMenu()
    {
        CurrentState = GameState.StartMenu;
        Time.timeScale = 0f; // freeze physics/movement behind the menu

        SetCanvas(_startCanvas, true);
        SetCanvas(_gameOverCanvas, false);
        SetCanvas(_hudCanvas, false);
    }

    public void HandlePlayPressed()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;

        SetCanvas(_startCanvas, false);
        SetCanvas(_gameOverCanvas, false);
        SetCanvas(_hudCanvas, true);

        ResetRun();
    }

    private void HandleDoofusFell()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.GameOver;

        int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
        bool isNewBest = finalScore > _bestScore;
        if (isNewBest)
        {
            _bestScore = finalScore;
            PlayerPrefs.SetInt(BestScoreKey, _bestScore);
            PlayerPrefs.Save();
        }

        if (_finalScoreText != null) _finalScoreText.text = $"{finalScore}";
        if (_bestScoreText != null) _bestScoreText.text = isNewBest ? "NEW BEST" : $"BEST: {_bestScore}";

        Time.timeScale = 0f;
        SetCanvas(_hudCanvas, false);
        SetCanvas(_gameOverCanvas, true);

        if (_gameOverPanel != null) StartCoroutine(PopInPanel());
        if (_warningRing != null) StartCoroutine(PulseWarningRing());
    }

    public void HandleRestartPressed()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;

        SetCanvas(_gameOverCanvas, false);
        SetCanvas(_hudCanvas, true);

        ResetRun();
    }

    // ---------------- Reset logic ----------------

    private void ResetRun()
    {
        ScoreManager.Instance?.ResetScore();
        PulpitSpawner.Instance?.ResetSpawner();
        RespawnDoofus();
    }

    private void RespawnDoofus()
    {
        if (_doofusTransform == null) return;

        if (_doofusRigidbody != null)
        {
#if UNITY_6000_0_OR_NEWER
            _doofusRigidbody.linearVelocity = Vector3.zero;
#else
            _doofusRigidbody.velocity = Vector3.zero;
#endif
            _doofusRigidbody.angularVelocity = Vector3.zero;
        }

        _doofusTransform.SetPositionAndRotation(_doofusStartPosition, Quaternion.identity);
        _doofusTransform.GetComponent<DoofusController>()?.ResetState();
    }

    // ---------------- Helpers ----------------

    private static void SetCanvas(CanvasGroup group, bool visible)
    {
        if (group == null) return;
        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }

    private IEnumerator PopInPanel()
    {
        float elapsed = 0f;
        Vector3 from = Vector3.one * 0.85f;
        Vector3 to = Vector3.one;
        _gameOverPanel.localScale = from;

        while (elapsed < _panelPopDuration)
        {
            // Unscaled because Time.timeScale is 0 during Game Over
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _panelPopDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f); // ease-out cubic
            _gameOverPanel.localScale = Vector3.LerpUnclamped(from, to, eased);
            yield return null;
        }

        _gameOverPanel.localScale = to;
    }

    private IEnumerator PulseWarningRing()
    {
        // Echoes Pulpit.cs's own red-lerp warning language on the Game Over screen.
        Color from = new Color(0.24f, 0.81f, 0.42f); // pulpit green
        Color to = new Color(0.91f, 0.27f, 0.24f);   // warning red

        float elapsed = 0f;
        float duration = 0.6f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _warningRing.color = Color.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        _warningRing.color = to;
    }
}
