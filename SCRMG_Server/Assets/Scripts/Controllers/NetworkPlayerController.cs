using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayerController : ShipController
{  
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
        em.OnProjectileSpawnedByClient += OnProjectileSpawnedByClient;

        isControllerByServer = false;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        em.OnProjectileSpawnedByClient -= OnProjectileSpawnedByClient;
    }
    #endregion

    #region Subscribers
    private void OnProjectileSpawnedByClient(int projectileOwnerIndex, int projectileIndex, Vector3 spawnPosition, Vector3 spawnRotation)
    {
        if (projectileOwnerIndex == index)
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

    #region FixedUpdate
    protected override void FixedUpdate()
    {
        if (myShipInfoElement == -1)
        {
            myShipInfoElement = shipInfoManager.GetMyShipInfoElement(index);
        }

        if (myShipInfoElement != -1)
        {
            transform.position = shipInfoManager.shipInfoList[myShipInfoElement].shipPosition;
            shipHull.eulerAngles = shipInfoManager.shipInfoList[myShipInfoElement].hullRotation;
            shipTurret.eulerAngles = shipInfoManager.shipInfoList[myShipInfoElement].turretRotation;
        }
    }
    #endregion
}
