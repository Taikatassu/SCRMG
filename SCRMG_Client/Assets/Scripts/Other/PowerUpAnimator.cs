using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpAnimator : MonoBehaviour {

    #region References & variables
    Toolbox toolbox;
    EventManager em;
    //GlobalVariableLibrary lib;
    Transform floatingPart;
    Transform rotatingPart;
    Transform tiltingPart;
    Vector3 originalPosition;
    Vector3 newPosition;
    float originalY;
    float floatingDistance = 0.25f;
    float floatingSpeed = 2f;
    bool isPaused = false;
    #endregion

    #region Awake
    private void Awake()
    {
        toolbox = FindObjectOfType<Toolbox>();
        em = toolbox.GetComponent<EventManager>();
        //lib = toolbox.GetComponent<GlobalVariableLibrary>();
        floatingPart = transform.GetChild(0);
        rotatingPart = floatingPart.GetChild(0);
        tiltingPart = rotatingPart.GetChild(0);
        originalPosition = floatingPart.position;
        originalY = floatingPart.position.y;
    }
    #endregion

    #region OnEnable & OnDisable
    private void OnEnable ()
    {
        em.OnPauseOn += OnPauseOn;
        em.OnPauseOff += OnPauseOff;
        floatingPart.position = originalPosition;
        newPosition = originalPosition;
    }

    private void OnDisable()
    {
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

    #region Update
    private void Update ()
    {
        if (!isPaused)
        {
            rotatingPart.Rotate(new Vector3(0, 0.25f, 0));
            tiltingPart.Rotate(new Vector3(0.25f, 0, 0));
            newPosition.y = originalY + floatingDistance * Mathf.Sin(floatingSpeed * Time.time);
            floatingPart.position = newPosition;
        }
    }
    #endregion
}
