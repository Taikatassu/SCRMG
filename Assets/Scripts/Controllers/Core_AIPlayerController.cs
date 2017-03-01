using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_AIPlayerController : Core_ShipController {

    // TODO: Implement an AI that evaluates the game situation decides 
    // actions based on it and controls the ship accordingly

    #region References & variables
    // TODO: Use this to pause AI behaviou rwhile game is paused
    bool isPaused = false; 
    #endregion

    #region Awake
    protected override void Awake()
    {
        base.Awake();
        GetStats();
        Debug.Log("AIPlayerController awake");
    }
    #endregion

    #region OnEnable & OnDisable
    protected override void OnEnable()
    {
        base.OnEnable();
        em.OnPauseOn += OnPauseOn;
        em.OnPauseOff += OnPauseOff;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        em.OnPauseOn -= OnPauseOn;
        em.OnPauseOff -= OnPauseOff;
    }
    #endregion

    #region Subscribers
    private void OnPauseOn()
    {
        isPaused = true;
    }

    private void OnPauseOff()
    {
        isPaused = false;
    }
    #endregion
}
