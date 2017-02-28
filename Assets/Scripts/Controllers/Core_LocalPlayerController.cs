using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_LocalPlayerController : Core_ShipController {
    
    LayerMask mouseRayCollisionLayer = -1;

    protected override void Awake()
    {
        base.Awake();
        mouseRayCollisionLayer = LayerMask.NameToLayer("MouseRayCollider");
        GetStats();

        //For testing only
        //StartCoroutine(WaitToDie(5));
    }

    protected override void GetStats()
    {
        base.GetStats();
        mouseRayCollisionLayer = LayerMask.NameToLayer(lib.shipVariables.
            mouseRayCollisionLayerName);
    }

    //For testing only
    IEnumerator WaitToDie(float time)
    {
        yield return new WaitForSeconds(time);
        TakeDamage(200);
    }


    #region OnEnable & OnDisable
    private void OnEnable()
    {
        em.OnGameRestart += OnGameRestart;
        em.OnMatchBeginTimerValue += OnMatchBeginTimerValue;
        em.OnMovementInput += OnMovementInput;
        em.OnMousePosition += OnMousePosition;
        em.OnMouseButtonLeftDown += OnMouseButtonLeftDown;
        em.OnMouseButtonLeftUp += OnMouseButtonLeftUp;
        em.OnMouseButtonRightDown += OnMouseButtonRightDown;
        em.OnMouseButtonRightUp += OnMouseButtonRightUp;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        em.OnGameRestart -= OnGameRestart;
        em.OnMatchBeginTimerValue -= OnMatchBeginTimerValue;
        em.OnMovementInput -= OnMovementInput;
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
            Debug.Log("currentTimerValue == 0");
            //Resurrect(); //Replaced by below (AddHealth)
            AddHealth(maxHealth);
            SetIsMoveable(true);
            SetIsVulnerable(true);
            SetCanShoot(true);
        }
    }

    private void OnGameRestart()
    {
        //TODO: Change if implementing a pool for ships instead of instantiating them
        Destroy(gameObject);
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
            //SetMovementDirection(movementDirection);
        }
    } 

    private void OnMousePosition(int controllerIndex, Vector2 mousePosition)
    {
        if (controllerIndex == index)
        {
            //Vector3 mousePositionInWorld = Camera.main.ScreenToWorldPoint(mousePosition);
            //Camera.main.ScreenToViewportPoint();
            //Debug.DrawRay(mousePositionInWorld, -Vector3.up * 10, Color.red);
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            //Debug.DrawRay(ray, Color.red);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Debug.DrawRay(hit.point, Vector3.up * 20, Color.red);
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
