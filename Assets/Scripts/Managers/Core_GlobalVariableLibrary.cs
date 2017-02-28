using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Implmen GlobalVariableLibrary usage to all scripts with eligible variables

public class Core_GlobalVariableLibrary : MonoBehaviour {

    public static Core_GlobalVariableLibrary instance;

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

    public Input_Variables inputVariables;
    public Ship_Variables shipVariables;
    public Scene_Variables sceneVariables;

    public Core_GlobalVariableLibrary()
    {
        inputVariables = new Input_Variables();
        shipVariables = new Ship_Variables();
        sceneVariables = new Scene_Variables();
    }

    [System.Serializable]
    public class Input_Variables
    {
        public int keyboardAndMouseIndex = 1;
    }

    [System.Serializable]
    public class Ship_Variables
    {
        public string shipTag = "Ship";
        public string environmentTag = "Environment";
        public string mouseRayCollisionLayerName = "MouseRayCollider";
        public float movementSpeed = 10;
        public float maxHealth = 100;
        public float shipTurretRotationSpeed = 10;
        public float shipHullRotationSpeed = 10;
        public float bulletLaunchForce = 20;
        public float shootCooldownTime = 0.5f;
        public float shootDamage = 20;
        public List<Color> shipColorOptions = new List<Color>();
        public float healthBarMinValue = 0.01f;
        public float healthBarMaxValue = 1;
        public float healthBarLerpDuration = 0.1f;
    }

    [System.Serializable]
    public class Scene_Variables
    {
        public int sceneIndexMainMenu = 0;
        public int sceneIndexLevel01 = 1;
        public int numberOfShips = 4;
        public int matchStartTimerLength = 3;
        public float waitTimeBeforeStartingMatchBeginTimer = 0.5f;
        public float fadeFromBlackTime = 2;
    }
}
