using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_EventManager : MonoBehaviour
{
    #region Initialization
    public static Core_EventManager instance;
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
    public delegate void IntVector2Void(int integer, Vector2 vec2);
    public delegate void IntVoid(int integer);
    public delegate void GameObjectVoid(GameObject gameObject);
    #endregion

    #region Events
    #region Settings events
    public event IntVoid OnSetGameMode;
    public void BroadcastSetGameMode(int gameModeIndex)
    {
        if(OnSetGameMode != null)
        {
            OnSetGameMode(gameModeIndex);
        }
    }
    #endregion

    #region Scene events
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
        if(OnMouseButtonLeftDown != null)
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
        if(OnEscapeButtonDown != null)
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

    public event EmptyVoid OnHUDJoystickButtonReleased;
    public void BroadcastHUDJoystickButtonReleased()
    {
        if(OnHUDJoystickButtonReleased != null)
        {
            OnHUDJoystickButtonReleased();
        }
    }
    #endregion
    #endregion

    #region GameplayEvents
    public event IntVoid OnMatchStartTimerValue;
    public void BroadcastMatchStartTimerValue(int currentValue)
    {
        if (OnMatchStartTimerValue != null)
        {
            OnMatchStartTimerValue(currentValue);
        }
    }

    public event EmptyVoid OnGameRestart;
    public void BroadcastGameRestart()
    {
        Debug.Log("EventManager: BroadcastGameRestart");
        if (OnGameRestart != null)
        {
            OnGameRestart();
        }
    }

    public event IntVoid OnShipDead;
    public void BroadcastShipDead(int shipIndex)
    {
        if(OnShipDead != null)
        {
            OnShipDead(shipIndex);
        }
    }

    public event IntVoid OnGameEnd;
    public void BroadcastGameEnd(int winnerIndex)
    {
        if (OnGameEnd != null)
        {
            OnGameEnd(winnerIndex);
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
        if(OnShipReference != null)
        {
            OnShipReference(newShip);
        }
    }
    #endregion
    #endregion


}
