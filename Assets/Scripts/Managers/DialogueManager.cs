using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerText;

    [Header("Input & Effects")]
    [SerializeField] private KeyCode advanceKey = KeyCode.F;
    [SerializeField] private bool useTypewriter = true;
    [SerializeField] private float charactersPerSecond = 40f;

    private DialogueData _currentData;
    private int _currentIndex;
    private bool _isActive;
    private bool _isTyping;
    private Coroutine _typingRoutine;
    private Action _onComplete;

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
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!_isActive) return;

        if (Input.GetKeyDown(advanceKey))
        {
            if (_isTyping)
            {
                // Skip typewriter: show full line immediately
                FinishTypingInstantly();
            }
            else
            {
                AdvanceLine();
            }
        }
    }

    // =============================
    // Public API
    // =============================

    public void Play(DialogueData data, Action onComplete = null)
    {
        if (data == null)
        {
            Debug.LogWarning("DialogueManager.Play called with null data");
            onComplete?.Invoke();
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

        if (dialogueUI != null)
            dialogueUI.SetActive(true);

        ShowCurrentLine();

        if (_currentData.pauseGameDuringDialogue)
            Time.timeScale = 0f;
    }

    // Optional string-based version
    public void Play(string id, Action onComplete = null)
    {
        if (string.IsNullOrEmpty(id))
        {
            onComplete?.Invoke();
            return;
        }

        if (!_byId.TryGetValue(id, out var data))
        {
            Debug.LogWarning($"DialogueManager: No DialogueData for id '{id}'");
            onComplete?.Invoke();
            return;
        }

        Play(data, onComplete);
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

        cb?.Invoke();
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
