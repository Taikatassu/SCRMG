using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVariableLibrary : MonoBehaviour {

    public static GlobalVariableLibrary instance;

    private void Awake()
    {
        #region Singletonization
        if (instance == null)
        {
            instance = this;
        }

        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        #endregion
    }

    public ServerVariables serverVariables;

    public GlobalVariableLibrary()
    {
        serverVariables = new ServerVariables();
    }

    [System.Serializable]
    public class ServerVariables
    {
        public int matchStartTimerLength = 3;
        public int maxNumberOfClientsInLobby = 4;
        public List<Color> shipColorOptions = new List<Color>();

        public string shipTag = "Ship";
        public string environmentTag = "Environment";

        public bool aiDisabled = false;
        public float closestTargetTimerDuration = 1;
        public float changeDirectionTimerDuration = 4;
        public float directionChangeLerpDuration = 1f;
        public float shootingRange = 18;
        public float preferredMaxDistanceToTarget = 35;

        public float movementSpeed = 12;
        public float maxHealth = 100;
        public float shipTurretRotationSpeed = 10;
        public float shipHullRotationSpeed = 10;
        public float shootCooldownDuration = 0.25f;
        public float healthBarMinValue = 0.01f;
        public float healthBarMaxValue = 1;
        public float healthBarLerpDuration = 0.5f;

        public float bulletDamage = 10;
        public float bulletSpeed = 30;
        public float bulletRange = 70;
        public float bulletTickRate = 1; //Default at 1
        public float bulletRicochetCooldown = 0;
        public int bulletRicochetNumber = 0;
    }
}
