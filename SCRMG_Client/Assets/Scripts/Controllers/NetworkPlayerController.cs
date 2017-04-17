using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayerController : ShipController {

    // TODO: Implement a system that gets inputs through networking system from
    // other players and controls the ship accordingly

    #region Awake
    protected override void Awake()
    {
        base.Awake();
        GetStats();
        Debug.Log("NetworkPlayerController awake");
    }
    #endregion

    #region OnEnable & OnDisable
    protected override void OnEnable()
    {
        base.OnEnable();

        em.OnProjectileSpawnedByServer += OnProjectileSpawnedByServer;

        isControllerByServer = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        em.OnProjectileSpawnedByServer -= OnProjectileSpawnedByServer;
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
            newProjectileScript.InitializeProjectile(index, projectileIndex, 0, GetShipColor());
        }
    }
    #endregion

    Vector3 previousShipPosition = Vector3.zero;
    Vector3 previousTurretRotation = Vector3.zero;
    #region FixedUpdate
    protected override void FixedUpdate()
    {
        if (myShipInfoElement == -1)
        {
            myShipInfoElement = shipInfoManager.GetMyShipInfoElement(index);
        }

        if (myShipInfoElement != -1)
        {
            //transform.position = shipInfoManager.shipInfoList[myShipInfoElement].shipPosition;
            //shipHull.eulerAngles = shipInfoManager.shipInfoList[myShipInfoElement].hullRotation;
            //shipTurret.eulerAngles = shipInfoManager.shipInfoList[myShipInfoElement].turretRotation;

            Vector3 newPosition = shipInfoManager.shipInfoList[myShipInfoElement].shipPosition;
            Vector3 newRotation = shipInfoManager.shipInfoList[myShipInfoElement].turretRotation;

            if (previousShipPosition != newPosition)
            {
                movementDirection = newPosition - transform.position;
            }

            if (previousTurretRotation != newRotation)
            {
                Vector3 turretTargetPosition = Quaternion.Euler(newRotation) * Vector3.forward;
                SetLookTargetPosition(transform.position + new Vector3(turretTargetPosition.x, 0, turretTargetPosition.y));
            }

            previousShipPosition = newPosition;
            previousTurretRotation = newRotation;
        }

        base.FixedUpdate();
    }
    #endregion

}
