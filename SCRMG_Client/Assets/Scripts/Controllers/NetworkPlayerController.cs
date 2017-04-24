using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayerController : ShipController
{
    Vector3 previousShipPosition = Vector3.zero;
    Vector3 previousTurretRotation = Vector3.zero;
    //Vector3 previousMovementDirection = Vector3.zero;
    Vector3 previousTurretTargetPosition = Vector3.zero;
    Vector3 positionLerpStartPosition = Vector3.zero;
    Vector3 positionLerpEndPosition = Vector3.zero;
    float positionLerpingStartTime = -1;
    bool isLerpingToPosition = false;
    bool gameEnded = false;

    float positionLerpDuration = -1;

    #region Awake
    protected override void Awake()
    {
        base.Awake();
        GetStats();
    }
    #endregion

    #region GetStats
    protected override void GetStats()
    {
        positionLerpDuration = 1 / lib.networkingVariables.shipInfoUpdatesPerSecond;
        base.GetStats();
    }
    #endregion

    #region OnEnable & OnDisable
    protected override void OnEnable()
    {
        base.OnEnable();

        em.OnProjectileSpawnedByServer += OnProjectileSpawnedByServer;
        em.OnMatchEndedByServer += OnMatchEndedByServer;
        em.OnExitNetworkMultiplayerMidGame += OnExitNetworkMultiplayerMidGame;

        isControllerByServer = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        em.OnProjectileSpawnedByServer -= OnProjectileSpawnedByServer;
        em.OnMatchEndedByServer -= OnMatchEndedByServer;
        em.OnExitNetworkMultiplayerMidGame -= OnExitNetworkMultiplayerMidGame;
    }
    #endregion

    #region Subscribers
    private void OnProjectileSpawnedByServer(int projectileOwnerIndex, int projectileIndex, Vector3 spawnPosition, Vector3 spawnRotation)
    {
        if(projectileOwnerIndex == index)
        {
            currentProjectileIndices.Add(projectileIndex);

            GameObject newProjectile = Instantiate(Resources.Load("Projectiles/Projectile", typeof(GameObject)),
                    spawnPosition, Quaternion.Euler(spawnRotation)) as GameObject;

            Physics.IgnoreCollision(newProjectile.GetComponent<Collider>(),
                GetComponentInChildren<Collider>());

            Projectile newProjectileScript = newProjectile.GetComponent<Projectile>();
            newProjectileScript.InitializeProjectile(index, projectileIndex, 0, GetShipColor(), true);
        }
    }

    private void OnMatchEndedByServer(string winnerName, bool localPlayerWins)
    {
        gameEnded = true;
    }

    private void OnExitNetworkMultiplayerMidGame()
    {
        gameEnded = true;
    }
    #endregion

    private void StartLerpingToPosition(Vector3 endPosition)
    {
        positionLerpStartPosition = transform.position;
        positionLerpEndPosition = endPosition;
        positionLerpingStartTime = Time.time;
        isLerpingToPosition = true;
    }

    #region FixedUpdate
    protected override void FixedUpdate()
    {
        if (!gameEnded)
        {
            if (myShipInfoElement == -1)
            {
                myShipInfoElement = shipInfoManager.GetMyShipInfoElement(index);
            }

            if (myShipInfoElement != -1)
            {
                if (shipInfoManager.shipInfoList.Count >= myShipInfoElement)
                {
                    //transform.position = shipInfoManager.shipInfoList[myShipInfoElement].shipPosition;
                    //shipHull.eulerAngles = shipInfoManager.shipInfoList[myShipInfoElement].hullRotation;
                    //shipTurret.eulerAngles = shipInfoManager.shipInfoList[myShipInfoElement].turretRotation;

                    Vector3 currentPosition = transform.position;
                    Vector3 newPosition = shipInfoManager.shipInfoList[myShipInfoElement].shipPosition;
                    if (previousShipPosition != newPosition)
                    {
                        StartLerpingToPosition(newPosition);
                        previousShipPosition = newPosition;
                    }

                    Vector3 newRotation = shipInfoManager.shipInfoList[myShipInfoElement].turretRotation;
                    if (previousTurretRotation != newRotation)
                    {
                        previousTurretTargetPosition = Quaternion.Euler(newRotation) * Vector3.forward;
                    }
                    SetLookTargetPosition(currentPosition + new Vector3(previousTurretTargetPosition.x, 0, previousTurretTargetPosition.y));

                    previousTurretRotation = newRotation;
                }
            }

            if (isLerpingToPosition)
            {
                float timeSinceStarted = Time.time - positionLerpingStartTime;
                float percentageComplete = timeSinceStarted / positionLerpDuration;

                if(percentageComplete > 1.0f)
                {
                    percentageComplete = 1.0f;
                }

                transform.position = Vector3.Lerp(positionLerpStartPosition, positionLerpEndPosition, percentageComplete);
            }

            base.FixedUpdate();
        }
    }
    #endregion

}
