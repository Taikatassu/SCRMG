using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_MainMenuInfo : MonoBehaviour {

    #region References & Variables
    //References
    Core_Toolbox toolbox;
    Core_EventManager em;
    Core_GlobalVariableLibrary lib;
    //Variables
    int mySceneIndex = -1;
    #endregion

    #region Initialization
    void Start()
    {
        toolbox = FindObjectOfType<Core_Toolbox>();
        if (toolbox == null)
            Debug.LogError("toolbox not found!!!");
        em = toolbox.GetComponent<Core_EventManager>();
        lib = toolbox.GetComponent<Core_GlobalVariableLibrary>();
        GetStats();

        em.BroadcastNewSceneLoaded(mySceneIndex);
    }

    private void GetStats()
    {
        mySceneIndex = lib.sceneVariables.sceneIndexLevel01;
    }
    #endregion

}
