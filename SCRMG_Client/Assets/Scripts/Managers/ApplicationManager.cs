using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationManager : MonoBehaviour
{

    #region References & variables
    //References
    public static ApplicationManager instance;

    Toolbox toolbox;
    EventManager em;
    GlobalVariableLibrary lib;
    //Variables coming from GlobalVariableLibrary
    int currentSceneIndex = -1;
    int sceneIndexMainMenu = -1;
    int sceneIndexLevel01 = -1;
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

        toolbox = FindObjectOfType<Toolbox>();
        em = toolbox.GetComponent<EventManager>();
        lib = toolbox.GetComponent<GlobalVariableLibrary>();
        GetStats();
    }

    private void GetStats()
    {
        sceneIndexMainMenu = lib.sceneVariables.sceneIndexMainMenu;
        sceneIndexLevel01 = lib.sceneVariables.sceneIndexLevel01;
    }

    private void OnEnable()
    {
        em.OnRequestApplicationExit += OnRequestApplicationExit;
        em.OnRequestSceneSingleMainMenu += OnRequestSceneSingleMainMenu;
        em.OnRequestSceneSingleLevel01 += OnRequestSceneSingleLevel01;
        em.OnRequestCurrentSceneIndex += OnRequestCurrentSceneIndex;
    }

    private void OnDisable()
    {
        em.OnRequestApplicationExit -= OnRequestApplicationExit;
        em.OnRequestSceneSingleMainMenu -= OnRequestSceneSingleMainMenu;
        em.OnRequestSceneSingleLevel01 -= OnRequestSceneSingleLevel01;
        em.OnRequestCurrentSceneIndex -= OnRequestCurrentSceneIndex;
    }
    #endregion

    #region Subscribers
    #region OnRequestScene subscribers
    private void OnRequestApplicationExit()
    {
        Debug.Log("ApplicationManager: OnRequestApplicationExit");
        Application.Quit();
    }

    private void OnRequestSceneSingleMainMenu()
    {
        Debug.Log("SceneManager: Received MainMenu load request!");
        em.BroadcastNewSceneLoading(sceneIndexMainMenu);
        //Load scene "MainMenu" in single mode
        SceneManager.LoadScene(sceneIndexMainMenu, LoadSceneMode.Single);
        currentSceneIndex = sceneIndexMainMenu;
    }

    private void OnRequestSceneSingleLevel01()
    {
        Debug.Log("SceneManager: Received Level01 load request!");
        em.BroadcastNewSceneLoading(sceneIndexLevel01);
        //Load scene "Level01" in single mode
        SceneManager.LoadScene(sceneIndexLevel01, LoadSceneMode.Single);
        currentSceneIndex = sceneIndexLevel01;
    }
    #endregion

    #region RequestSceneInfo subscribers
    private int OnRequestCurrentSceneIndex()
    {
        return currentSceneIndex;
    }
    #endregion
    #endregion
}
