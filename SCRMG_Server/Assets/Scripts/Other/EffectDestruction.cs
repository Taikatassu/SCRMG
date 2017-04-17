using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectDestruction : MonoBehaviour
{

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
        //em.OnGameRestart += OnGameRestart;
        myEffect = transform.GetComponentInChildren<ParticleSystem>();
    }

    private void OnDisable()
    {
        //em.OnGameRestart -= OnGameRestart;
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
