using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectDestruction : MonoBehaviour {

    Toolbox toolbox;
    EventManager em;
    ParticleSystem myEffect;

    private void Awake()
    {
        toolbox = FindObjectOfType<Toolbox>();
        em = toolbox.GetComponent<EventManager>();
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
