using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Core_Level01Info : MonoBehaviour {

    #region References & Variables
    //References
    Core_Toolbox toolbox;
    Core_EventManager em;
    Core_GameManager gameManager;
    Core_GlobalVariableLibrary lib;
    List<Transform> respawnPoints = new List<Transform>();
    //Variables coming from globalVariableLibrary
    int mySceneIndex = 0;
    #endregion

    #region Start
    void Start()
    {
        //Find references
        toolbox = FindObjectOfType<Core_Toolbox>();
        if (toolbox == null)
            Debug.LogError("toolbox not found!!!");
        em = toolbox.GetComponent<Core_EventManager>();
        gameManager = toolbox.GetComponent<Core_GameManager>();
        Transform respawnPointHolder = transform.
            GetComponentInChildren<Core_RespawnPointHolderTag>().transform;
        foreach (Transform child in respawnPointHolder)
        {
            respawnPoints.Add(child);
        }

        //Find globalVariableLibrary and get variables from it
        lib = toolbox.GetComponent<Core_GlobalVariableLibrary>();
        GetStats();

        //Send respawnPoint list to GameManager and broadcast NewSceneLoaded
        gameManager.SetRespawnPoints(respawnPoints);
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
