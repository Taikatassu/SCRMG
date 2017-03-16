using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_AIPlayerController : Core_ShipController {

    // TODO: Implement an AI that evaluates the game situation decides 
    // actions based on it and controls the ship accordingly

    #region References & variables
    // TODO: Use this to pause AI behaviour while game is paused

    //References
    Transform closestTarget;
    Transform currentTarget;
    List<Transform> shipList = new List<Transform>();
    //Values coming from within the script
    Vector3 newMovementDirection = Vector3.zero;
    Vector3 oldMovementDirection = Vector3.zero;
    int closestTargetTimer = -1;
    int closestTargetTimerFrames = -1;
    int changeDirectionTimer = -1;
    int changeDirectionTimerFrames = -1;
    float directionChangeLerpStartTime = -1;
    bool currentTargetNoLongerClosest = false;
    bool directionChangeLerping = false;
    bool isPaused = false;
    //Values coming from GlobalVariableLibrary
    //float preferredMinDistanceToTarget = 5;
    float closestTargetTimerDuration = -1;
    float changeDirectionTimerDuration = -1;
    float directionChangeLerpDuration = -1;
    float shootingRange = -1;
    float preferredMaxDistanceToTarget = -1;
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


        closestTargetTimerDuration = lib.aiVariables.closestTargetTimerDuration;
        changeDirectionTimerDuration = lib.aiVariables.changeDirectionTimerDuration;
        directionChangeLerpDuration = lib.aiVariables.directionChangeLerpDuration;
        shootingRange = lib.aiVariables.shootingRange;
        //preferredMinDistanceToTarget = lib.aiVariables.preferredMinDistanceToTarget;
        closestTargetTimerFrames = Mathf.RoundToInt(closestTargetTimerDuration / Time.fixedDeltaTime);
        changeDirectionTimerFrames = Mathf.RoundToInt(changeDirectionTimerDuration / Time.fixedDeltaTime);
        changeDirectionTimer = 0;
        preferredMaxDistanceToTarget = lib.aiVariables.preferredMaxDistanceToTarget;
    }
    #endregion

    #region OnEnable & OnDisable
    protected override void OnEnable()
    {
        base.OnEnable();
        em.OnPauseOn += OnPauseOn;
        em.OnPauseOff += OnPauseOff;
        em.OnShipReference += OnShipReference;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        em.OnPauseOn -= OnPauseOn;
        em.OnPauseOff -= OnPauseOff;
        em.OnShipReference -= OnShipReference;
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

    private Vector3 RandomizeDirection(int minAngle, int maxAngle)
    {
        //int x = Random.Range(-1, 1);
        //int z = Random.Range(-1, 1);

        //Vector3 randomizedDirection = new Vector3(x, 0, z);
        //randomizedDirection.Normalize();
        //movementDirection = randomizedDirection;
        int newAngle = Random.Range(minAngle, maxAngle);

        return Quaternion.AngleAxis(newAngle, Vector3.up) * transform.forward;
    }

    protected override void Update()
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
                        //Debug.Log("AI: ShipList element was null, removing");
                        shipList.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        if (currentTarget == null)
                        {
                            currentTarget = shipList[i];
                            closestTarget = currentTarget;
                            //Debug.Log("AI " + index + ": currentTarget was null, set to shipList element " + 
                            //    i + ", " + shipList[i].gameObject.name);
                        }

                        if (DistanceToObject(shipList[i].position) < DistanceToObject(currentTarget.position))
                        {
                            //Debug.Log("AI: closest target found!");
                            closestTarget = shipList[i];
                        }
                    }
                }
                //Debug.Log("closestTarget.gameObject: " + closestTarget.gameObject.name + 
                //    ", currentTarget.gameObject: " + currentTarget.gameObject.name);
                if (closestTarget != null && currentTarget != null)
                {
                    if (closestTarget.gameObject == currentTarget.gameObject)
                    {
                        //Debug.Log("currentTarget is closestTarget");
                        currentTargetNoLongerClosest = false;
                        closestTargetTimer = closestTargetTimerFrames;
                    }
                    else
                    {
                        //Debug.Log("currentTarget is NOT closestTarget");
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

        #region [WIP] Experimental Movement behaviour
        //if (matchStarted && !isPaused)
        //{

        //    if (currentTarget != null)
        //    {
        //        Vector3 direction = currentTarget.position - transform.position;
        //        float distanceToTarget = direction.magnitude;
        //        if (distanceToTarget < preferredMinDistanceToTarget)
        //        {
        //            Debug.Log("AI " + index + "distanceToTarget < preferredMinDistanceToTarget, changeDirectionTimer: " + changeDirectionTimer);
        //            //Move to random direction
        //            if (changeDirectionTimer <= 0)
        //            {
        //                float angle = Vector3.Angle(movementDirection, currentTarget.position);
        //                Debug.Log("AI " + index + ": angle: " + angle);
        //                RandomizeDirection((int)angle - 90, (int)angle + 90);
        //                changeDirectionTimer = changeDirectionTimerFrames;
        //                changeDirectionTimerOn = true;
        //            }
        //        }
        //        else
        //        {
        //            if (distanceToTarget > 1)
        //            {
        //                //Debug.Log("AI " + index + "distanceToTarget > 1");
        //                direction = new Vector3(direction.x / distanceToTarget, 
        //                    direction.y / distanceToTarget, direction.z / distanceToTarget);
        //            }
        //            movementDirection = direction;
        //        }
        //    }
        //    else
        //    {
        //        Debug.Log("AI " + index + ": currentTarget == null");
        //    }
        //}
        #endregion

        #region Movement behaviour
        if (!isPaused)
        {
            if (changeDirectionTimer <= 0)
            {
                newMovementDirection = RandomizeDirection(-180, 180).normalized;
                changeDirectionTimer = changeDirectionTimerFrames;

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
        if (DistanceToObject(currentTarget.position) > preferredMaxDistanceToTarget)
        {
            newMovementDirection = (currentTarget.position - transform.position).normalized;
            changeDirectionTimer = changeDirectionTimerFrames;

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
                Shoot();
            }
        }
        #endregion

        base.Update();
    }

    private float DistanceToObject(Vector3 objectPosition)
    {
        return Vector3.Distance(transform.position, objectPosition);
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
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Environment"))
        {
            //float angle = Vector3.Angle(transform.forward, movementDirection);
            //newMovementDirection = RandomizeDirection((int)-angle - 45, (int)-angle + 45);
            //changeDirectionTimer = changeDirectionTimerFrames;

            //oldMovementDirection = movementDirection;
            //directionChangeLerpStartTime = Time.time;
            //directionChangeLerping = true;
            movementDirection = -movementDirection;
        }
    }
}
