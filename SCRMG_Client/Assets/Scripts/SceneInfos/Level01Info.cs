using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Level01Info : MonoBehaviour {

    #region References & Variables
    //References
    Toolbox toolbox;
    EventManager em;
    GameManager gameManager;
    GlobalVariableLibrary lib;
    List<Transform> respawnPoints = new List<Transform>();
    List<Transform> powerUpPositions = new List<Transform>();
    //Variables coming from globalVariableLibrary
    int mySceneIndex = 0;
    #endregion

    #region Start
    void Start()
    {
        //Find references
        toolbox = FindObjectOfType<Toolbox>();
        if (toolbox == null)
            Debug.LogError("toolbox not found!!!");
        em = toolbox.GetComponent<EventManager>();
        gameManager = toolbox.GetComponent<GameManager>();
        Transform respawnPointHolder = transform.
            GetComponentInChildren<RespawnPointHolderTag>().transform;
        foreach (Transform child in respawnPointHolder)
        {
            respawnPoints.Add(child);
        }

        Transform powerUpPositionHolder = transform.
            GetComponentInChildren<PowerUpPositionsHolderTag>().transform;
        foreach(Transform child in powerUpPositionHolder)
        {
            powerUpPositions.Add(child);
        }

        //Find globalVariableLibrary and get variables from it
        lib = toolbox.GetComponent<GlobalVariableLibrary>();
        GetStats();

        //Send respawnPoint list to GameManager and broadcast NewSceneLoaded
        gameManager.SetRespawnPoints(respawnPoints);
        gameManager.SetPowerUpPositions(powerUpPositions);
        em.BroadcastNewSceneLoaded(mySceneIndex);
    }
    #endregion

    #region GetStats
    private void GetStats()
    {
        mySceneIndex = lib.sceneVariables.sceneIndexLevel01;
    }
    #endregion
}
