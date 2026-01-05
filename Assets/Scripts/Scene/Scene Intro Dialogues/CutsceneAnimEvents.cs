using UnityEngine;
using System;

public class CutsceneAnimEvents : MonoBehaviour
{
    // Yoji: invoked on each attack hit frame
    public event Action<int> OnHitIndex;

    // Mona: invoked near end of death anim
    public event Action OnDeathFinished;

    // Animation Events (Yoji Attack clips)
    public void AnimEvent_Hit1() => OnHitIndex?.Invoke(1);
    public void AnimEvent_Hit2() => OnHitIndex?.Invoke(2);
    public void AnimEvent_Hit3() => OnHitIndex?.Invoke(3);

    // Animation Event (Mona Die clip)
    public void AnimEvent_DeathFinished() => OnDeathFinished?.Invoke();
}
