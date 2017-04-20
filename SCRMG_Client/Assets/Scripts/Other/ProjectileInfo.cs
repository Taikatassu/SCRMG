using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileInfo
{
    public ProjectileInfo() { }

    public string infoSenderID;
    public int projectileOwnerIndex;
    public int projectileIndex;
    public int hitShipIndex;
    public int projectileType;
    public float projectileDamage;
    public Vector3 spawnPosition;
    public Vector3 spawnRotation;
    public Vector3 hitLocation;
}
