using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_LocalPlayerController : MonoBehaviour {

    Core_Toolbox toolbox;
    Core_EventManager em;
    Vector3 movementDirection;
    Vector3 lookDirection;
    Core_ShipController playerShipController;
    int index = 0;
    LayerMask mouseRayCollisionLayer;
    float mouseRayDistance = 1000;

    private void Awake()
    {
        toolbox = FindObjectOfType<Core_Toolbox>();
        em = toolbox.GetComponent<Core_EventManager>();
        playerShipController = GetComponent<Core_ShipController>();
        index = playerShipController.GetIndex();
        mouseRayCollisionLayer = LayerMask.NameToLayer("MouseRayCollider");
    }

    #region OnEnable & OnDisable
    private void OnEnable()
    {
        em.OnMovementInput += OnMovementInput;
        em.OnMatchBeginTimerValue += OnMatchBeginTimerValue;
        em.OnMousePosition += OnMousePosition;
    }

    private void OnDisable()
    {
        em.OnMovementInput -= OnMovementInput;
        em.OnMatchBeginTimerValue -= OnMatchBeginTimerValue;
        em.OnMousePosition -= OnMousePosition;
    }
    #endregion

    #region Subscribers
    #region Input subscribers
    private void OnMovementInput(int controllerIndex, Vector2 movementInputVector)
    {
        if (controllerIndex == index)
        {
            movementDirection.x = movementInputVector.x;
            movementDirection.z = movementInputVector.y;
            movementDirection.y = 0;
            playerShipController.SetMovementDirection(movementDirection);
        }
    }

    private void OnMatchBeginTimerValue(int currentTimerValue)
    {
        if(currentTimerValue == 0)
        {
            playerShipController.SetIsMoveable(true);
            playerShipController.SetIsVulnerable(true);
        }
    }

    private void OnMousePosition(int controllerIndex, Vector2 mousePosition)
    {
        if (controllerIndex == index)
        {
            Vector3 mousePositionInWorld = Camera.main.ScreenToWorldPoint(mousePosition);
            Debug.DrawRay(mousePositionInWorld, -Vector3.up * 40, Color.red);
            RaycastHit hit;
            if (Physics.Raycast(mousePositionInWorld, -Vector3.up, out hit))
            {
                if (hit.collider.gameObject.layer == mouseRayCollisionLayer)
                {
                    Debug.Log("hit.point" + hit.point);
                    playerShipController.SetLookTargetPosition(hit.point);
                }
            }
        }
    }
    #endregion
    #endregion

}
