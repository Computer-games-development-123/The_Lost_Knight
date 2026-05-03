using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerText;

    [Header("Portrait (optional)")]
    [SerializeField] private Image portraitImage;        // UI Image
    [SerializeField] private bool hidePortraitIfMissing = true;

    [Header("Input & Effects")]
    [SerializeField] private KeyCode advanceKey = KeyCode.F;
    [SerializeField] private bool useTypewriter = true;
    [SerializeField] private float charactersPerSecond = 40f;

    [Header("Touch Input (Mobile)")]
    [Tooltip("If true, tapping anywhere on screen advances the dialogue")]
    [SerializeField] private bool tapAnywhereToAdvance = true;

    [Tooltip("Delay before tap input becomes valid after dialogue starts (prevents tap-through)")]
    [SerializeField] private float tapInputDelay = 0.15f;

    private DialogueData _currentData;
    private int _currentIndex;
    private bool _isActive;
    private bool _isTyping;
    private Coroutine _typingRoutine;
    private Action _onComplete;
    private bool _keepInputDisabled; // Flag to keep input disabled after dialogue ends
    private float _dialogueStartTime; // Used to ignore taps that started before dialogue opened

    private readonly Dictionary<string, DialogueData> _byId =
        new Dictionary<string, DialogueData>();

    public bool IsDialogueActive => _isActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllDialoguesFromResources();
    }

    private void Start()
    {
        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        ApplyPortrait(null);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!_isActive) return;

        bool advancePressed = false;

        // Keyboard input (F key) - still works for PC
        if (Input.GetKeyDown(advanceKey))
        {
            advancePressed = true;
        }

        // Mobile interact button
        if (MobileInputBridge.Instance != null && MobileInputBridge.Instance.InteractPressed)
        {
            advancePressed = true;
        }

        // Touch / mobile mouse input — tap anywhere to advance
        if (tapAnywhereToAdvance && Time.unscaledTime - _dialogueStartTime > tapInputDelay)
        {
            // Touch input
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began)
                {
                    if (!IsPointerOverMobileButton(touch.position))
                    {
                        advancePressed = true;
                        break;
                    }
                }
            }

            // Mouse input (for editor testing)
            if (!advancePressed && Input.GetMouseButtonDown(0))
            {
                if (!IsPointerOverMobileButton(Input.mousePosition))
                {
                    advancePressed = true;
                }
            }
        }

        if (advancePressed)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDialogueAdvance();
            }

            if (_isTyping)
            {
                FinishTypingInstantly();
            }
            else
            {
                AdvanceLine();
            }
        }
    }

    /// <summary>
    /// Returns true if the screen position is over a UI element on the MobileControlsCanvas
    /// (joystick or action buttons), so taps there don't accidentally advance dialogue.
    /// </summary>
    private bool IsPointerOverMobileButton(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;

        PointerEventData ped = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);

        foreach (var hit in results)
        {
            // Block taps on the mobile controls (joystick + action buttons)
            if (hit.gameObject.transform.root.name.Contains("MobileControls"))
                return true;
        }
        return false;
    }

    // =============================
    // Public API
    // =============================

    public void Play(DialogueData data, Action onComplete = null, bool keepInputDisabled = false)
    {
        // Disable player input (with null safety)
        if (UserInputManager.Instance != null)
        {
            UserInputManager.Instance.DisableInput();
        }
        else
        {
            Debug.LogWarning("DialogueManager: UserInputManager instance not found!");
        }

        if (data == null)
        {
            Debug.LogWarning("DialogueManager.Play called with null data");
            onComplete?.Invoke();
            // Only re-enable if not keeping disabled
            if (!keepInputDisabled && UserInputManager.Instance != null)
            {
                UserInputManager.Instance.EnableInput();
            }
            return;
        }

        if (_isActive)
        {
            Debug.LogWarning("DialogueManager: dialogue already active");
            return;
        }

        _currentData = data;
        _currentIndex = 0;
        _onComplete = onComplete;
        _isActive = true;
        _keepInputDisabled = keepInputDisabled; // Store the flag
        _dialogueStartTime = Time.unscaledTime; // Track start time for tap input delay

        if (dialogueUI != null)
            dialogueUI.SetActive(true);

        // NEW: show portrait (one per dialogue)
        ApplyPortrait(_currentData.portrait);

        ShowCurrentLine();

        if (_currentData.pauseGameDuringDialogue)
            Time.timeScale = 0f;
    }

    // =============================
    // Internal
    // =============================

    private void AdvanceLine()
    {
        if (_currentData == null || _currentData.lines == null)
        {
            EndDialogue();
            return;
        }

        _currentIndex++;

        if (_currentIndex >= _currentData.lines.Length)
        {
            EndDialogue();
        }
        else
        {
            ShowCurrentLine();
        }
    }

    private void ShowCurrentLine()
    {
        if (_currentData == null ||
            _currentData.lines == null ||
            _currentIndex >= _currentData.lines.Length)
        {
            EndDialogue();
            return;
        }

        string line = _currentData.lines[_currentIndex];

        if (speakerText != null)
            speakerText.text = _currentData.speakerName;

        if (!useTypewriter || dialogueText == null)
        {
            _isTyping = false;
            if (_typingRoutine != null)
            {
                StopCoroutine(_typingRoutine);
                _typingRoutine = null;
            }

            if (dialogueText != null)
                dialogueText.text = line;
        }
        else
        {
            if (_typingRoutine != null)
                StopCoroutine(_typingRoutine);

            _typingRoutine = StartCoroutine(TypeLine(line));
        }
    }

    private IEnumerator TypeLine(string fullText)
    {
        _isTyping = true;
        dialogueText.text = string.Empty;

        int visibleChars = 0;
        float delay = 1f / Mathf.Max(1f, charactersPerSecond);

        while (visibleChars < fullText.Length)
        {
            visibleChars++;
            dialogueText.text = fullText.Substring(0, visibleChars);
            yield return new WaitForSecondsRealtime(delay);
        }

        _isTyping = false;
        _typingRoutine = null;
    }

    private void FinishTypingInstantly()
    {
        if (!_isTyping || _currentData == null ||
            _currentData.lines == null ||
            _currentIndex >= _currentData.lines.Length)
            return;

        if (_typingRoutine != null)
        {
            StopCoroutine(_typingRoutine);
            _typingRoutine = null;
        }

        string fullText = _currentData.lines[_currentIndex];
        if (dialogueText != null)
            dialogueText.text = fullText;

        _isTyping = false;
    }

    private void EndDialogue()
    {
        _isActive = false;

        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        if (_currentData != null && _currentData.pauseGameDuringDialogue)
            Time.timeScale = 1f;

        var cb = _onComplete;
        _onComplete = null;
        _currentData = null;

        if (dialogueText != null)
            dialogueText.text = string.Empty;

        if (speakerText != null)
            speakerText.text = string.Empty;

        // NEW: clear portrait
        ApplyPortrait(null);

        cb?.Invoke();

        // Re-enable player input (with null safety) - but only if not keeping it disabled
        if (!_keepInputDisabled && UserInputManager.Instance != null)
        {
            UserInputManager.Instance.EnableInput();
        }

        // Reset the flag
        _keepInputDisabled = false;
    }

    private void ApplyPortrait(Sprite sprite)
    {
        if (portraitImage == null) return;

        if (sprite == null)
        {
            portraitImage.sprite = null;
            if (hidePortraitIfMissing)
                portraitImage.gameObject.SetActive(false);
            return;
        }

        portraitImage.sprite = sprite;
        portraitImage.gameObject.SetActive(true);
    }

    private void LoadAllDialoguesFromResources()
    {
        _byId.Clear();
        DialogueData[] all = Resources.LoadAll<DialogueData>("");

        foreach (var d in all)
        {
            if (d == null || string.IsNullOrEmpty(d.id)) continue;
            if (_byId.ContainsKey(d.id))
            {
                Debug.LogWarning($"DialogueManager: duplicate id '{d.id}'");
                continue;
            }

            _byId.Add(d.id, d);
        }
    }
}