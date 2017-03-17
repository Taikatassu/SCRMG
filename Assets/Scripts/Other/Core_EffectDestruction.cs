using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_EffectDestruction : MonoBehaviour {

    Core_Toolbox toolbox;
    Core_EventManager em;
    ParticleSystem myEffect;

    private void Awake()
    {
        toolbox = FindObjectOfType<Core_Toolbox>();
        em = toolbox.GetComponent<Core_EventManager>();
    }

    private void OnEnable()
    {
        em.OnGameRestart += OnGameRestart;
        em.OnNewSceneLoading += OnNewSceneLoading;
        myEffect = transform.GetComponentInChildren<ParticleSystem>();
    }

    private void OnDisable()
    {
        em.OnGameRestart -= OnGameRestart;
        em.OnNewSceneLoading -= OnNewSceneLoading;
    }

    private void OnGameRestart()
    {
        Destroy(gameObject);
    }

    private void OnNewSceneLoading(int sceneIndex)
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        if (!myEffect.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}
