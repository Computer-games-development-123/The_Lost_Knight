using UnityEngine;
using UnityEngine.SceneManagement;

public class FormSwitcher : MonoBehaviour
{
    [SerializeField] private Animator anim;

    [Header("Overrides")]
    [SerializeField] private RuntimeAnimatorController normalOverride;
    [SerializeField] private RuntimeAnimatorController fireOverride;

    [Header("Save Flag")]
    [SerializeField] private GameFlag fireFormFlag = GameFlag.hasUpgradedSword;

    private void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        GameManagerReadyHelper.RunWhenReady(this, ApplySavedForm);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySavedForm();
    }

    private void ApplySavedForm()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsProgressLoaded)
        {
            SwitchToNormalFormImmediate();
            return;
        }

        bool hasFire = GameManager.Instance.GetFlag(GameFlag.hasUpgradedSword);
        if (hasFire) SwitchToFireFormImmediate();
        else SwitchToNormalFormImmediate();
    }

    public void StartTransformation()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFlag(fireFormFlag, true);
            GameManager.Instance.SaveProgress();
        }

        SwitchToNormalFormImmediate();

        if (anim != null)
            anim.SetTrigger("Transform");

    }

    public void OnTransformFinished()
    {
        SwitchToFireFormImmediate();
    }

    public void SwitchToFireFormImmediate()
    {
        if (anim != null && fireOverride != null)
            anim.runtimeAnimatorController = fireOverride;
    }

    public void SwitchToNormalFormImmediate()
    {
        if (anim != null && normalOverride != null)
            anim.runtimeAnimatorController = normalOverride;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ProgressLoaded -= ApplySavedForm;
    }
}
