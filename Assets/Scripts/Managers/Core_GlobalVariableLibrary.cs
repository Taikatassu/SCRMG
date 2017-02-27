using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_GlobalVariableLibrary : MonoBehaviour {

    public Ship_Variables shipVariables;

    public Core_GlobalVariableLibrary()
    {
        shipVariables = new Ship_Variables();
    }

    [System.Serializable]
    public class Ship_Variables
    {
        public float movementSpeed = 10;
        public float maxHealth = 100;
        public float shipTurretRotationSpeed = 10;
        public float shipHullRotationSpeed = 10;
        public float bulletLaunchForce = 20;
        public float shootCooldownTime = 0.5f;
        public float shootDamage = 20;
    }
}
