using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayerController : ShipController {

    // TODO: Implement a system that gets inputs through networking system from
    // other players and controls the ship accordingly

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
