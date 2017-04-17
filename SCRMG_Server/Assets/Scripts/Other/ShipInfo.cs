using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipInfo
{
    public ShipInfo() { }

    public Vector3 shipPosition;
    public Vector3 hullRotation;
    public Vector3 turretRotation;
    public int shipIndex;
    public int shipColorIndex;
    public int spawnPointIndex;
    public float currentHealth;
    public string ownerID;
    public bool isControlledByServer = false;
}
