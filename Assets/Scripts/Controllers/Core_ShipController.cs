using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_ShipController : MonoBehaviour {

    /* TODO:
     * Implement PlayerController
     *      -When game is started, ONE player controller is assigned to ONE ship 
     *      -(Other ships are controlled by AI or through network by other players)
     *      -PlayerController gets input from inputManager and sends them onward to the player's ship
    */

    Core_Toolbox toolbox;
    Core_EventManager em;
    Rigidbody rb;
    Vector3 movementDirection;
    Core_ShipColorablePartTag[] shipColorableParts;
    int index = 0;
    float movementSpeed = 10;
    bool isMovable = false;
    bool isVulnerable = false;
    Vector3 lookTargetPosition;
    Transform shipHull;
    Transform shipTurret;
    float shipTurretRotationSpeed = 5;

    private void Awake()
    {
        toolbox = FindObjectOfType<Core_Toolbox>();
        em = toolbox.GetComponent<Core_EventManager>();
        rb = GetComponent<Rigidbody>();
        shipColorableParts = GetComponentsInChildren<Core_ShipColorablePartTag>();
        Debug.Log("shipColorableParts.Length" + shipColorableParts.Length);
        shipHull = GetComponentInChildren<Core_ShipHullTag>().transform;
        shipTurret = GetComponentInChildren<Core_ShipTurretTag>().transform;
    }

    private void FixedUpdate()
    {
        #region Movement
        //TODO: Add lerp to movement?
        if (isMovable)
        {
            rb.MovePosition(transform.position + movementDirection * movementSpeed * Time.fixedDeltaTime);
            if (movementDirection == Vector3.zero)
            {
                rb.velocity = Vector3.zero;
            }
        }
        #endregion

        #region Turret rotation
        // TODO: Fix shipTurret rotation and mousePosition raycasting

        Vector3 lookDirection = lookTargetPosition - shipTurret.position;
        float step = shipTurretRotationSpeed * Time.fixedDeltaTime;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, lookDirection, step, 0.5f);
        shipTurret.rotation = Quaternion.LookRotation(newDirection);

        //lookTargetPosition.y = shipTurret.position.y;
        //shipTurret.rotation = Quaternion.FromToRotation(shipTurret.position, lookTargetPosition);
        
        //Debug.Log("lookDirection: " + lookDirection);
        //lookDirection.y = shipTurret.position.y;
        //shipTurret.LookAt(lookDirection);
        #endregion
    }

    public void GiveIndex(int newIndex)
    {
        if (index == 0)
        {
            index = newIndex;
        }
    }

    public int GetIndex()
    {
        return index;
    }

    #region SetVariables
    public void SetMovementDirection(Vector3 newMovementDirection)
    {
        movementDirection = newMovementDirection;
    }

    public void SetLookTargetPosition(Vector3 newLookTargetPosition)
    {
        lookTargetPosition = newLookTargetPosition;
    }

    public void SetIsMoveable(bool state)
    {
        isMovable = state;
    }

    public void SetIsVulnerable(bool state)
    {
        isVulnerable = state;
    }

    public void SetShipColor(Color newColor)
    {
        Debug.Log("SetShipColor called");
        for (int i = 0; i < shipColorableParts.Length; i++)
        {
            shipColorableParts[i].GetComponent<Renderer>().material.color = newColor;
        }
    }
    #endregion
}
