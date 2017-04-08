using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayerController : ShipController {


    #region References & variables
    //References
    Transform closestTarget;
    Transform currentTarget;
    List<Transform> shipList = new List<Transform>();
    //Values coming from within the script
    Vector3 newMovementDirection = Vector3.zero;
    Vector3 oldMovementDirection = Vector3.zero;
    int closestTargetTimer = -1;
    int changeDirectionTimer = -1;
    float directionChangeLerpStartTime = -1;
    bool currentTargetNoLongerClosest = false;
    bool directionChangeLerping = false;
    bool isPaused = false;
    bool isShooting = false;
    //Values coming from GlobalVariableLibrary
    float closestTargetTimerDuration = -1;
    float changeDirectionTimerDuration = -1;
    float directionChangeLerpDuration = -1;
    float shootingRange = -1;
    float preferredMaxDistanceToTarget = -1;
    bool aiDisabled = false;
    #endregion

    #region Awake & GetStates
    protected override void Awake()
    {
        base.Awake();
        GetStats();
    }

    protected override void GetStats()
    {
        base.GetStats();

        //aiDisabled = lib.aiVariables.aiDisabled;
        //closestTargetTimerDuration = lib.aiVariables.closestTargetTimerDuration;
        //changeDirectionTimerDuration = lib.aiVariables.changeDirectionTimerDuration;
        //directionChangeLerpDuration = lib.aiVariables.directionChangeLerpDuration;
        //shootingRange = lib.aiVariables.shootingRange;
        changeDirectionTimer = 0;
        //preferredMaxDistanceToTarget = lib.aiVariables.preferredMaxDistanceToTarget;
    }
    #endregion

    #region OnEnable & OnDisable
    protected override void OnEnable()
    {
        base.OnEnable();
        //em.OnPauseOn += OnPauseOn;
        //em.OnPauseOff += OnPauseOff;
        //em.OnShipReference += OnShipReference;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        //em.OnPauseOn -= OnPauseOn;
        //em.OnPauseOff -= OnPauseOff;
        //em.OnShipReference -= OnShipReference;
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

    private void OnShipReference(GameObject newShip)
    {
        if (newShip != gameObject)
        {
            shipList.Add(newShip.transform);
        }
    }
    #endregion

    private Vector3 GetRandomDirection()
    {
        int x = Random.Range(-1, 1);
        int z = Random.Range(-1, 1);

        if (x == 0 && z == 0)
        {
            int[] validValues = { -1, 1 };
            x = validValues[Random.Range(0, validValues.Length)];
            z = validValues[Random.Range(0, validValues.Length)];
        }

        Vector3 randomizedDirection = new Vector3(x, 0, z);
        randomizedDirection.Normalize();
        return randomizedDirection;
    }

    #region Update & FixedUpdate
    protected override void Update()
    {
        if (!aiDisabled)
        {
            #region Targeting behaviour
            if (!isPaused)
            {
                rotatingTurret = true;
                if (shipList.Count > 0)
                {
                    if (currentTargetNoLongerClosest && closestTargetTimer <= 0)
                    {
                        currentTarget = closestTarget;
                    }

                    for (int i = 0; i < shipList.Count; i++)
                    {
                        if (shipList[i] == null)
                        {
                            shipList.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            if (currentTarget == null)
                            {
                                currentTarget = shipList[i];
                                closestTarget = currentTarget;
                            }

                            if (DistanceToObject(shipList[i].position) < DistanceToObject(currentTarget.position))
                            {
                                closestTarget = shipList[i];
                            }
                        }
                    }
                    if (closestTarget != null && currentTarget != null)
                    {
                        if (closestTarget.gameObject == currentTarget.gameObject)
                        {
                            currentTargetNoLongerClosest = false;
                            closestTargetTimer = Mathf.RoundToInt(closestTargetTimerDuration / Time.fixedDeltaTime);
                        }
                        else
                        {
                            if (!currentTargetNoLongerClosest)
                                currentTargetNoLongerClosest = true;
                        }
                    }

                    if (currentTarget != null)
                    {
                        SetLookTargetPosition(currentTarget.position);
                    }
                }

            }
            else
            {
                rotatingTurret = false;
            }
            #endregion

            #region Movement behaviour
            if (!isPaused)
            {
                if (changeDirectionTimer <= 0)
                {
                    newMovementDirection = GetRandomDirection();
                    changeDirectionTimer = Mathf.RoundToInt(changeDirectionTimerDuration / Time.fixedDeltaTime);

                    oldMovementDirection = movementDirection;
                    directionChangeLerpStartTime = Time.time;
                    directionChangeLerping = true;
                }
            }

            if (directionChangeLerping)
            {
                float timeSinceStarted = Time.time / directionChangeLerpStartTime;
                float percentageCompleted = timeSinceStarted / directionChangeLerpDuration;
                movementDirection = Vector3.Lerp(oldMovementDirection, newMovementDirection, percentageCompleted);

                if (percentageCompleted >= 1)
                {
                    directionChangeLerping = false;
                }
            }

            //If target is too far
            if (currentTarget != null && DistanceToObject(currentTarget.position) > preferredMaxDistanceToTarget)
            {
                newMovementDirection = currentTarget.position - transform.position;
                changeDirectionTimer = Mathf.RoundToInt(changeDirectionTimerDuration / Time.fixedDeltaTime);

                oldMovementDirection = movementDirection;
                directionChangeLerpStartTime = Time.time;
                directionChangeLerping = true;
            }
            #endregion

            #region Shooting behaviour
            if (!isPaused)
            {
                if (currentTarget != null && DistanceToObject(currentTarget.position) < shootingRange)
                {
                    isShooting = true;
                }
                else
                {
                    isShooting = false;
                    EndPersistingProjectile();
                }
            }
            #endregion
        }

        base.Update();
    }

    protected override void FixedUpdate()
    {
        #region ClosestTargetTimer
        if (!isPaused)
        {
            if (currentTargetNoLongerClosest && closestTargetTimer > 0)
            {
                closestTargetTimer--;
                if (closestTargetTimer <= 0)
                {
                    closestTargetTimer = 0;
                }
            }
        }
        #endregion

        #region ChangeDirectionTimer
        if (!isPaused)
        {
            if (changeDirectionTimer > 0)
            {
                changeDirectionTimer--;
                if (changeDirectionTimer <= 0)
                {
                    changeDirectionTimer = 0;
                }
            }
        }
        #endregion

        base.FixedUpdate();

        #region Shooting
        if (isShooting)
        {
            Shoot();
        }
        #endregion
    }
    #endregion

    private float DistanceToObject(Vector3 objectPosition)
    {
        return Vector3.Distance(transform.position, objectPosition);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Environment"))
        {
            RaycastHit hit;
            Vector3 originalDirection = shipHull.forward;
            Vector3 startPoint = shipHull.position;
            if (Physics.Raycast(startPoint, originalDirection, out hit))
            {
                Vector3 newDirection = Vector3.Reflect(originalDirection, hit.normal);
                movementDirection = newDirection;
                changeDirectionTimer = Mathf.RoundToInt(changeDirectionTimerDuration / Time.fixedDeltaTime);
            }
            else
            {
                movementDirection = -movementDirection;
            }
        }
    }
}
