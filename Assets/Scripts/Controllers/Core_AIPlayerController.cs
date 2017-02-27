using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_AIPlayerController : Core_ShipController {
    
    Vector3 movementDirection;

    protected override void Awake()
    {
        base.Awake();
        GetStats();
    }

    #region OnEnable & OnDisable
    private void OnEnable()
    {
        em.OnMatchBeginTimerValue += OnMatchBeginTimerValue;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        em.OnMatchBeginTimerValue -= OnMatchBeginTimerValue;
    }
    #endregion

    #region Subscribers
    private void OnMatchBeginTimerValue(int currentTimerValue)
    {
        if(currentTimerValue == 0)
        {
            Resurrect();
            SetIsMoveable(true);
            SetIsVulnerable(true);
            SetCanShoot(true);
        }
    }
    #endregion
}
