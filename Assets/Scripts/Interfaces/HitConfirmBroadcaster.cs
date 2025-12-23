using UnityEngine;
using System.Collections.Generic;

public class HitConfirmBroadcaster : MonoBehaviour
{
    private readonly List<IHitConfirmListener> listeners = new List<IHitConfirmListener>();

    public void Register(IHitConfirmListener l)
    {
        if (l == null) return;
        if (!listeners.Contains(l)) listeners.Add(l);
    }

    public void Unregister(IHitConfirmListener l)
    {
        if (l == null) return;
        listeners.Remove(l);
    }

    public void NotifyHit(GameObject target)
    {
        for (int i = 0; i < listeners.Count; i++)
            listeners[i].OnHitConfirmed(target);
    }
}
