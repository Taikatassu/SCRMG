using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_InputManager : MonoBehaviour {

    #region References & variables
    //References
    public static Core_InputManager instance;

    Core_Toolbox toolbox;
    Core_EventManager em;
    Core_GlobalVariableLibrary lib;
    //Variables coming from within the script
    Vector2 movementInputVector;
    Vector2 mousePosition;
    bool movementInputType = true;
    bool movementZeroSent = false;
    //Variables coming from GlobalVariableLibrary
    int keyboardAndMouseIndex = -1;
    #endregion

    #region Initialization
    void Awake ()
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

        toolbox = FindObjectOfType<Core_Toolbox>();
        em = toolbox.GetComponent<Core_EventManager>();
        lib = toolbox.GetComponent<Core_GlobalVariableLibrary>();
        GetStats();
	}

    private void GetStats()
    {
        keyboardAndMouseIndex = lib.inputVariables.keyboardAndMouseIndex;
    }
    #endregion

    #region Update
    void Update () {

        #region KeyCode inputs
        if (!movementInputType)
        {
            #region WASD
            if (Input.GetKeyDown(KeyCode.W))
            {
                em.BroadcastWKeyDown();
            }
            if (Input.GetKeyUp(KeyCode.W))
            {
                em.BroadcastWKeyUp();
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                em.BroadcastAKeyDown();
            }
            if (Input.GetKeyUp(KeyCode.A))
            {
                em.BroadcastAKeyUp();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                em.BroadcastSKeyDown();
            }
            if (Input.GetKeyUp(KeyCode.S))
            {
                em.BroadcastSKeyUp();
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                em.BroadcastDKeyDown();
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                em.BroadcastDKeyUp();
            }
            #endregion
        }
        else
        {
            #region WASD Experimental
            movementInputVector = Vector2.zero;

            //right, up == 1, 1
            if (Input.GetKey(KeyCode.W))
            {
                movementInputVector.y += 1;
            }

            if (Input.GetKey(KeyCode.A))
            {
                movementInputVector.x -= 1;
            }

            if (Input.GetKey(KeyCode.S))
            {
                movementInputVector.y -= 1;
            }

            if (Input.GetKey(KeyCode.D))
            {
                movementInputVector.x += 1;
            }

            //If movement vector's magnitude is not already 1
            if (movementInputVector.x != 0 || movementInputVector.y != 0)
            {
                movementInputVector.Normalize();
            }

            if (movementInputVector != Vector2.zero)
            {
                em.BroadcastMovementInput(keyboardAndMouseIndex, movementInputVector);
                movementZeroSent = false;
            }
            else if (!movementZeroSent)
            {
                em.BroadcastMovementInput(keyboardAndMouseIndex, movementInputVector);
                movementZeroSent = true;
            }
            #endregion
        }

        #region Keys
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            em.BroadcastEscapeButtonDown(keyboardAndMouseIndex);
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            em.BroadcastEscapeButtonUp(keyboardAndMouseIndex);
        }
        #endregion
        #endregion

        #region Mouse movement
        mousePosition = Input.mousePosition;
        em.BroadcastMousePosition(keyboardAndMouseIndex, mousePosition);
        #endregion

        #region Mouse buttons
        if (Input.GetMouseButtonDown(0))
        {
            em.BroadcastMouseButtonLeftDown(keyboardAndMouseIndex);
        }

        if (Input.GetMouseButtonUp(0))
        {
            em.BroadcastMouseButtonLeftUp(keyboardAndMouseIndex);
        }

        if (Input.GetMouseButtonDown(1))
        {
            em.BroadcastMouseButtonRightDown(keyboardAndMouseIndex);
        }

        if (Input.GetMouseButtonUp(1))
        {
            em.BroadcastMouseButtonRightDown(keyboardAndMouseIndex);
        }
        #endregion
    }
    #endregion

}
