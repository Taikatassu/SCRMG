using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerController : ShipController
{
    Transform closestTarget;
    //List<Transform> shipList = new List<Transform>();
    LayerMask mouseRayCollisionLayer = -1;
    Vector2 turretJoystickValue = Vector2.zero;
    bool isShooting = false;

    #region Initialization
    #region Awake & GetStats
    protected override void Awake()
    {
        base.Awake();
        mouseRayCollisionLayer = LayerMask.NameToLayer("MouseRayCollider");
        GetStats();
    }

    protected override void GetStats()
    {
        base.GetStats();
        mouseRayCollisionLayer = LayerMask.NameToLayer(lib.shipVariables.
            mouseRayCollisionLayerName);
    }
    #endregion

    #region OnEnable & OnDisable
    protected override void OnEnable()
    {
        base.OnEnable();
        //TODO: Remove if autoAim deemed permanently obsolete
        //em.OnShipReference += OnShipReference;

        if (buildPlatform == 0)
        {
            em.OnMovementInput += OnMovementInput;
            em.OnMousePosition += OnMousePosition;
            em.OnMouseButtonLeftDown += OnMouseButtonLeftDown;
            em.OnMouseButtonLeftUp += OnMouseButtonLeftUp;
            em.OnMouseButtonRightDown += OnMouseButtonRightDown;
            em.OnMouseButtonRightUp += OnMouseButtonRightUp;
        }
        else if (buildPlatform == 1)
        {
            em.OnVirtualJoystickPressed += OnVirtualJoystickPressed;
            em.OnVirtualJoystickReleased += OnVirtualJoystickReleased;
            em.OnVirtualJoystickValueChange += OnVirtualJoystickValueChange;
        }

        isControllerByServer = false;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        //TODO: Remove if autoAim deemed permanently obsolete
        //em.OnShipReference -= OnShipReference;

        if (buildPlatform == 0)
        {
            em.OnMovementInput -= OnMovementInput;
            em.OnMousePosition -= OnMousePosition;
            em.OnMouseButtonLeftDown -= OnMouseButtonLeftDown;
            em.OnMouseButtonLeftUp -= OnMouseButtonLeftUp;
            em.OnMouseButtonRightDown -= OnMouseButtonRightDown;
            em.OnMouseButtonRightUp -= OnMouseButtonRightUp;
        }
        else if (buildPlatform == 1)
        {
            em.OnVirtualJoystickPressed -= OnVirtualJoystickPressed;
            em.OnVirtualJoystickReleased -= OnVirtualJoystickReleased;
            em.OnVirtualJoystickValueChange -= OnVirtualJoystickValueChange;
        }
    }
    #endregion

    #region Subscribers
    #region Input subscribers
    private void OnMovementInput(int controllerIndex, Vector2 movementInputVector)
    {
        if (buildPlatform == 0)
        {
            if (controllerIndex == index || (currentGameModeIndex == gameModeNetworkMultiplayerIndex && controllerIndex == 1))
            {
                movementDirection.x = movementInputVector.x;
                movementDirection.z = movementInputVector.y;
                movementDirection.y = 0;
            }
        }
    }

    private void OnMousePosition(int controllerIndex, Vector2 mousePosition)
    {
        if (buildPlatform == 0)
        {
            if (controllerIndex == index || (currentGameModeIndex == gameModeNetworkMultiplayerIndex && controllerIndex == 1))
            {
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);

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
        if (controllerIndex == index || (currentGameModeIndex == gameModeNetworkMultiplayerIndex && controllerIndex == 1))
        {
            isShooting = true;
        }
    }

    private void OnMouseButtonLeftUp(int controllerIndex)
    {
        if (controllerIndex == index || (currentGameModeIndex == gameModeNetworkMultiplayerIndex && controllerIndex == 1))
        {
            isShooting = false;
            EndPersistingProjectile();
        }
    }

    private void OnMouseButtonRightDown(int controllerIndex)
    {
        if (controllerIndex == index || (currentGameModeIndex == gameModeNetworkMultiplayerIndex && controllerIndex == 1))
        {
            //Debug.Log("OnMouseButtonRightDown");
        }
    }

    private void OnMouseButtonRightUp(int controllerIndex)
    {
        if (controllerIndex == index || (currentGameModeIndex == gameModeNetworkMultiplayerIndex && controllerIndex == 1))
        {
            //Debug.Log("OnMouseButtonRightUp");
        }
    }

    private void OnVirtualJoystickPressed(int joystickIndex)
    {
        if (joystickIndex == 2)
        {
            isShooting = true;
            rotatingTurret = true;
        }
    }

    private void OnVirtualJoystickReleased(int joystickIndex)
    {
        if (joystickIndex == 2)
        {
            isShooting = false;
            EndPersistingProjectile();
            rotatingTurret = false;
        }
    }

    private void OnVirtualJoystickValueChange(int joystickIndex, Vector2 newValue)
    {
        if (joystickIndex == 1)
        {
            //Move ship
            movementDirection.x = newValue.x;
            movementDirection.z = newValue.y;
            movementDirection.y = 0;
        }
        else if (joystickIndex == 2)
        {
            turretJoystickValue = newValue;
        }
    }
    #endregion

    #region GameEvent subscribers
    //TODO: Remove if autoAim deemed permanently obsolete
    //Required for autoAim, [NOT CURRENTLY IN USE]
    //private void OnShipReference(GameObject newShip)
    //{
    //    if(newShip != gameObject)
    //    {
    //        shipList.Add(newShip.transform);
    //    }
    //}
    #endregion
    #endregion
    #endregion

    #region Update & FixedUpdate
    protected override void Update()
    {
        if (buildPlatform == 1 && rotatingTurret)
        {
            //Rotate turret
            SetLookTargetPosition(transform.position + new Vector3(turretJoystickValue.x, 0, turretJoystickValue.y));
        }

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

        base.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isShooting)
        {
            Shoot();
        }

        //TODO: Update my info on shipInfoList

        if (myShipInfoElement == -1)
        {
            myShipInfoElement = shipInfoManager.GetMyShipInfoElement(index);
        }

        if (myShipInfoElement != -1)
        {
            shipInfoManager.shipInfoList[myShipInfoElement].shipPosition = transform.position;
            shipInfoManager.shipInfoList[myShipInfoElement].hullRotation = shipHull.eulerAngles;
            shipInfoManager.shipInfoList[myShipInfoElement].turretRotation = shipTurret.eulerAngles;
        }
    }
    #endregion

    #region DistanceToObject
    private float DistanceToObject(Vector3 objectPosition)
    {
        return Vector3.Distance(transform.position, objectPosition);
    }
    #endregion

}
