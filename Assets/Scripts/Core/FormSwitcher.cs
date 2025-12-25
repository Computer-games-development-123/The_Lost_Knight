using UnityEngine;

public class FormSwitcher : MonoBehaviour
{
    [SerializeField] private Animator anim;

    [Header("Overrides")]
    [SerializeField] private RuntimeAnimatorController normalOverride;
    [SerializeField] private RuntimeAnimatorController fireOverride;

    private void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (normalOverride != null)
            anim.runtimeAnimatorController = normalOverride;
    }

    public void StartTransformation()
    {
        anim.SetTrigger("Transform");
    }

    public void SwitchToFireForm()
    {
        if (fireOverride != null)
            anim.runtimeAnimatorController = fireOverride;
    }
}
