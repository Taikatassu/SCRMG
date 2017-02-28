using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_AIPlayerController : Core_ShipController {

    //Vector3 movementDirection;

    #region Awake
    protected override void Awake()
    {
        base.Awake();
        GetStats();
    }
    #endregion

    #region OnEnable & OnDisable
    private void OnEnable()
    {
        em.OnMatchBeginTimerValue += OnMatchBeginTimerValue;
        em.OnGameRestart += OnGameRestart;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        em.OnMatchBeginTimerValue -= OnMatchBeginTimerValue;
        em.OnGameRestart -= OnGameRestart;
    }
    #endregion

    #region Subscribers
    private void OnMatchBeginTimerValue(int currentTimerValue)
    {
        if(currentTimerValue == 0)
        {
            //Resurrect(); //Replaced by below(AddHealth)
            AddHealth(maxHealth);
            SetIsMoveable(true);
            SetIsVulnerable(true);
            SetCanShoot(true);
        }
    }

    private void OnGameRestart()
    {
        //TODO: Change if implementing a pool for ships instead of instantiating them
        Destroy(gameObject);
    }
    #endregion
}
