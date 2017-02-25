using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_AIPlayerController : MonoBehaviour {

    Core_Toolbox toolbox;
    Core_EventManager em;
    Vector3 movementDirection;
    Core_ShipController aiShipController;
    int index = 0;

    private void Awake()
    {
        toolbox = FindObjectOfType<Core_Toolbox>();
        em = toolbox.GetComponent<Core_EventManager>();
        aiShipController = GetComponent<Core_ShipController>();
        index = aiShipController.GetIndex();
    }

    #region OnEnable & OnDisable
    private void OnEnable()
    {
        em.OnMatchBeginTimerValue += OnMatchBeginTimerValue;
    }

    private void OnDisable()
    {
        em.OnMatchBeginTimerValue -= OnMatchBeginTimerValue;
    }
    #endregion

    #region Subscribers
    private void OnMatchBeginTimerValue(int currentTimerValue)
    {
        if(currentTimerValue == 0)
        {
            aiShipController.SetIsMoveable(true);
            aiShipController.SetIsVulnerable(true);
        }
    }
    #endregion
}
