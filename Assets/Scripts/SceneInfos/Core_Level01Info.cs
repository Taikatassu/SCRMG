using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Core_Level01Info : MonoBehaviour {

    #region References & Variables
    //References
    Core_Toolbox toolbox;
    Core_GameManager gameManager;
    Core_GlobalVariableLibrary lib;
    List<Transform> respawnPoints = new List<Transform>();
    //UI Elements
    GameObject currentCanvas;
    Image fullscreenBlackImage;
    Text matchBeginTimerText;
    //Variables coming from globalVariableLibrary
    int numberOfShips = 0;
    float waitTimeBeforeStartingMatchBeginTimer = 0;
    #endregion

    #region OnEnable
    void OnEnable()
    {
        //Find references
        toolbox = FindObjectOfType<Core_Toolbox>();
        if (toolbox == null)
            Debug.LogError("toolbox not found!!!");
        gameManager = toolbox.GetComponent<Core_GameManager>();
        Transform respawnPointHolder = transform.
            GetComponentInChildren<Core_RespawnPointHolderTag>().transform;
        foreach (Transform child in respawnPointHolder)
        {
            respawnPoints.Add(child);
        }

        //Find globalVariableLibrary and get variables from it
        lib = toolbox.GetComponentInChildren<Core_GlobalVariableLibrary>();
        GetStats();

        //Send info to managers
        gameManager.SetRespawnPoints(respawnPoints);
        gameManager.SetShipCount(numberOfShips);      
        gameManager.InitializeGame();

        //Wait a moment before starting actual match begin timer
        StartCoroutine(WaitForSceneLoadedAndStartMatchTimer(waitTimeBeforeStartingMatchBeginTimer));
    }
    #endregion

    #region GetStats
    private void GetStats()
    {
        numberOfShips = lib.sceneVariables.numberOfShips;
        waitTimeBeforeStartingMatchBeginTimer = lib.sceneVariables.waitTimeBeforeStartingMatchBeginTimer;
    }
    #endregion

    #region WaitForSceneLoadedAndStartMatchTimer
    IEnumerator WaitForSceneLoadedAndStartMatchTimer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        gameManager.StartMatchBeginTimer();
    }
    #endregion
}
