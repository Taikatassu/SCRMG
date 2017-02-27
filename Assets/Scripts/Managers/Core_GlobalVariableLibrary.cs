using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Implmen GlobalVariableLibrary usage to all scripts with eligible variables

public class Core_GlobalVariableLibrary : MonoBehaviour {

    public Ship_Variables shipVariables;
    public scene_Variables sceneVariables;

    public Core_GlobalVariableLibrary()
    {
        shipVariables = new Ship_Variables();
        sceneVariables = new scene_Variables();
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
        public List<Color> shipColorOptions = new List<Color>();
    }

    [System.Serializable]
    public class scene_Variables
    {
        public int mainMenuIndex = 0;
        public int level01Index = 1;
        public int numberOfShips = 4;
        public int matchBeginTimerLength = 3;
        public float waitTimeBeforeStartingMatchBeginTimer = 0.5f;
        public float fadeFromBlackTime = 2;
        public int matchBeginTimer = 3;
    }
}
