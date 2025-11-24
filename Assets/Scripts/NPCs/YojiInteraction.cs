using UnityEngine;
using TMPro;   // שים לב: במקום UnityEngine.UI

public class YojiInteraction : MonoBehaviour
{
    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;   // עכשיו זה TMP_Text ולא Text רגיל

    [TextArea(2, 5)]
    public string[] dialogueLines;
    [SerializeField] private YojiGateBarrier gateBarrier;


    public KeyCode interactKey = KeyCode.UpArrow;   // חץ למעלה

    private bool isPlayerInRange = false;
    private int currentLineIndex = 0;
    private bool dialogueActive = false;

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!isPlayerInRange) return;

        // התחלת דיאלוג
        if (!dialogueActive && Input.GetKeyDown(interactKey))
        {
            StartDialogue();
        }
        // המשך דיאלוג
        else if (dialogueActive && Input.GetKeyDown(interactKey))
        {
            NextLine();
        }
    }

    private void StartDialogue()
    {
        if (dialoguePanel == null || dialogueText == null)
        {
            Debug.LogError("YojiInteraction: Dialogue UI is not assigned!");
            return;
        }

        dialogueActive = true;
        currentLineIndex = 0;
        dialoguePanel.SetActive(true);
        dialogueText.text = dialogueLines[currentLineIndex];

        Time.timeScale = 0f; // אם מציק שהמשחק נעצר – אפשר להסיר
    }

    private void NextLine()
    {
        currentLineIndex++;

        if (currentLineIndex < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentLineIndex];
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        dialogueActive = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        Time.timeScale = 1f;

        // עדכון GameManager שהשחקן דיבר עם יוג'י
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetYojiTalked();
        }

        // הורדת המחסום
        if (gateBarrier != null)
        {
            gateBarrier.DisableBarrier();
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Player can talk to Yoji (press UpArrow).");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (dialogueActive)
            {
                EndDialogue();
            }
        }
    }
}
