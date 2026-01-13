using UnityEngine;
using System.Collections;
using TMPro;

public class FikaBossCutsceneManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject fikaBossPrefab;
    [SerializeField] private GameObject monaPrefab;
    [SerializeField] private GameObject yojiPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform fikaSpawnPoint;
    [SerializeField] private Transform monaSpawnPoint;
    [SerializeField] private Transform yojiSpawnPoint;

    [Header("Dialogues")]
    [SerializeField] private DialogueData MonaFirstDialogue;
    [SerializeField] private DialogueData FikaFirstDialogue;
    [SerializeField] private DialogueData FikaBattleStartDialogue; // Battle start dialogue for return visits
    [SerializeField] private DialogueData yojiInterruptsDialogue;
    
    [Header("Movement")]
    [SerializeField] private float yojiMoveSpeed = 5f;
    [SerializeField] private float arriveDistance = 0.25f;

    [Header("Drama Timings")]
    [SerializeField] private float pauseBeforeCombo = 0.15f;
    [SerializeField] private float pauseBetweenHits = 0.12f;
    [SerializeField] private float pauseBeforeMonaDies = 0.2f;

    [Header("Vanish")]
    [SerializeField] private float vanishDelay = 0.25f;

    [Header("Boss HealthBar")]
    [SerializeField] private GameObject FikaHealthBar;

    [Header("Player")]
    [SerializeField] private GameObject playerOverride;

    private bool cutsceneTriggered;

    private GameObject fikaInstance, monaInstance, yojiInstance;

    private FikaBoss fikaAI;
    private Animator yojiAnim, monaAnim;
    private Rigidbody2D yojiRB;

    private CutsceneAnimEvents yojiEvents;
    private CutsceneAnimEvents monaEvents;

    private bool monaDeathFinished;
    private int lastHitIndex;

    public void TriggerBossCutscene()
    {
        if (cutsceneTriggered) return;
        cutsceneTriggered = true;

        // Check if cutscene has already been seen using dedicated flag
        if (GameManager.Instance != null)
        {
            bool cutsceneSeen = GameManager.Instance.GetFlag(GameFlag.FikaCutsceneSeen);
            
            if (cutsceneSeen)
            {
                Debug.Log("Fika cutscene already seen (FikaCutsceneSeen flag = true) - skipping to battle start");
                StartCoroutine(SkipToBattle());
                return;
            }
            else
            {
                Debug.Log("First time seeing Fika cutscene - playing full sequence");
            }
        }

        // First time - play full cutscene
        if (playerOverride != null)
        {
            playerOverride.transform.position = new Vector3(-7, playerOverride.transform.position.y, 0);
        }
        StartCoroutine(CutsceneSequence());
    }

    /// <summary>
    /// Skip cutscene and go straight to battle (for return visits)
    /// </summary>
    private IEnumerator SkipToBattle()
    {
        // Lock player briefly
        UserInputManager.Instance.DisableInput();
        GameObject player = playerOverride != null ? playerOverride : GameObject.FindGameObjectWithTag("Player");
        PlayerController pc = player != null ? player.GetComponent<PlayerController>() : null;
        Rigidbody2D playerRB = player != null ? player.GetComponent<Rigidbody2D>() : null;

        if (pc != null) pc.enabled = false;
        if (playerRB != null) playerRB.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.2f);

        // Spawn only Fika (no Mona, no Yoji)
        fikaInstance = Instantiate(fikaBossPrefab, fikaSpawnPoint.position, Quaternion.identity);

        fikaAI = fikaInstance.GetComponent<FikaBoss>();
        if (fikaAI != null)
        {
            fikaAI.enabled = false;
            
            // Assign WaveManager reference
            WaveManager waveManager = FindFirstObjectByType<WaveManager>();
            if (waveManager != null)
            {
                fikaAI.waveManager = waveManager;
                Debug.Log("Fika WaveManager reference assigned");
            }
        }

        yield return new WaitForSeconds(0.5f);

        // Play battle start dialogue (quick intro)
        if (FikaBattleStartDialogue != null)
        {
            yield return PlayDialogueIfAny(FikaBattleStartDialogue);
            UserInputManager.Instance.DisableInput();
        }

        yield return new WaitForSeconds(0.3f);

        // Start fight
        if (fikaAI != null) fikaAI.enabled = true;
        if (FikaHealthBar != null) FikaHealthBar.SetActive(true);
        if (pc != null) pc.enabled = true;
        UserInputManager.Instance.EnableInput();

        Debug.Log("Skipped cutscene - Boss fight begins!");
    }

    private IEnumerator CutsceneSequence()
    {
        // Lock player
        UserInputManager.Instance.DisableInput();
        GameObject player = playerOverride != null ? playerOverride : GameObject.FindGameObjectWithTag("Player");
        PlayerController pc = player != null ? player.GetComponent<PlayerController>() : null;
        Rigidbody2D playerRB = player != null ? player.GetComponent<Rigidbody2D>() : null;

        if (pc != null) pc.enabled = false;
        if (playerRB != null) playerRB.linearVelocity = Vector2.zero;

        // Spawn Fika + Mona
        fikaInstance = Instantiate(fikaBossPrefab, fikaSpawnPoint.position, Quaternion.identity);
        monaInstance = Instantiate(monaPrefab, monaSpawnPoint.position, Quaternion.identity);

        fikaAI = fikaInstance.GetComponent<FikaBoss>();
        if (fikaAI != null)
        {
            fikaAI.enabled = false;
            
            // Assign WaveManager reference so portals spawn when Fika dies
            WaveManager waveManager = FindFirstObjectByType<WaveManager>();
            if (waveManager != null)
            {
                fikaAI.waveManager = waveManager;
                Debug.Log("Fika WaveManager reference assigned");
            }
            else
            {
                Debug.LogWarning("WaveManager not found in scene!");
            }
        }

        monaAnim = monaInstance.GetComponentInChildren<Animator>();

        yield return new WaitForSeconds(0.25f);

        // Dialogue 1
        yield return PlayDialogueIfAny(MonaFirstDialogue);
        UserInputManager.Instance.DisableInput();
        
        // Dialogue 2
        yield return PlayDialogueIfAny(FikaFirstDialogue);
        UserInputManager.Instance.DisableInput();
        
        // Spawn Yoji
        yojiInstance = Instantiate(yojiPrefab, yojiSpawnPoint.position, Quaternion.identity);
        yojiAnim = yojiInstance.GetComponentInChildren<Animator>();

        yojiRB = yojiInstance.GetComponent<Rigidbody2D>();
        if (yojiRB == null) yojiRB = yojiInstance.AddComponent<Rigidbody2D>();
        yojiRB.gravityScale = 0f;
        yojiRB.freezeRotation = true;
        yojiRB.linearVelocity = Vector2.zero;

        // Events
        yojiEvents = yojiInstance.GetComponentInChildren<CutsceneAnimEvents>();
        if (yojiEvents == null) yojiEvents = yojiInstance.AddComponent<CutsceneAnimEvents>();

        monaEvents = monaInstance.GetComponentInChildren<CutsceneAnimEvents>();
        if (monaEvents == null) monaEvents = monaInstance.AddComponent<CutsceneAnimEvents>();

        HookEvents();

        if (yojiAnim != null) yojiAnim.SetTrigger("Enter");
        yojiInstance.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        // Dialogue 3
        yield return PlayDialogueIfAny(yojiInterruptsDialogue);
        UserInputManager.Instance.DisableInput();
        
        // Move to Mona
        yield return MoveYojiToMona();
        if (yojiAnim != null) yojiAnim.SetBool("Run", false);

        yield return new WaitForSeconds(pauseBeforeCombo);

        // 3-hit combo (Attack1 -> Attack2 -> Attack3)
        yield return DoYojiCombo3();

        yield return new WaitForSeconds(pauseBeforeMonaDies);

        // Mona dies
        monaDeathFinished = false;
        if (monaAnim != null) monaAnim.SetTrigger("Die");

        yield return WaitUntilOrTimeout(() => monaDeathFinished, 3f);

        // Vanish
        yield return new WaitForSeconds(vanishDelay);
        SetRenderersEnabled(monaInstance, false);
        SetRenderersEnabled(yojiInstance, false);

        Destroy(monaInstance);
        Destroy(yojiInstance);

        //Mark cutscene as seen BEFORE starting the fight
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag(GameFlag.FikaCutsceneSeen, true);
            GameManager.Instance.SaveProgress();
            Debug.Log("Fika cutscene completed - FikaCutsceneSeen flag set to true");
        }

        // Start fight + unlock player
        if (fikaAI != null) fikaAI.enabled = true;
        if (FikaHealthBar != null) FikaHealthBar.SetActive(true);
        if (pc != null) pc.enabled = true;
        UserInputManager.Instance.EnableInput();
        UnhookEvents();

        Debug.Log("Full cutscene done. Boss fight begins!");
    }

    private IEnumerator DoYojiCombo3()
    {
        lastHitIndex = 0;
        SpriteRenderer monaSR = monaInstance.GetComponent<SpriteRenderer>();

        if (yojiAnim != null) yojiAnim.SetTrigger("Attack1");
        yield return new WaitForSeconds(0.2f);
        if (monaAnim != null) monaAnim.SetTrigger("Hurt");
        StartCoroutine(FlashRed(monaSR));
        yield return WaitUntilOrTimeout(() => lastHitIndex == 1, 2f);
        yield return new WaitForSeconds(pauseBetweenHits);

        if (yojiAnim != null) yojiAnim.SetTrigger("Attack2");
        yield return new WaitForSeconds(0.2f);
        if (monaAnim != null) monaAnim.SetTrigger("Hurt");
        StartCoroutine(FlashRed(monaSR));
        yield return WaitUntilOrTimeout(() => lastHitIndex == 2, 2f);
        yield return new WaitForSeconds(pauseBetweenHits);

        if (yojiAnim != null) yojiAnim.SetTrigger("Attack3");
        yield return new WaitForSeconds(0.2f);
        if (monaAnim != null) monaAnim.SetTrigger("Hurt");
        StartCoroutine(FlashRed(monaSR));
        yield return WaitUntilOrTimeout(() => lastHitIndex == 3, 2f);

    }

    protected IEnumerator FlashRed(SpriteRenderer sr)
    {
        if (sr != null)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = Color.white;
        }
    }

    private IEnumerator MoveYojiToMona()
    {
        if (yojiInstance == null || monaInstance == null || yojiRB == null) yield break;

        Vector2 target = monaInstance.transform.position;

        if (yojiAnim != null) yojiAnim.SetBool("Run", true);
        FaceTowards(yojiInstance.transform, target);

        while (Vector2.Distance(yojiRB.position, target) > arriveDistance)
        {
            Vector2 next = Vector2.MoveTowards(yojiRB.position, target, yojiMoveSpeed * Time.deltaTime);
            yojiRB.MovePosition(next);
            yield return null;
        }

        yojiRB.linearVelocity = Vector2.zero;
    }

    private void FaceTowards(Transform t, Vector2 targetPos)
    {
        if (t == null) return;
        float dir = targetPos.x - t.position.x;
        if (Mathf.Abs(dir) < 0.01f) return;

        Vector3 s = t.localScale;
        s.x = Mathf.Abs(s.x) * (dir >= 0 ? 1f : -1f);
        t.localScale = s;
    }

    private IEnumerator PlayDialogueIfAny(DialogueData data)
    {
        if (DialogueManager.Instance == null || data == null) yield break;

        bool done = false;
        DialogueManager.Instance.Play(data, () => done = true);
        while (!done) yield return null;
    }

    private IEnumerator WaitUntilOrTimeout(System.Func<bool> condition, float timeoutSeconds)
    {
        float t = 0f;
        while (!condition())
        {
            t += Time.deltaTime;
            if (t >= timeoutSeconds) break;
            yield return null;
        }
    }

    private void SetRenderersEnabled(GameObject go, bool enabled)
    {
        if (go == null) return;
        var srs = go.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < srs.Length; i++) srs[i].enabled = enabled;
    }

    private void HookEvents()
    {
        if (yojiEvents != null) yojiEvents.OnHitIndex += OnYojiHitIndex;
        if (monaEvents != null) monaEvents.OnDeathFinished += OnMonaDeathFinished;
    }

    private void UnhookEvents()
    {
        if (yojiEvents != null) yojiEvents.OnHitIndex -= OnYojiHitIndex;
        if (monaEvents != null) monaEvents.OnDeathFinished -= OnMonaDeathFinished;
    }

    private void OnYojiHitIndex(int idx)
    {
        lastHitIndex = idx;
    }

    private void OnMonaDeathFinished()
    {
        monaDeathFinished = true;
    }
}