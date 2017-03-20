using System.Collections;
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
    public Projectile_Variables projectileVariables;
    public PowerUp_Variables powerUpVariables;
    public Scene_Variables sceneVariables;
    public UI_Variables uiVariables;
    public AI_Variables aiVariables;

    public Core_GlobalVariableLibrary()
    {
        inputVariables = new Input_Variables();
        gameSettingVariables = new GameSetting_Variables();
        shipVariables = new Ship_Variables();
        projectileVariables = new Projectile_Variables();
        powerUpVariables = new PowerUp_Variables();
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
        public float movementSpeed = 12;
        public float maxHealth = 100;
        public float shipTurretRotationSpeed = 10;
        public float shipHullRotationSpeed = 10;
        public float shootCooldownDuration = 0.25f;
        public float healthBarMinValue = 0.01f;
        public float healthBarMaxValue = 1;
        public float healthBarLerpDuration = 0.5f;
        public float cameraSpectatingHeight = 45;
        public float cameraFollowDistance = 0;
        public float cameraFollowHeight = 20;
    }

    [System.Serializable]
    public class Projectile_Variables
    {
        public float bulletDamage = 10;
        public float bulletSpeed = 30;
        public float bulletRange = 70;
        public float bulletTickRate = 1; //Default at 1
        public float bulletRicochetCooldown = 0;
        public int bulletRicochetNumber = 0;

        public float rubberBulletDamage = 5;
        public float rubberBulletSpeed = 90;
        public float rubberBulletRange = 200;
        public float rubberBulletTickRate = 1; //Default at 1
        public float rubberBulletRicochetCooldown = 0;
        public int rubberBulletRicochetNumber = 3;

        public float blazingRamDamage = 80;
        public float blazingRamSpeed = 0;
        public float blazingRamRange = 0;
        public float blazingRamTickRate = 25; //Default at 1
        public float blazingRamRicochetCooldown = 0;
        public int blazingRamRicochetNumber = 0;

        public float beamCannonDamage = 10;
        public float beamCannonSpeed = 0;
        public float beamCannonRange = 0;
        public float beamCannonTickRate = 25; //Default at 1
        public float beamCannonRicochetCooldown = 0;
        public int beamCannonRicochetNumber = 0;

        public float bombsDamage = 50;
        public float bombsSpeed = 15;
        public float bombsRange = 70;
        public float bombsTickRate = 1; //Default at 1
        public float bombsRicochetCooldown = 0;
        public int bombsRicochetNumber = 0;
    }

    [System.Serializable]
    public class PowerUp_Variables
    {
        public bool powerUpsDisabled = false;

        public float powerUpCooldown = 10;
        public float powerUpPlatformColliderTickRate = 15;

        public int rubberBulletsIndex = 1;
        public int blazingRamIndex = 2;
        public int beamCannonIndex = 3;
        public int bombsIndex = 4;

        public bool rubberBulletsAvailable = true;
        public bool blazingRamAvailable = true;
        public bool beamCannonAvailable = true;
        public bool bombsAvailable = false;

        #region RubberBullets variables
        public float rubberBulletsDuration = 8;
        public int rubberBulletsProjectileType = 1;
        public bool rubberBulletsIsPersistingProjectileState = false;
        public float rubberBulletsShipSpeedModifier = 1;
        public float rubberBulletsShipDamageTakenModifier = 1;
        public float rubberBulletsShootCooldownModifier = 1;
        public bool rubberBulletsCanShootState = true;
        public bool rubberBulletsIsMovableState = true;
        public bool rubberBulletsIsVulnerableState = true;
        #endregion

        #region BlazingRam variables
        public float blazingRamDuration = 9;
        public int blazingRamProjectileType = 2;
        public bool blazingRamIsPersistingProjectileState = true;
        public float blazingRamShipSpeedModifier = 2.5f;
        public float blazingRamShipDamageTakenModifier = 0.5f;
        public float blazingRamShootCooldownModifier = 1;
        public bool blazingRamCanShootState = true;
        public bool blazingRamIsMovableState = true;
        public bool blazingRamIsVulnerableState = true;
        #endregion

        #region BeamCannon variables
        public float beamCannonDuration = 8;
        public int beamCannonProjectileType = 3;
        public bool beamCannonIsPersistingProjectileState = true;
        public float beamCannonShipSpeedModifier = 1;
        public float beamCannonShipDamageTakenModifier = 1;
        public float beamCannonShootCooldownModifier = 1;
        public bool beamCannonCanShootState = true;
        public bool beamCannonIsMovableState = true;
        public bool beamCannonIsVulnerableState = true;
        #endregion

        #region Bombs variables
        public float bombsDuration = -1;
        public int bombsProjectileType = -1;
        public bool bombsIsPersistingProjectileState = false;
        public float bombsShipSpeedModifier = -1;
        public float bombsShipDamageTakenModifier = -1;
        public float bombsShootCooldownModifier = -1;
        public bool bombsCanShootState = false;
        public bool bombsIsMovableState = false;
        public bool bombsIsVulnerableState = false;
        #endregion
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
        public bool aiDisabled = false;
        //public float preferredMinDistanceToTarget = 5;
        public float closestTargetTimerDuration = 1;
        public float changeDirectionTimerDuration = 4;
        public float directionChangeLerpDuration = 1f;
        public float shootingRange = 18;
        public float preferredMaxDistanceToTarget = 35;
    }
}
