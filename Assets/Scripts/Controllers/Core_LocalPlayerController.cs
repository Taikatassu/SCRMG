using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_LocalPlayerController : Core_ShipController {

    Transform playerTarget;
    Transform closestTarget;
    List<Transform> shipList = new List<Transform>();
    LayerMask mouseRayCollisionLayer = -1;
    bool debugMode = false;
    int counter = 0;

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
        em.OnShootButtonPressed += OnShootButtonPressed;
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
        em.OnShootButtonPressed -= OnShootButtonPressed;
    }
    #endregion
    #endregion

    #region Update
    protected override void Update()
    {
        if (!debugMode)
        {
            if (matchStarted)
            {
                if (shipList.Count > 0)
                {
                    for (int i = 0; i < shipList.Count; i++)
                    {
                        if (shipList[i] == null)
                        {
                            shipList.RemoveAt(i);
                            i--;
                        }
                        else if (closestTarget == null)
                        {
                            closestTarget = shipList[i];
                            Debug.Log("closestTarget was null, set to shipList element " + i);
                        }
                        else
                        {
                            if (DistanceToObject(shipList[i].position) < DistanceToObject(closestTarget.position))
                            {
                                Debug.Log("New closest target found!");
                                closestTarget = shipList[i];
                            }
                        }
                    }
                    if (closestTarget != null)
                    {
                        SetLookTargetPosition(closestTarget.position);
                    }
                }

            }
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
                //SetMovementDirection(movementDirection);
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

    private void OnShootButtonPressed()
    {
        Shoot();
    }
    #endregion

    #region GameEvent subscribers
    private void OnShipReference(GameObject newShip)
    {
        if(newShip != gameObject)
        {
            shipList.Add(newShip.transform);
            Debug.Log("Ship reference got: " + ++counter);
        }
    }
    #endregion
    #endregion

}
