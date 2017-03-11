using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_LocalPlayerController : Core_ShipController {
    
    Transform closestTarget;
    List<Transform> shipList = new List<Transform>();
    LayerMask mouseRayCollisionLayer = -1;
    bool debugMode = false;
    bool shootingJoystickDown = false;

    #region Initialization
    #region Awake & GetStats
    protected override void Awake()
    {
        base.Awake();
        mouseRayCollisionLayer = LayerMask.NameToLayer("MouseRayCollider");
        GetStats();

        //For testing only
        //StartCoroutine(WaitToDie(5));
    }

    //For testing only
    IEnumerator WaitToDie(float time)
    {
        yield return new WaitForSeconds(time);
        TakeDamage(200);
    }

    protected override void GetStats()
    {
        base.GetStats();
        mouseRayCollisionLayer = LayerMask.NameToLayer(lib.shipVariables.
            mouseRayCollisionLayerName);
        debugMode = lib.gameSettingVariables.debugMode;
    }
    #endregion

    #region OnEnable & OnDisable
    protected override void OnEnable()
    {
        base.OnEnable();
        em.OnShipReference += OnShipReference;
        em.OnMovementInput += OnMovementInput;
        em.OnMousePosition += OnMousePosition;
        em.OnMouseButtonLeftDown += OnMouseButtonLeftDown;
        em.OnMouseButtonLeftUp += OnMouseButtonLeftUp;
        em.OnMouseButtonRightDown += OnMouseButtonRightDown;
        em.OnMouseButtonRightUp += OnMouseButtonRightUp;
        em.OnVirtualJoystickPressed += OnVirtualJoystickPressed;
        em.OnVirtualJoystickReleased += OnVirtualJoystickReleased;
        em.OnVirtualJoystickValueChange += OnVirtualJoystickValueChange;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        em.OnShipReference -= OnShipReference;
        em.OnMovementInput -= OnMovementInput;
        em.OnMousePosition -= OnMousePosition;
        em.OnMouseButtonLeftDown -= OnMouseButtonLeftDown;
        em.OnMouseButtonLeftUp -= OnMouseButtonLeftUp;
        em.OnMouseButtonRightDown -= OnMouseButtonRightDown;
        em.OnMouseButtonRightUp -= OnMouseButtonRightUp;
        em.OnVirtualJoystickPressed -= OnVirtualJoystickPressed;
        em.OnVirtualJoystickReleased -= OnVirtualJoystickReleased;
        em.OnVirtualJoystickValueChange -= OnVirtualJoystickValueChange;
    }
    #endregion
    #endregion

    #region Update
    protected override void Update()
    {
        #region [NOT CURRENTLY IN USE] AutoAim
        //if (!debugMode)
        //{
        //    if (matchStarted)
        //    {
        //        if (shipList.Count > 0)
        //        {
        //            for (int i = 0; i < shipList.Count; i++)
        //            {
        //                if (shipList[i] == null)
        //                {
        //                    shipList.RemoveAt(i);
        //                    i--;
        //                }
        //                else if (closestTarget == null)
        //                {
        //                    closestTarget = shipList[i];
        //                }
        //                else
        //                {
        //                    if (DistanceToObject(shipList[i].position) < DistanceToObject(closestTarget.position))
        //                    {
        //                        closestTarget = shipList[i];
        //                    }
        //                }
        //            }
        //            if (closestTarget != null)
        //            {
        //                SetLookTargetPosition(closestTarget.position);
        //            }
        //        }

        //    }
        //}
        #endregion

        if (shootingJoystickDown)
        {
            Shoot();
        }

        base.Update();
    }

    private float DistanceToObject(Vector3 objectPosition)
    {
        return Vector3.Distance(transform.position, objectPosition);
    }
    #endregion

    #region Subscribers
    #region Input subscribers
    private void OnMovementInput(int controllerIndex, Vector2 movementInputVector)
    {
        if (debugMode)
        {
            if (controllerIndex == index)
            {
                movementDirection.x = movementInputVector.x;
                movementDirection.z = movementInputVector.y;
                movementDirection.y = 0;
            }
        }
    } 

    private void OnMousePosition(int controllerIndex, Vector2 mousePosition)
    {
        if (debugMode)
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
    }

    private void OnMouseButtonLeftDown(int controllerIndex)
    {
        if (debugMode)
        {
            Shoot();
        }
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

    private void OnVirtualJoystickPressed(int joystickIndex)
    {
        //TODO: Get shooting joystickIndex from GVL and implement a check in case 
        //          of a setting where player can swap functionality of the sticks from side to side
        if (joystickIndex == 2)
        {
            shootingJoystickDown = true;
            rotatingTurret = true;
        }
    }

    private void OnVirtualJoystickReleased(int joystickIndex)
    {
        //TODO: See OnVirtualJoystickPressed comment
        if (joystickIndex == 2)
        {
            shootingJoystickDown = false;
            rotatingTurret = false;
        }
    }

    private void OnVirtualJoystickValueChange(int joystickIndex, Vector2 newValue)
    {
        //TODO: See OnVirtualJoystickPressed comment
        if (joystickIndex == 1)
        {
            movementDirection.x = newValue.x;
            movementDirection.z = newValue.y;
            movementDirection.y = 0;
        }
        else if (joystickIndex == 2)
        {
            // Move turret
            SetLookTargetPosition(transform.position + new Vector3(newValue.x, 0, newValue.y));
        }
    }
    #endregion

    #region GameEvent subscribers
    private void OnShipReference(GameObject newShip)
    {
        if(newShip != gameObject)
        {
            shipList.Add(newShip.transform);
        }
    }
    #endregion
    #endregion

}
