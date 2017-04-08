using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuInfo : MonoBehaviour {

    #region References & Variables
    //References
    Toolbox toolbox;
    EventManager em;
    GlobalVariableLibrary lib;
    //Variables
    int mySceneIndex = -1;
    #endregion

    #region Initialization
    void Start()
    {
        toolbox = FindObjectOfType<Toolbox>();
        if (toolbox == null)
            Debug.LogError("toolbox not found!!!");
        em = toolbox.GetComponent<EventManager>();
        lib = toolbox.GetComponent<GlobalVariableLibrary>();
        GetStats();

        em.BroadcastNewSceneLoaded(mySceneIndex);
    }

    private void GetStats()
    {
        mySceneIndex = lib.sceneVariables.sceneIndexMainMenu;
    }
    #endregion

}
