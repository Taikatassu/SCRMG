using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_LocalPlayerController : Core_ShipController {
    
    Vector3 movementDirection;
    Vector3 lookDirection;
    LayerMask mouseRayCollisionLayer;

    protected override void Awake()
    {
        base.Awake();
        mouseRayCollisionLayer = LayerMask.NameToLayer("MouseRayCollider");
        GetStats();
    }

    #region OnEnable & OnDisable
    private void OnEnable()
    {
        em.OnMovementInput += OnMovementInput;
        em.OnMatchBeginTimerValue += OnMatchBeginTimerValue;
        em.OnMousePosition += OnMousePosition;
        em.OnMouseButtonLeftDown += OnMouseButtonLeftDown;
        em.OnMouseButtonLeftUp += OnMouseButtonLeftUp;
        em.OnMouseButtonRightDown += OnMouseButtonRightDown;
        em.OnMouseButtonRightUp += OnMouseButtonRightUp;
    }

    private void OnDisable()
    {
        em.OnMovementInput -= OnMovementInput;
        em.OnMatchBeginTimerValue -= OnMatchBeginTimerValue;
        em.OnMousePosition -= OnMousePosition;
        em.OnMouseButtonLeftDown -= OnMouseButtonLeftDown;
        em.OnMouseButtonLeftUp -= OnMouseButtonLeftUp;
        em.OnMouseButtonRightDown -= OnMouseButtonRightDown;
        em.OnMouseButtonRightUp -= OnMouseButtonRightUp;
    }
    #endregion

    #region Subscribers
    #region Game event subscribers
    private void OnMatchBeginTimerValue(int currentTimerValue)
    {
        if (currentTimerValue == 0)
        {
            Resurrect();
            SetIsMoveable(true);
            SetIsVulnerable(true);
            SetCanShoot(true);
        }
    }
    #endregion

    #region Input subscribers
    private void OnMovementInput(int controllerIndex, Vector2 movementInputVector)
    {
        if (controllerIndex == index)
        {
            movementDirection.x = movementInputVector.x;
            movementDirection.z = movementInputVector.y;
            movementDirection.y = 0;
            SetMovementDirection(movementDirection);
        }
    }

    private void OnMousePosition(int controllerIndex, Vector2 mousePosition)
    {
        if (controllerIndex == index)
        {
            Vector3 mousePositionInWorld = Camera.main.ScreenToWorldPoint(mousePosition);
            Debug.DrawRay(mousePositionInWorld, -Vector3.up * 10, Color.red);
            RaycastHit hit;
            if (Physics.Raycast(mousePositionInWorld, -Vector3.up, out hit))
            {
                if (hit.collider.gameObject.layer == mouseRayCollisionLayer)
                {
                    SetLookTargetPosition(hit.point);
                }
            }
        }
    }

    private void OnMouseButtonLeftDown(int controllerIndex)
    {
        Shoot();
    }

    private void OnMouseButtonLeftUp(int controllerIndex)
    {
        //Debug.Log("OnMouseButtonLeftUp");
    }

    private void OnMouseButtonRightDown(int controllerIndex)
    {
        //Debug.Log("OnMouseButtonRightDown");
    }

    private void OnMouseButtonRightUp(int controllerIndex)
    {
        //Debug.Log("OnMouseButtonRightUp");
    }
    #endregion
    #endregion

}
