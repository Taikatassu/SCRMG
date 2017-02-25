using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_EventManager : MonoBehaviour
{
    #region Initialization
    public static Core_EventManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region Delegates
    public delegate void EmptyVoid();
    public delegate void IntVector2Void(int integer, Vector2 vec2);
    public delegate void IntVoid(int integer);
    #endregion

    #region Events
    #region Scene events
    public event EmptyVoid OnRequestSceneSingleLevel01;
    public void BroadcastRequestSceneSingleLevel01()
    {
        if (OnRequestSceneSingleLevel01 != null)
        {
            OnRequestSceneSingleLevel01();
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
    #endregion

    #region GameplayEvents
    public event IntVoid OnMatchBeginTimerValue;
    public void BroadcastMatchBeginTimerValue(int currentValue)
    {
        if (OnMatchBeginTimerValue != null)
        {
            OnMatchBeginTimerValue(currentValue);
        }
    }
    #endregion
    #endregion


}
