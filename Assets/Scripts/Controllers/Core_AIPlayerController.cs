using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_AIPlayerController : Core_ShipController {

    #region Awake
    protected override void Awake()
    {
        base.Awake();
        GetStats();
    }
    #endregion

    #region OnEnable & OnDisable
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
    #endregion
}
