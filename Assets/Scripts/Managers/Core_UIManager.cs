using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Core_UIManager : MonoBehaviour {

    /* TODO: 
     * - Game over menu
     *      "Restart" "Return to main menu"
     *      Full gameloop with starting a match, restarting it, returning to main menu, and starting
     *      all over again
    *       
    *  - Enemy indicators showing enemy directions at the screen boarders
    *  
    *  - Find out why BroadcastRequestSceneSingleLevel01 is called twice 
    *       (and does it causes problems?)
    */

    #region References & variables
    public static Core_UIManager instance;
    //References
    Core_Toolbox toolbox;
    Core_GlobalVariableLibrary lib;
    Core_EventManager em;
    GameObject canvas;

    //MainMenu UI
    GameObject mainMenuHolder;
    Button playButton;

    //InGame UI
    GameObject inGameUIHolder;
    GameObject pauseMenuHolder;
    Image loadingScreenImage;
    Text matchStartTimerText;
    Button pauseMenuResumeButton;
    Button pauseMenuRestartButton;
    Button pauseMenuMainMenuButton;
    Color loadingScreenNewColor;
    Color loadingScreenOriginalColor;
    bool isFadingFromLoadingScreen = false;
    int matchStartTimerValue = -1;
    float loadingScreenFadeStartTime = -1;
    //Variables coming from globalVariableLibrary
    float loadingScreenFadeTime = -1;
    int sceneIndexMainMenu = -1;
    int sceneIndexLevel01 = -1;
    #endregion

    #region Initialization
    #region Awake
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

        #region Getting references
        toolbox = FindObjectOfType<Core_Toolbox>();
        em = toolbox.GetComponent<Core_EventManager>();
        lib = toolbox.GetComponent<Core_GlobalVariableLibrary>();
        GetStats();

        canvas = GameObject.FindWithTag("Canvas");
        mainMenuHolder = canvas.GetComponentInChildren<Core_MainMenuHolderTag>(true).gameObject;
        inGameUIHolder = canvas.GetComponentInChildren<Core_InGameUIHolderTag>(true).gameObject;
        pauseMenuHolder = inGameUIHolder.GetComponentInChildren<Core_PauseMenuHolderTag>(true).gameObject;

        playButton = mainMenuHolder.GetComponentInChildren<Core_MainMenuPlayButtonTag>(true).
            GetComponent<Button>();
        loadingScreenImage = inGameUIHolder.GetComponentInChildren<Core_FullscreenBlackImageTag>(true).
            GetComponent<Image>();
        matchStartTimerText = inGameUIHolder.GetComponentInChildren<Core_MatchBeginTimerTag>(true).
            GetComponent<Text>();
        pauseMenuResumeButton = inGameUIHolder.GetComponentInChildren<Core_PauseMenuResumeButtonTag>().
            GetComponent<Button>();
        pauseMenuRestartButton = inGameUIHolder.GetComponentInChildren<Core_PauseMenuRestartButtonTag>().
           GetComponent<Button>();
        pauseMenuMainMenuButton = inGameUIHolder.GetComponentInChildren<Core_PauseMenuMainMenuButtonTag>().
           GetComponent<Button>();
        #endregion

        #region Initialize UI
        OpenMainMenuUI();
        ClosePauseMenu();
        CloseInGameUI();
        #endregion
    }
    #endregion

    #region GetStats
    private void GetStats()
    {
        sceneIndexMainMenu = lib.sceneVariables.sceneIndexMainMenu;
        sceneIndexLevel01 = lib.sceneVariables.sceneIndexLevel01;
        loadingScreenFadeTime = lib.sceneVariables.fadeFromBlackTime;
    }
    #endregion

    #region OnEnable & OnDisable
    private void OnEnable()
    {
        em.OnMatchStartTimerValue += OnMatchStartTimerValue;
        em.OnGameRestart += OnGameRestart;
        em.OnNewSceneLoading += OnNewSceneLoading;
        em.OnNewSceneLoaded += OnNewSceneLoaded;
        em.OnEscapeButtonDown += OnEscapeButtonDown;
    }

    private void OnDisable()
    {
        em.OnMatchStartTimerValue -= OnMatchStartTimerValue;
        em.OnGameRestart -= OnGameRestart;
        em.OnNewSceneLoading -= OnNewSceneLoading;
        em.OnNewSceneLoaded -= OnNewSceneLoaded;
        em.OnEscapeButtonDown -= OnEscapeButtonDown;
    }
    #endregion

    #region Subscribers
    #region GameEvent subscribers
    private void OnMatchStartTimerValue(int currentTimerValue)
    {
        UpdateMatchStartTimer(currentTimerValue);
    }

    private void OnGameRestart()
    {
        //Reset neccessary values
        loadingScreenImage.gameObject.SetActive(true);
        StartFadeFromLoadingScreen();
    }

    private void OnNewSceneLoading(int sceneIndex)
    {
        //TODO: Add checks for all future scenes
        if (sceneIndex == sceneIndexMainMenu)
        {
            //Close InGameUI
            ClosePauseMenu();
            CloseInGameUI();
        }
        else if (sceneIndex == sceneIndexLevel01)
        {
            //Close main menu UI
            CloseMainMenuUI();
        }
    }

    private void OnNewSceneLoaded(int sceneIndex)
    {
        //TODO: Add checks for all future scenes
        if (sceneIndex == sceneIndexMainMenu)
        {
            Debug.Log("OnNewSceneLoaded: MainMenu");
            //Open MainMenuUI
            OpenMainMenuUI();
        }
        else if (sceneIndex == sceneIndexLevel01)
        {
            Debug.Log("OnNewSceneLoaded: Level01");
            //Open InGameUI
            OpenInGameUI();
            loadingScreenImage.gameObject.SetActive(true);
            StartFadeFromLoadingScreen();
            ClosePauseMenu();
        }
    }
    #endregion

    #region Input subscribers
    private void OnEscapeButtonDown(int controllerIndex)
    {
        if (inGameUIHolder.activeSelf)
        {
            if (pauseMenuHolder.activeSelf)
            {

                ClosePauseMenu();
            }
            else
            {
                OpenPauseMenu();
            }
        }
    }
    #endregion
    #endregion
    #endregion

    #region FixedUpdate
    private void FixedUpdate()
    {
        #region LoadingScreen fade
        if (isFadingFromLoadingScreen)
        {
            float timeSinceStarted = Time.time - loadingScreenFadeStartTime;
            float percentageComplete = timeSinceStarted / loadingScreenFadeTime;

            loadingScreenNewColor.a = Mathf.Lerp(1, 0, percentageComplete);
            loadingScreenImage.color = loadingScreenNewColor;

            if (percentageComplete >= 1.0f)
            {
                isFadingFromLoadingScreen = false;
                loadingScreenImage.gameObject.SetActive(false);
                loadingScreenImage.color = loadingScreenOriginalColor;
            }
        }
        #endregion
    }
    #endregion

    #region UI functions
    #region MainMenu UI
    private void OpenMainMenuUI()
    {
        playButton.onClick.AddListener(OnPlayButtonPressed);
        mainMenuHolder.SetActive(true);
    }

    private void CloseMainMenuUI()
    {
        playButton.onClick.RemoveAllListeners();
        mainMenuHolder.SetActive(false);
    }

    private void OnPlayButtonPressed()
    {
        em.BroadcastRequestSceneSingleLevel01();
    }
    #endregion

    #region InGame UI
    #region Toggle UI Elements
    private void OpenInGameUI()
    {
        inGameUIHolder.SetActive(true);
    }

    private void CloseInGameUI()
    {
        inGameUIHolder.SetActive(false);
    }

    private void OpenPauseMenu()
    {
        //TODO: Implement game pausing if in singleplayer
        Debug.Log("PauseMenuOpened");
        pauseMenuResumeButton.onClick.AddListener(PauseMenuResumeButtonPressed);
        pauseMenuRestartButton.onClick.AddListener(PauseMenuRestartButtonPressed);
        pauseMenuMainMenuButton.onClick.AddListener(PauseMenuMainMenuButtonPressed);
        pauseMenuHolder.SetActive(true);
    }

    private void ClosePauseMenu()
    {
        pauseMenuResumeButton.onClick.RemoveAllListeners();
        pauseMenuRestartButton.onClick.RemoveAllListeners();
        pauseMenuMainMenuButton.onClick.RemoveAllListeners();
        pauseMenuHolder.SetActive(false);
    }
    #endregion

    #region Match timer & Loading screen fade
    private void UpdateMatchStartTimer(int newTimerValue)
    {
        matchStartTimerValue = newTimerValue;
        matchStartTimerText.text = matchStartTimerValue.ToString();
        if (!matchStartTimerText.gameObject.activeSelf)
        {
            matchStartTimerText.gameObject.SetActive(true);
        }
        
        if (matchStartTimerValue <= 0)
        {
            matchStartTimerText.gameObject.SetActive(false);
        }
    }

    private void StartFadeFromLoadingScreen()
    {
        loadingScreenImage.gameObject.SetActive(true);
        loadingScreenOriginalColor = loadingScreenImage.color;
        loadingScreenOriginalColor.a = 1;
        loadingScreenNewColor = loadingScreenOriginalColor;
        isFadingFromLoadingScreen = true;
        loadingScreenFadeStartTime = Time.time;
    }
    #endregion

    #region PauseMenu buttons
    private void PauseMenuResumeButtonPressed()
    {
        Debug.Log("Resume button pressed");
        //Close pauseMenu
        ClosePauseMenu();
        //TODO: Resume game (if pause implemented and in effect)
    }

    private void PauseMenuRestartButtonPressed()
    {
        Debug.Log("Restart button pressed");
        //Close pauseMenu
        ClosePauseMenu();
        //Restart game
        em.BroadcastGameRestart();
    }

    private void PauseMenuMainMenuButtonPressed()
    {
        Debug.Log("ReturnToMainMenu button pressed");
        //Close pauseMenu (happens when loading main menu)
        //Close InGameUI (happens when loading main menu)
        //Load mainMenuScene
        em.BroadcastRequestSceneSingleMainMenu();
        //Open mainMenuUI (happens when loading main menu)
    }
    #endregion
    #endregion
    #endregion

}
