using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Core_HUDJoystickManager : MonoBehaviour {

    // Used as a tag for HUDJoystick

    protected Core_Toolbox toolbox;
    protected Core_EventManager em;

    private void Awake()
    {
        toolbox = FindObjectOfType<Core_Toolbox>();
        em = toolbox.GetComponent<Core_EventManager>();
    }

    public void OnPointerUp()
    {
        em.BroadcastHUDJoystickButtonReleased();
    }

}
