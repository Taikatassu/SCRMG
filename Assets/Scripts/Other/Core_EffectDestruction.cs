using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_EffectDestruction : MonoBehaviour {

    ParticleSystem myEffect;

    private void OnEnable()
    {
        myEffect = transform.GetComponentInChildren<ParticleSystem>();
    }

    private void Update()
    {
        if (!myEffect.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}
