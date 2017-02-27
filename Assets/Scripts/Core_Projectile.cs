using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_Projectile : MonoBehaviour {

    public enum EProjectileType { DEFAULT, BULLET }
    private EProjectileType projectileType = EProjectileType.DEFAULT;
    private float spawnTime = 0;
    private float lifeTime = 0;
    private Core_ShipController myShipController;

    private float bulletLifeTime = 2;

    void OnEnable()
    {
        spawnTime = Time.time;
    }

    void OnDisable()
    {
        spawnTime = 0;
        lifeTime = 0;
        projectileType = EProjectileType.DEFAULT;
    }

    public float GetSpawnTime()
    {
        return spawnTime;
    }

    public float GetLifeTime()
    {
        return lifeTime;
    }

    public void SetProjectileType(EProjectileType newProjectileType)
    {
        projectileType = newProjectileType;
        switch (projectileType)
        {
            case EProjectileType.DEFAULT:
                Debug.LogError("Projetile should never be type: DEFAULT after spawning!!");
                break;
            case EProjectileType.BULLET:
                lifeTime = bulletLifeTime;
                break;
        }
    }

    public void SetShipController(Core_ShipController newShipController)
    {
        myShipController = newShipController;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (myShipController != null)
            myShipController.OnProjectileTriggerEnter(this, collider.gameObject);
    }

}
