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
    *  - Find a way to have the game stay paused (ship immoveable and invulnerable) in a case where
    *       pause menu is opened during matchStartTimer and left open
    *       --> When timer is zero, game is effectively unpaused
    *       - A separate isPaused bool that's state is checked when ever unpausing the game?
    *       - A check in Update loop to constantly check pause state?
    */

    #region References & variables
    public static Core_UIManager instance;
    
    Core_Toolbox toolbox;
    Core_GlobalVariableLibrary lib;
    Core_EventManager em;
    GameObject canvas;
    string canvasTag;

    //MainMenu UI
    GameObject mainMenuHolder;
    Button playButton;

    //InGame UI
    GameObject inGameUIHolder;
    GameObject pauseMenuHolder;
    GameObject gameEndMenuHolder;
    Image loadingScreenImage;
    Text matchStartTimerText;
    Text gameEndMenuText;
    Button pauseMenuResumeButton;
    Button pauseMenuRestartButton;
    Button pauseMenuMainMenuButton;
    Button gameEndMenuRestartButton;
    Button gameEndMenuMainMenuButton;
    Color loadingScreenNewColor;
    Color loadingScreenOriginalColor;
    bool isFadingFromLoadingScreen = false;
    int matchStartTimerValue = -1;
    float loadingScreenFadeStartTime = -1;
    int currentGameModeIndex = -1;
    //Variables coming from globalVariableLibrary
    int gameModeSingleplayerIndex = -1;
    int gameModeNetworkMultiplayerIndex = -1;
    int gameModeLocalMultiplayerIndex = -1;
    float loadingScreenFadeTime = -1;
    int sceneIndexMainMenu = -1;
    int sceneIndexLevel01 = -1;
    string winText;
    string lossText;
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

        canvas = GameObject.FindWithTag(canvasTag);
        mainMenuHolder = canvas.GetComponentInChildren<Core_MainMenuHolderTag>(true).gameObject;
        inGameUIHolder = canvas.GetComponentInChildren<Core_InGameUIHolderTag>(true).gameObject;
        pauseMenuHolder = inGameUIHolder.GetComponentInChildren<Core_PauseMenuHolderTag>(true).gameObject;
        gameEndMenuHolder = inGameUIHolder.GetComponentInChildren<Core_GameEndMenuHolderTag>(true).gameObject;

        playButton = mainMenuHolder.GetComponentInChildren<Core_MainMenuPlayButtonTag>(true).
            GetComponent<Button>();

        loadingScreenImage = inGameUIHolder.GetComponentInChildren<Core_LoadingScreenImageTag>(true).
            GetComponent<Image>();
        matchStartTimerText = inGameUIHolder.GetComponentInChildren<Core_MatchBeginTimerTag>(true).
            GetComponent<Text>();

        pauseMenuResumeButton = pauseMenuHolder.GetComponentInChildren<Core_PauseMenuResumeButtonTag>().
            GetComponent<Button>();
        pauseMenuRestartButton = pauseMenuHolder.GetComponentInChildren<Core_PauseMenuRestartButtonTag>().
            GetComponent<Button>();
        pauseMenuMainMenuButton = pauseMenuHolder.GetComponentInChildren<Core_PauseMenuMainMenuButtonTag>().
            GetComponent<Button>();

        gameEndMenuText = gameEndMenuHolder.GetComponentInChildren<Core_GameEndMenuTextTag>().GetComponent<Text>();
        gameEndMenuRestartButton = gameEndMenuHolder.GetComponentInChildren<Core_GameEndMenuRestartButtonTag>().
            GetComponent<Button>();
        gameEndMenuMainMenuButton = gameEndMenuHolder.GetComponentInChildren<Core_GameEndMenuMainMenuButtonTag>().
            GetComponent<Button>();
        #endregion

        #region Initialize UI
        ClosePauseMenu();
        CloseInGameUI();
        #endregion
    }
    #endregion

    #region GetStats
    private void GetStats()
    {
        gameModeSingleplayerIndex = lib.gameSettingVariables.gameModeSingleplayerIndex;
        gameModeNetworkMultiplayerIndex = lib.gameSettingVariables.gameModeNetworkMultiplayerIndex;
        gameModeLocalMultiplayerIndex = lib.gameSettingVariables.gameModeLocalMultiplayerIndex;
        sceneIndexMainMenu = lib.sceneVariables.sceneIndexMainMenu;
        sceneIndexLevel01 = lib.sceneVariables.sceneIndexLevel01;
        loadingScreenFadeTime = lib.uiVariables.fadeFromBlackTime;
        canvasTag = lib.uiVariables.canvasTag;
        winText = lib.uiVariables.winText;
        lossText = lib.uiVariables.lossText;
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
        em.OnSetGameMode += OnSetGameMode;
        em.OnGameEnd += OnGameEnd;
    }

    private void OnDisable()
    {
        em.OnMatchStartTimerValue -= OnMatchStartTimerValue;
        em.OnGameRestart -= OnGameRestart;
        em.OnNewSceneLoading -= OnNewSceneLoading;
        em.OnNewSceneLoaded -= OnNewSceneLoaded;
        em.OnEscapeButtonDown -= OnEscapeButtonDown;
        em.OnSetGameMode -= OnSetGameMode;
        em.OnGameEnd -= OnGameEnd;
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
            CloseGameEndMenu();
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
            //Open MainMenuUI
            OpenMainMenuUI();
        }
        else if (sceneIndex == sceneIndexLevel01)
        {
            //Open InGameUI
            OpenInGameUI();

            loadingScreenImage.gameObject.SetActive(true);
            StartFadeFromLoadingScreen();
            ClosePauseMenu();
            CloseGameEndMenu();
        }
    }

    private void OnSetGameMode(int newGameModeIndex)
    {
        currentGameModeIndex = newGameModeIndex;
    }

    private void OnGameEnd(int newWinnerIndex)
    {
        if (currentGameModeIndex == gameModeSingleplayerIndex)
        {
            if (newWinnerIndex == 1)
            {
                gameEndMenuText.text = winText;
            }
            else
            {
                gameEndMenuText.text = lossText;
            }
            OpenGameEndMenu();
        }
        else if (currentGameModeIndex == gameModeNetworkMultiplayerIndex)
        {
            // TODO: Check if last ship alive is localPlayer and change text accordingly
            gameEndMenuText.text = "NetMp GameEnd";
            OpenGameEndMenu();
        }
        else if (currentGameModeIndex == gameModeLocalMultiplayerIndex)
        {
            // TODO: Check who is the last ship alive and change text accordingly
            gameEndMenuText.text = "LocMP GameEnd";
            OpenGameEndMenu();
        }
    }
    #endregion

    #region Input subscribers
    private void OnEscapeButtonDown(int controllerIndex)
    {
        if (inGameUIHolder.activeSelf && !gameEndMenuHolder.activeSelf)
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
        em.BroadcastSetGameMode(gameModeSingleplayerIndex);
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
        pauseMenuResumeButton.onClick.AddListener(PauseMenuResumeButtonPressed);
        pauseMenuRestartButton.onClick.AddListener(PauseMenuRestartButtonPressed);
        pauseMenuMainMenuButton.onClick.AddListener(PauseMenuMainMenuButtonPressed);
        pauseMenuHolder.SetActive(true);
        em.BroadcastPauseOn();
    }

    private void ClosePauseMenu()
    {
        pauseMenuResumeButton.onClick.RemoveAllListeners();
        pauseMenuRestartButton.onClick.RemoveAllListeners();
        pauseMenuMainMenuButton.onClick.RemoveAllListeners();
        pauseMenuHolder.SetActive(false);
        em.BroadcastPauseOff();
    }

    private void OpenGameEndMenu()
    {
        ClosePauseMenu();
        gameEndMenuRestartButton.onClick.AddListener(GameEndMenuRestartButtonPressed);
        gameEndMenuMainMenuButton.onClick.AddListener(GameEndMenuMainMenuButtonPressed);
        gameEndMenuHolder.SetActive(true);
        em.BroadcastPauseOn();
    }

    private void CloseGameEndMenu()
    {
        gameEndMenuText.text = "GAME END MENU";
        gameEndMenuRestartButton.onClick.RemoveAllListeners();
        gameEndMenuMainMenuButton.onClick.RemoveAllListeners();
        gameEndMenuHolder.SetActive(false);
        em.BroadcastPauseOff();
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

    #region GameEndMenu buttons
    private void GameEndMenuRestartButtonPressed()
    {
        Debug.Log("Restart button pressed");
        //Close gameEndMenu
        CloseGameEndMenu();
        //Restart game
        em.BroadcastGameRestart();
    }

    private void GameEndMenuMainMenuButtonPressed()
    {
        Debug.Log("ReturnToMainMenu button pressed");
        //Close gameEndMenu (happens when loading main menu)
        //Close InGameUI (happens when loading main menu)
        //Load mainMenuScene
        em.BroadcastRequestSceneSingleMainMenu();
        //Open mainMenuUI (happens when loading main menu)
    }
    #endregion
    #endregion
    #endregion

}
