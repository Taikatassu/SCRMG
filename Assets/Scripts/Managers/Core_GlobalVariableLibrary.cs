﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* TODO: 
 * - Check that all scripts use GlobalVariableLibrary for
 *   their suitable variables
 *      
 * - Reorganize GlobalVariableLibrary !
 *      Separate categories for variables meant to be set by the 
 *      developer and settings that can be changed through in
 *      game menus
*/

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
    public GameSetting_Variables gameSettingVariables;
    public Ship_Variables shipVariables;
    public Scene_Variables sceneVariables;
    public UI_Variables uiVariables;
    public AI_Variables aiVariables;

    public Core_GlobalVariableLibrary()
    {
        inputVariables = new Input_Variables();
        gameSettingVariables = new GameSetting_Variables();
        shipVariables = new Ship_Variables();
        sceneVariables = new Scene_Variables();
        uiVariables = new UI_Variables();
        aiVariables = new AI_Variables();
    }

    [System.Serializable]
    public class Input_Variables
    {
        public int keyboardAndMouseIndex = 1;
    }

    [System.Serializable]
    public class GameSetting_Variables
    {
        public int gameModeSingleplayerIndex = 0;
        public int gameModeNetworkMultiplayerIndex = 1;
        public int gameModeLocalMultiplayerIndex = 2;
        public int buildPlatform = 0; //0 = PC, 1 = Android
    }

    [System.Serializable]
    public class Ship_Variables
    {
        // TODO: Add ship camera variables to GlobalVariableLibrary
        public List<Color> shipColorOptions = new List<Color>();
        public string shipTag = "Ship";
        public string environmentTag = "Environment";
        public string mouseRayCollisionLayerName = "MouseRayCollider";
        public float movementSpeed = 10;
        public float maxHealth = 100;
        public float shipTurretRotationSpeed = 10;
        public float shipHullRotationSpeed = 10;
        public float bulletLaunchForce = 20;
        public float shootCooldownTime = 0.25f;
        public float shootDamage = 10;
        public float healthBarMinValue = 0.01f;
        public float healthBarMaxValue = 1;
        public float healthBarLerpDuration = 0.1f;
        public float bulletSpeed = 5.0f;
        public float bulletRange = 25.0f;
        public float cameraSpectatingHeight = 45;
        public float cameraFollowDistance = 0;
        public float cameraFollowHeight = 25.0f;
    }

    [System.Serializable]
    public class Scene_Variables
    {
        public float waitTimeBeforeStartingMatchBeginTimer = 0.5f;
        public int sceneIndexMainMenu = 0;
        public int sceneIndexLevel01 = 1;
        public int numberOfShips = 4;
        public int matchStartTimerLength = 3;
    }

    [System.Serializable]
    public class UI_Variables
    {
        public string winText = "Victory!";
        public string lossText = "Defeat";
        public float fadeFromBlackTime = 2;
        public float offscreenIndicatorSidebufferPC = 0;
        public float offscreenIndicatorSidebufferAndroid = 0.22f;
        public bool invertedHUD = false;
    }

    [System.Serializable]
    public class AI_Variables
    {
        //public float preferredMinDistanceToTarget = 5;
        public float closestTargetTimerDuration = 1;
        public float changeDirectionTimerDuration = 4;
        public float directionChangeLerpDuration = 1f;
        public float shootingRange = 15;
    }
}
