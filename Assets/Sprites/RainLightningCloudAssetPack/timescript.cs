using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timescript : MonoBehaviour
{

    public float timefloat = 0.1f;
    ParticleSystem lightning;
    // Start is called before the first frame update
    void Start()
    {
        lightning = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    [System.Obsolete]
    void Update()
    {
        lightning.playbackSpeed = timefloat;
    }
}
