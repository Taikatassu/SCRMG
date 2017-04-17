using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    #region Initialization
    public static EventManager instance;
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
    #endregion

    #region Delegates
    public delegate void EmptyVoid();
    public delegate void FloatVoid(float floatingPoint);
    public delegate void IntVoid(int integer);
    public delegate void IntFloatVoid(int integer, float floatingpoint);
    public delegate void IntIntVoid(int integer1, int integer2);
    public delegate void IntIntIntVoid(int integer1, int integer2, int integer3);
    public delegate void IntIntVector3Vector3Void(int integer1, int integer2, Vector3 vec31, Vector3 vec32);
    public delegate void Vector2Void(Vector2 vec2);
    public delegate void IntVector2Void(int integer, Vector2 vec2);
    public delegate void IntVector3Void(int integer, Vector3 vec3);
    public delegate void GameObjectVoid(GameObject gameObject);
    public delegate void StringVoid(string string1);
    public delegate void BoolVoid(bool boolean);
    public delegate string EmptyString();
    public delegate bool EmptyBool();
    public delegate int EmptyInt();
    public delegate void IntIntIntStringVoid(int integer1, int integer2, int integer3, string string1);
    #endregion

    #region Events
    #region Settings events
    public event IntVoid OnSetGameMode;
    public void BroadcastSetGameMode(int gameModeIndex)
    {
        if (OnSetGameMode != null)
        {
            OnSetGameMode(gameModeIndex);
        }
    }
    #endregion

    #region Scene events
    public event EmptyVoid OnRequestApplicationExit;
    public void BroadcastRequestApplicationExit()
    {
        if (OnRequestApplicationExit != null)
        {
            OnRequestApplicationExit();
        }
    }

    public event EmptyVoid OnRequestSceneSingleMainMenu;
    public void BroadcastRequestSceneSingleMainMenu()
    {
        if (OnRequestSceneSingleMainMenu != null)
        {
            OnRequestSceneSingleMainMenu();
        }
    }

    public event EmptyVoid OnRequestSceneSingleLevel01;
    public void BroadcastRequestSceneSingleLevel01()
    {
        if (OnRequestSceneSingleLevel01 != null)
        {
            OnRequestSceneSingleLevel01();
        }
    }

    public event IntVoid OnNewSceneLoading; //Notice difference to the event BELOW!!
    public void BroadcastNewSceneLoading(int sceneIndex)
    {
        if (OnNewSceneLoading != null)
        {
            OnNewSceneLoading(sceneIndex);
        }
    }

    public event IntVoid OnNewSceneLoaded; //Notice difference to the event ABOVE!!
    public void BroadcastNewSceneLoaded(int sceneIndex)
    {
        if (OnNewSceneLoaded != null)
        {
            OnNewSceneLoaded(sceneIndex);
        }
    }
    #endregion

    #region Input events
    #region WASD
    //----------------- W Key --------------------------------------
    public event EmptyVoid OnWKeyDown;
    public void BroadcastWKeyDown()
    {
        if (OnWKeyDown != null)
        {
            Debug.Log("EventManager: W down");
            OnWKeyDown();
        }
    }

    public event EmptyVoid OnWKeyUp;
    public void BroadcastWKeyUp()
    {
        if (OnWKeyUp != null)
        {
            Debug.Log("EventManager: W up");
            OnWKeyUp();
        }
    }
    //----------------- A Key --------------------------------------
    public event EmptyVoid OnAKeyDown;
    public void BroadcastAKeyDown()
    {
        if (OnAKeyDown != null)
        {
            Debug.Log("EventManager: A down");
            OnAKeyDown();
        }
    }

    public event EmptyVoid OnAKeyUp;
    public void BroadcastAKeyUp()
    {
        if (OnAKeyUp != null)
        {
            Debug.Log("EventManager: A up");
            OnAKeyUp();
        }
    }
    //----------------- S Key --------------------------------------
    public event EmptyVoid OnSKeyDown;
    public void BroadcastSKeyDown()
    {
        if (OnSKeyDown != null)
        {
            Debug.Log("EventManager: S down");
            OnSKeyDown();
        }
    }

    public event EmptyVoid OnSKeyUp;
    public void BroadcastSKeyUp()
    {
        if (OnSKeyUp != null)
        {
            Debug.Log("EventManager: S up");
            OnSKeyUp();
        }
    }
    //----------------- D Key --------------------------------------
    public event EmptyVoid OnDKeyDown;
    public void BroadcastDKeyDown()
    {
        if (OnDKeyDown != null)
        {
            Debug.Log("EventManager: D down");
            OnDKeyDown();
        }
    }

    public event EmptyVoid OnDKeyUp;
    public void BroadcastDKeyUp()
    {
        if (OnDKeyUp != null)
        {
            Debug.Log("EventManager: D up");
            OnDKeyUp();
        }
    }
    #endregion

    #region WASD Experimental
    public event IntVector2Void OnMovementInput;
    public void BroadcastMovementInput(int controllerIndex, Vector2 movementInputVector)
    {
        if (OnMovementInput != null)
        {
            OnMovementInput(controllerIndex, movementInputVector);
        }
    }
    #endregion

    #region Mouse movement
    public event IntVector2Void OnMousePosition;
    public void BroadcastMousePosition(int controllerIndex, Vector2 mousePosition)
    {
        if (OnMousePosition != null)
        {
            OnMousePosition(controllerIndex, mousePosition);
        }
    }
    #endregion

    #region Mouse buttons
    public event IntVoid OnMouseButtonLeftDown;
    public void BroadcastMouseButtonLeftDown(int controllerIndex)
    {
        if (OnMouseButtonLeftDown != null)
        {
            OnMouseButtonLeftDown(controllerIndex);
        }
    }

    public event IntVoid OnMouseButtonLeftUp;
    public void BroadcastMouseButtonLeftUp(int controllerIndex)
    {
        if (OnMouseButtonLeftUp != null)
        {
            OnMouseButtonLeftUp(controllerIndex);
        }
    }

    public event IntVoid OnMouseButtonRightDown;
    public void BroadcastMouseButtonRightDown(int controllerIndex)
    {
        if (OnMouseButtonRightDown != null)
        {
            OnMouseButtonRightDown(controllerIndex);
        }
    }

    public event IntVoid OnMouseButtonRightUp;
    public void BroadcastMouseButtonRightUp(int controllerIndex)
    {
        if (OnMouseButtonRightUp != null)
        {
            OnMouseButtonRightUp(controllerIndex);
        }
    }
    #endregion

    #region Keys
    public event IntVoid OnEscapeButtonDown;
    public void BroadcastEscapeButtonDown(int controllerIndex)
    {
        if (OnEscapeButtonDown != null)
        {
            OnEscapeButtonDown(controllerIndex);
        }
    }

    public event IntVoid OnEscapeButtonUp;
    public void BroadcastEscapeButtonUp(int controllerIndex)
    {
        if (OnEscapeButtonUp != null)
        {
            OnEscapeButtonUp(controllerIndex);
        }
    }
    #endregion

    #region UI Buttons
    public event EmptyVoid OnShootButtonPressed;
    public void BroadcastShootButtonPressed()
    {
        if (OnShootButtonPressed != null)
        {
            OnShootButtonPressed();
        }
    }

    public event IntVoid OnVirtualJoystickPressed;
    public void BroadcastVirtualJoystickPressed(int joystickIndex)
    {
        if (OnVirtualJoystickPressed != null)
        {
            OnVirtualJoystickPressed(joystickIndex);
        }
    }

    public event IntVoid OnVirtualJoystickReleased;
    public void BroadcastVirtualJoystickReleased(int joystickIndex)
    {
        if (OnVirtualJoystickReleased != null)
        {
            OnVirtualJoystickReleased(joystickIndex);
        }
    }

    public event IntVector2Void OnVirtualJoystickValueChange;
    public void BroadcastVirtualJoystickValueChange(int joystickIndex, Vector2 newValue)
    {
        if (OnVirtualJoystickValueChange != null)
        {
            OnVirtualJoystickValueChange(joystickIndex, newValue);
        }
    }
    #endregion
    #endregion

    #region Gameplay events
    public event EmptyInt OnRequestCurrentGameModeIndex;
    public int BroadcastRequestCurrentGameModeIndex()
    {
        if (OnRequestCurrentGameModeIndex != null)
        {
            return OnRequestCurrentGameModeIndex();
        }

        return -1;
    }

    public event EmptyVoid OnRequestLoadingIconOn;
    public void BroadcastRequestLoadingIconOn()
    {
        if (OnRequestLoadingIconOn != null)
        {
            OnRequestLoadingIconOn();
        }
    }

    public event EmptyVoid OnRequestLoadingIconOff;
    public void BroadcastRequestLoadingIconOff()
    {
        if (OnRequestLoadingIconOff != null)
        {
            OnRequestLoadingIconOff();
        }
    }

    public event IntVoid OnMatchStartTimerValueChange;
    public void BroadcastMatchStartTimerValueChange(int currentValue)
    {
        if (OnMatchStartTimerValueChange != null)
        {
            OnMatchStartTimerValueChange(currentValue);
        }
    }

    public event EmptyVoid OnMatchStarted;
    public void BroadcastMatchStarted()
    {
        if (OnMatchStarted != null)
        {
            OnMatchStarted();
        }
    }

    public event IntVoid OnMatchEnded;
    public void BroadcastMatchEnded(int winnerIndex)
    {
        if (OnMatchEnded != null)
        {
            OnMatchEnded(winnerIndex);
        }
    }

    public event EmptyVoid OnGameRestart;
    public void BroadcastGameRestart()
    {
        if (OnGameRestart != null)
        {
            OnGameRestart();
        }
    }

    public event IntVoid OnShipDead;
    public void BroadcastShipDead(int shipIndex)
    {
        if (OnShipDead != null)
        {
            OnShipDead(shipIndex);
        }
    }

    public event EmptyVoid OnPauseOn;
    public void BroadcastPauseOn()
    {
        if (OnPauseOn != null)
        {
            OnPauseOn();
        }
    }

    public event EmptyVoid OnPauseOff;
    public void BroadcastPauseOff()
    {
        if (OnPauseOff != null)
        {
            OnPauseOff();
        }
    }

    public event GameObjectVoid OnShipReference;
    public void BroadcastShipReference(GameObject newShip)
    {
        if (OnShipReference != null)
        {
            OnShipReference(newShip);
        }
    }

    public event FloatVoid OnMatchTimerValueChange;
    public void BroadcastMatchTimerValueChange(float newValue)
    {
        if (OnMatchTimerValueChange != null)
        {
            OnMatchTimerValueChange(newValue);
        }
    }

    public event IntIntIntVoid OnPowerUpPickedUp;
    public void BroadcastPowerUpPickedUp(int shipIndex, int powerUpBaseIndex, int powerUpType)
    {
        if (OnPowerUpPickedUp != null)
        {
            OnPowerUpPickedUp(shipIndex, powerUpBaseIndex, powerUpType);
        }
    }

    public event IntIntVoid OnPowerUpOnline;
    public void BroadcastPowerUpOnline(int powerUpBaseIndex, int powerUpType)
    {
        if (OnPowerUpOnline != null)
        {
            OnPowerUpOnline(powerUpBaseIndex, powerUpType);
        }
    }

    public event IntIntVoid OnPowerUpEnded;
    public void BroadcastPowerUpEnded(int shipIndex, int powerUpType)
    {
        if (OnPowerUpEnded != null)
        {
            OnPowerUpEnded(shipIndex, powerUpType);
        }
    }

    public event StringVoid OnRequestUINotification;
    public void BroadcastRequestUINotification(string notificationContent)
    {
        Debug.Log("Broadcasting UINotification request");
        if (OnRequestUINotification != null)
        {
            OnRequestUINotification(notificationContent);
        }
    }

    public event EmptyInt OnRequestCurrentSceneIndex;
    public int BroadcastRequestCurrentSceneIndex()
    {
        if (OnRequestCurrentSceneIndex != null)
        {
            return OnRequestCurrentSceneIndex();
        }

        return -1;
    }
    #endregion

    #region ShipInfo
    public event IntFloatVoid OnShipHealthChange;
    public void BroadcastShipHealthChange(int shipIndex, float currentHealth)
    {
        if (OnShipHealthChange != null)
        {
            OnShipHealthChange(shipIndex, currentHealth);
        }
    }

    public event IntIntVector3Vector3Void OnProjectileSpawned;
    public void BroadcastProjectileSpawned(int projectileOwnerIndex, int projectileIndex, Vector3 spawnPosition, Vector3 spawnRotation)
    {
        if (OnProjectileSpawned != null)
        {
            OnProjectileSpawned(projectileOwnerIndex, projectileIndex, spawnPosition, spawnRotation);
        }
    }

    public event IntIntVoid OnProjectileDestroyed;
    public void BroadcastProjectileDestroyed(int projectileOwnerIndex, int projectileIndex)
    {
        if (OnProjectileDestroyed != null)
        {
            OnProjectileDestroyed(projectileOwnerIndex, projectileIndex);
        }
    }

    public event IntIntVector3Vector3Void OnProjectileSpawnedByServer;
    public void BroadcastProjectileSpawnedByServer(int projectileOwnerIndex, int projectileIndex, Vector3 spawnPosition, Vector3 spawnRotation)
    {
        if (OnProjectileSpawnedByServer != null)
        {
            OnProjectileSpawnedByServer(projectileOwnerIndex, projectileIndex, spawnPosition, spawnRotation);
        }
    }

    public event IntIntVoid OnProjectileDestroyedByServer;
    public void BroadcastProjectileDestroyedByServer(int projectileOwnerIndex, int projectileIndex)
    {
        if (OnProjectileDestroyedByServer != null)
        {
            OnProjectileDestroyedByServer(projectileOwnerIndex, projectileIndex);
        }
    }

    public event IntIntIntStringVoid OnShipSpawnByServer;
    public void BroadcastShipSpawnByServer(int shipIndex, int spawnPointIndex, int shipColorIndex, string ownerID)
    {
        if (OnShipSpawnByServer != null)
        {

            OnShipSpawnByServer(shipIndex, spawnPointIndex, shipColorIndex, ownerID);
        }
    }
    #endregion

    #region Network events
    public event EmptyVoid OnNetworkMultiplayerStartMatchStartTimer;
    public void BroadcastNetworkMultiplayerStartMatchStartTimer()
    {
        if (OnNetworkMultiplayerStartMatchStartTimer != null)
        {
            OnNetworkMultiplayerStartMatchStartTimer();
        }
    }

    public event EmptyVoid OnNetworkMultiplayerMatchInitialized;
    public void BroadcastNetworkMultiplayerMatchInitialized()
    {
        if (OnNetworkMultiplayerMatchInitialized != null)
        {
            OnNetworkMultiplayerMatchInitialized();
        }
    }

    public event IntVoid OnStartingMatchByServer;
    public void BroadcastStartingMatchByServer(int numberOfShips)
    {
        if (OnStartingMatchByServer != null)
        {
            OnStartingMatchByServer(numberOfShips);
        }
    }

    public event EmptyString OnRequestMyNetworkID;
    public string BroadcastRequestMyNetworkID()
    {
        if (OnRequestMyNetworkID != null)
        {
            return OnRequestMyNetworkID();
        }
        return "NA";
    }

    public event BoolVoid OnLobbyReadyStateChange;
    public void BroadcastLobbyReadyStateChange(bool state)
    {
        if (OnLobbyReadyStateChange != null)
        {
            OnLobbyReadyStateChange(state);
        }
    }

    public event IntVoid OnReadyCountInLobbyChange;
    public void BroadcastReadyCountInLobbyChange(int newCount)
    {
        if (OnReadyCountInLobbyChange != null)
        {
            OnReadyCountInLobbyChange(newCount);
        }
    }

    public event EmptyVoid OnRequestOnlineMatchStart;
    public void BroadcastRequestOnlineMatchStart()
    {
        if (OnRequestOnlineMatchStart != null)
        {
            OnRequestOnlineMatchStart();
        }
    }

    public event StringVoid OnRequestConnectToNetwork;
    public void BroadcastRequestConnectToNetwork(string ip)
    {
        if (OnRequestConnectToNetwork != null)
        {
            OnRequestConnectToNetwork(ip);
        }
    }

    public event StringVoid OnConnectingToNetworkSucceeded;
    public void BroadcastConnectingToNetworkSucceeded(string ip)
    {
        if (OnConnectingToNetworkSucceeded != null)
        {
            OnConnectingToNetworkSucceeded(ip);
        }
    }

    public event StringVoid OnConnectingToNetworkFailed;
    public void BroadcastConnectingToNetworkFailed(string ip)
    {
        if (OnConnectingToNetworkFailed != null)
        {
            OnConnectingToNetworkFailed(ip);
        }
    }

    public event EmptyVoid OnConnectionToNetworkLost;
    public void BroadcastConnectionToNetworkLost()
    {
        if (OnConnectionToNetworkLost != null)
        {
            OnConnectionToNetworkLost();
        }
    }

    public event EmptyVoid OnRequestDisconnectFromNetwork;
    public void BroadcastRequestDisconnectFromNetwork()
    {
        if (OnRequestDisconnectFromNetwork != null)
        {
            OnRequestDisconnectFromNetwork();
        }
    }

    public event EmptyString OnRequestServerIPAddress;
    public string BroadcastRequestServerIPAddress()
    {
        if (OnRequestServerIPAddress != null)
        {
            return OnRequestServerIPAddress();
        }

        return "172.31.16.131";
    }

    public event EmptyBool OnRequestNetworkConnectionStatus;
    public bool BroadcastRequestNetworkConnectionStatus()
    {
        if (OnRequestNetworkConnectionStatus != null)
        {
            return OnRequestNetworkConnectionStatus();
        }

        return false;
    }

    public event IntVoid OnClientCountInLobbyChange;
    public void BroadcastClientCountInLobbyChange(int newCount)
    {
        if (OnClientCountInLobbyChange != null)
        {
            OnClientCountInLobbyChange(newCount);
        }
    }

    public event EmptyVoid OnRequestLobbyEnter;
    public void BroadcastRequestLobbyEnter()
    {
        if (OnRequestLobbyEnter != null)
        {
            OnRequestLobbyEnter();
        }
    }

    public event EmptyVoid OnLobbyEnterSuccessful;
    public void BroadcastLobbyEnterSuccessful()
    {
        if (OnLobbyEnterSuccessful != null)
        {
            OnLobbyEnterSuccessful();
        }
    }

    public event EmptyVoid OnLobbyEnterDenied;
    public void BroadcastLobbyEnterDenied()
    {
        if (OnLobbyEnterDenied != null)
        {
            OnLobbyEnterDenied();
        }
    }

    public event EmptyVoid OnRequestLobbyExit;
    public void BroadcastRequestLobbyExit()
    {
        if (OnRequestLobbyExit != null)
        {
            OnRequestLobbyExit();
        }
    }
    #endregion
    #endregion

}
