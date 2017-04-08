using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayerController : ShipController {

    #region Awake
    protected override void Awake()
    {
        base.Awake();
        GetStats();
        Debug.Log("NetworkPlayerController awake");
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
