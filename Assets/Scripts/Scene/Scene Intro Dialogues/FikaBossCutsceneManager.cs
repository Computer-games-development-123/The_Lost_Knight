using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the Fika & Mona boss cutscene:
/// 1. Fika & Mona spawn
/// 2. Fika dialogue
/// 3. Yoji pops in
/// 4. Yoji moves to Mona, both disappear
/// 5. Fika challenge dialogue
/// 6. Boss fight begins
/// </summary>
public class FikaBossCutsceneManager : MonoBehaviour
{
    [Header("Boss References")]
    public GameObject fikaBossPrefab;
    public GameObject monaBossPrefab;
    public GameObject yojiPrefab; // Or use existing Yoji if available
    
    [Header("Spawn Positions")]
    public Transform fikaSpawnPoint;
    public Transform monaSpawnPoint;
    public Transform yojiSpawnPoint;
    
    [Header("Dialogues")]
    public DialogueData fikaMonaAppearDialogue;
    public DialogueData yojiInterruptsDialogue;
    public DialogueData fikaChallengeDialogue;
    
    [Header("Movement")]
    public float yojiMoveSpeed = 5f;
    
    private GameObject fikaInstance;
    private GameObject monaInstance;
    private GameObject yojiInstance;
    private bool cutsceneTriggered = false;

    public void TriggerBossCutscene()
    {
        if (cutsceneTriggered) return;
        cutsceneTriggered = true;
        
        StartCoroutine(BossCutsceneSequence());
    }

    private IEnumerator BossCutsceneSequence()
    {
        // Pause player movement
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.enabled = false; // Disable player control
        }

        // 1. Spawn Fika and Mona
        fikaInstance = Instantiate(fikaBossPrefab, fikaSpawnPoint.position, Quaternion.identity);
        monaInstance = Instantiate(monaBossPrefab, monaSpawnPoint.position, Quaternion.identity);
        
        // Disable Fika's AI temporarily
        FikaBoss fikaScript = fikaInstance.GetComponent<FikaBoss>();
        if (fikaScript != null) fikaScript.enabled = false;

        yield return new WaitForSeconds(1f);

        // 2. Fika & Mona appear dialogue
        if (DialogueManager.Instance != null && fikaMonaAppearDialogue != null)
        {
            bool dialogueDone = false;
            DialogueManager.Instance.Play(fikaMonaAppearDialogue, () => dialogueDone = true);
            
            while (!dialogueDone)
                yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // 3. Yoji pops in
        yojiInstance = Instantiate(yojiPrefab, yojiSpawnPoint.position, Quaternion.identity);
        
        yield return new WaitForSeconds(0.5f);

        // 4. Yoji dialogue
        if (DialogueManager.Instance != null && yojiInterruptsDialogue != null)
        {
            bool dialogueDone = false;
            DialogueManager.Instance.Play(yojiInterruptsDialogue, () => dialogueDone = true);
            
            while (!dialogueDone)
                yield return null;
        }

        // 5. Yoji moves to Mona
        yield return StartCoroutine(MoveYojiToMona());

        yield return new WaitForSeconds(0.5f);

        // 6. Both disappear
        Destroy(yojiInstance);
        Destroy(monaInstance);

        yield return new WaitForSeconds(1f);

        // 7. Fika challenge dialogue
        if (DialogueManager.Instance != null && fikaChallengeDialogue != null)
        {
            bool dialogueDone = false;
            DialogueManager.Instance.Play(fikaChallengeDialogue, () => dialogueDone = true);
            
            while (!dialogueDone)
                yield return null;
        }

        // 8. Enable Fika's AI - BOSS FIGHT STARTS
        if (fikaScript != null) fikaScript.enabled = true;

        // Re-enable player control
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.enabled = true;
        }

        Debug.Log("Boss fight begins!");
    }

    private IEnumerator MoveYojiToMona()
    {
        if (yojiInstance == null || monaInstance == null) yield break;

        Vector3 targetPos = monaInstance.transform.position;
        
        while (Vector3.Distance(yojiInstance.transform.position, targetPos) > 0.1f)
        {
            yojiInstance.transform.position = Vector3.MoveTowards(
                yojiInstance.transform.position,
                targetPos,
                yojiMoveSpeed * Time.deltaTime
            );
            
            yield return null;
        }
    }
}