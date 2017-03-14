using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Core_ApplicationManager : MonoBehaviour
{

    #region References & variables
    //References
    public static Core_ApplicationManager instance;

    Core_Toolbox toolbox;
    Core_EventManager em;
    Core_GlobalVariableLibrary lib;
    //Variables coming from GlobalVariableLibrary
    int sceneIndexMainMenu = 0;
    int sceneIndexLevel01 = 0;
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
        sceneIndexMainMenu = lib.sceneVariables.sceneIndexMainMenu;
        sceneIndexLevel01 = lib.sceneVariables.sceneIndexLevel01;
    }

    private void OnEnable()
    {
        em.OnRequestSceneSingleMainMenu += OnRequestSceneSingleMainMenu;
        em.OnRequestSceneSingleLevel01 += OnRequestSceneSingleLevel01;
    }

    private void OnDisable()
    {
        em.OnRequestSceneSingleMainMenu -= OnRequestSceneSingleMainMenu;
        em.OnRequestSceneSingleLevel01 -= OnRequestSceneSingleLevel01;
    }
    #endregion

    #region Subscribers
    #region OnRequestScene subscribers
    private void OnRequestSceneSingleMainMenu()
    {
        Debug.Log("SceneManager: Received MainMenu load request!");
        em.BroadcastNewSceneLoading(sceneIndexMainMenu);
        //Load scene "MainMenu" in single mode
        SceneManager.LoadScene(sceneIndexMainMenu, LoadSceneMode.Single);
    }

    private void OnRequestSceneSingleLevel01()
    {
        Debug.Log("SceneManager: Received Level01 load request!");
        em.BroadcastNewSceneLoading(sceneIndexLevel01);
        //Load scene "Level01" in single mode
        SceneManager.LoadScene(sceneIndexLevel01, LoadSceneMode.Single);
    }
    #endregion
    #endregion
}
