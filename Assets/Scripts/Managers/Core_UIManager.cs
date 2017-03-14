using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Core_UIManager : MonoBehaviour {

    /* TODO: 
     * - Find a way to have the game stay paused (ship immoveable and invulnerable) in a case where
     *      pause menu is opened during matchStartTimer and left open
     *      --> When timer is zero, game is effectively unpaused
     *      - A separate isPaused bool that's state is checked when ever unpausing the game?
     *      - A check in Update loop to constantly check pause state?
    */

    #region References & variables
    public static Core_UIManager instance;
    
    Core_Toolbox toolbox;
    Core_GlobalVariableLibrary lib;
    Core_EventManager em;
    GameObject canvas;

    //MainMenu UI
    GameObject mainMenuHolder;
    GameObject mainMenuCenter;
    GameObject mainMenuLeftPanel;
    GameObject mainMenuRightPanel;
    GameObject mainMenuTitle;
    GameObject mainMenuPlayButton;
    GameObject mainMenuExitButton;
    GameObject mainMenuSettingsButton;
    GameObject mainMenuReturnButton;
    GameObject settingsInvertedHUDButtonHolder;
    GameObject settingsInvertHUDToggleMarkOnImage;
    GameObject mainMenuGameModeSinglePlayerButton;
    GameObject mainMenuGameModeNetworkMultiplayerButton;
    GameObject mainMenuGameModeLocalMultiplayerButton;
    Transform mainMenuLeftSlotTop;
    Transform mainMenuLeftSlotBot;
    Transform mainMenuRightSlotTop;
    Transform mainMenuRightSlotBot;

    //InGame UI
    GameObject inGameUIHolder;
    GameObject pauseMenuHolder;
    GameObject hudOfflineImage;
    GameObject hudLeftPanel;
    GameObject hudRightPanel;
    GameObject hudVirtualJoystickOne;
    GameObject hudVirtualJoystickTwo;
    GameObject hudOpenPauseMenuButton;

    GameObject pauseMenuText;
    GameObject pauseMenuResumeButton;
    GameObject pauseMenuRestartButton;
    GameObject pauseMenuMainMenuButton;

    Transform pauseMenuPanel;
    Transform offscreenIndicatorHolder;
    Transform hudHolder;
    Transform hudRightSlotTop;
    Transform hudRightSlotMid;
    Transform hudRightSlotBot;
    Transform hudLeftSlotTop;
    Transform hudLeftSlotMid;
    Transform hudLeftSlotBot;
    Image loadingScreenImage;
    Text matchStartTimerText;
    List<Transform> offscreenIndicatorPool = new List<Transform>();
    List<Transform> offscreenIndicatorTargets = new List<Transform>();

    enum UIState
    {
        MAINMENUDEFAULT,
        MAINMENUSETTINGS,
        MAINMENUGAMEMODE,
        INGAMEDEFAULT,
        INGAMEPAUSEMENU,
        INGAMEGAMEENDMENU
    };

    UIState uiState;

    Color loadingScreenNewColor;
    Color loadingScreenOriginalColor;
    Vector3 offscreenIndicatorDefaultPosition;
    bool isFadingFromLoadingScreen = false;
    bool followingOffscreenTargets = false;
    int matchStartTimerValue = -1;
    int currentGameModeIndex = -1;
    float loadingScreenFadeStartTime = -1;
    float offscreenIndicatorSidebuffer = -1;
    //Variables coming from globalVariableLibrary
    string winText;
    string lossText;
    int gameModeSingleplayerIndex = -1;
    int gameModeNetworkMultiplayerIndex = -1;
    int gameModeLocalMultiplayerIndex = -1;
    int sceneIndexMainMenu = -1;
    int sceneIndexLevel01 = -1;
    int buildPlatform = -1; //0 = PC, 1 = Android
    float loadingScreenFadeTime = -1;
    float offscreenIndicatorSidebufferPC = -1;
    float offscreenIndicatorSidebufferAndroid = -1;
    bool invertedHUD = false;
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

        //General UI
        canvas = transform.GetComponentInChildren<Canvas>(true).gameObject;
        loadingScreenImage = canvas.GetComponentInChildren<Core_LoadingScreenImageTag>(true).
            GetComponent<Image>();

        //MainMenu
        mainMenuHolder = canvas.GetComponentInChildren<Core_MainMenuHolderTag>(true).gameObject;
        mainMenuCenter = Instantiate(Resources.Load("UI/MainMenu/MainMenuCenter", typeof(GameObject)),
                mainMenuHolder.transform) as GameObject;
        mainMenuRightPanel = Instantiate(Resources.Load("UI/MainMenu/MainMenuRightPanel", typeof(GameObject)),
            mainMenuHolder.transform) as GameObject;
        mainMenuRightSlotTop = mainMenuRightPanel.GetComponentInChildren<Core_UISlotTopTag>(true).transform;
        mainMenuRightSlotBot = mainMenuRightPanel.GetComponentInChildren<Core_UISlotBotTag>(true).transform;
        mainMenuLeftPanel = Instantiate(Resources.Load("UI/MainMenu/MainMenuLeftPanel", typeof(GameObject)),
            mainMenuHolder.transform) as GameObject;
        mainMenuLeftSlotTop = mainMenuRightPanel.GetComponentInChildren<Core_UISlotTopTag>(true).transform;
        mainMenuLeftSlotBot = mainMenuLeftPanel.GetComponentInChildren<Core_UISlotBotTag>(true).transform;
        mainMenuTitle = Instantiate(Resources.Load("UI/MainMenu/MainMenuTitle", typeof(GameObject)),
                mainMenuCenter.transform) as GameObject;

        //HUD
        inGameUIHolder = canvas.GetComponentInChildren<Core_InGameUIHolderTag>(true).gameObject;
        hudHolder = inGameUIHolder.GetComponentInChildren<Core_HUDHolderTag>(true).transform;
        hudHolder.gameObject.SetActive(true);
        matchStartTimerText = inGameUIHolder.GetComponentInChildren<Core_MatchBeginTimerTag>(true).
            GetComponent<Text>();
        offscreenIndicatorHolder = inGameUIHolder.GetComponentInChildren<Core_OffscreenIndicatorHolderTag>(true).
            transform;
        offscreenIndicatorHolder.gameObject.SetActive(true);
        hudOfflineImage = inGameUIHolder.GetComponentInChildren<Core_HUDOffilneImageTag>(true).gameObject;

        if (buildPlatform < 0)
        {
            Debug.LogError("UIManager: BuildPlatform not set!");
        }
        if (buildPlatform == 0)
        {
            Debug.Log("UIManager: BuildPlatform set to PC");
            offscreenIndicatorSidebuffer = offscreenIndicatorSidebufferPC;
        }
        else if (buildPlatform == 1)
        {
            Debug.Log("UIManager: BuildPlatform set to Android");
            hudLeftPanel = Instantiate(Resources.Load("UI/HUD/HUDLeftPanel", typeof(GameObject)),
                hudHolder.transform) as GameObject;
            
            hudRightPanel = Instantiate(Resources.Load("UI/HUD/HUDRightPanel", typeof(GameObject)),
                hudHolder.transform) as GameObject;


            hudLeftSlotTop = hudLeftPanel.GetComponentInChildren<Core_UISlotTopTag>(true).transform;
            hudLeftSlotMid = hudLeftPanel.GetComponentInChildren<Core_UISlotMidTag>(true).transform;
            hudLeftSlotBot = hudLeftPanel.GetComponentInChildren<Core_UISlotBotTag>(true).transform;
            hudRightSlotTop = hudRightPanel.GetComponentInChildren<Core_UISlotTopTag>(true).transform;
            hudRightSlotMid = hudRightPanel.GetComponentInChildren<Core_UISlotMidTag>(true).transform;
            hudRightSlotBot = hudRightPanel.GetComponentInChildren<Core_UISlotBotTag>(true).transform;

            offscreenIndicatorSidebuffer = offscreenIndicatorSidebufferAndroid;
        }

        //PauseMenu
        pauseMenuHolder = inGameUIHolder.GetComponentInChildren<Core_PauseMenuHolderTag>(true).gameObject;
        pauseMenuPanel = pauseMenuHolder.transform.GetChild(0);
        pauseMenuText = Instantiate(Resources.Load("UI/HUD/PauseMenu/PauseMenuText", typeof(GameObject)),
                pauseMenuPanel.transform) as GameObject;
        #endregion
    }
    #endregion

    #region GetStats
    private void GetStats()
    {
        buildPlatform = lib.gameSettingVariables.buildPlatform;
        invertedHUD = lib.uiVariables.invertedHUD;
        gameModeSingleplayerIndex = lib.gameSettingVariables.gameModeSingleplayerIndex;
        gameModeNetworkMultiplayerIndex = lib.gameSettingVariables.gameModeNetworkMultiplayerIndex;
        gameModeLocalMultiplayerIndex = lib.gameSettingVariables.gameModeLocalMultiplayerIndex;
        sceneIndexMainMenu = lib.sceneVariables.sceneIndexMainMenu;
        sceneIndexLevel01 = lib.sceneVariables.sceneIndexLevel01;
        loadingScreenFadeTime = lib.uiVariables.fadeFromBlackTime;
        winText = lib.uiVariables.winText;
        lossText = lib.uiVariables.lossText;
        offscreenIndicatorSidebufferPC = lib.uiVariables.offscreenIndicatorSidebufferPC;
        offscreenIndicatorSidebufferAndroid = lib.uiVariables.offscreenIndicatorSidebufferAndroid;
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
        em.OnShipReference += OnShipReference;
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
        em.OnShipReference -= OnShipReference;
    }
    #endregion

    #region Subscribers
    #region GameEvent subscribers
    private void OnMatchStartTimerValue(int currentTimerValue)
    {
        UpdateMatchStartTimer(currentTimerValue);

        if (currentTimerValue == 0)
        {
            followingOffscreenTargets = true;
        }
    }

    private void OnGameRestart()
    {
        //Reset neccessary values
        OpenLoadingScreen();
        StartFadeFromLoadingScreen();
    }

    private void OnNewSceneLoading(int sceneIndex)
    {
        //TODO: Add checks for all future scenes
        if (sceneIndex == sceneIndexMainMenu)
        {
            OpenLoadingScreen();
        }
        else if (sceneIndex == sceneIndexLevel01)
        {
            OpenLoadingScreen();
        }
    }

    private void OnNewSceneLoaded(int sceneIndex)
    {
        //TODO: Add checks for all future scenes
        if (sceneIndex == sceneIndexMainMenu)
        {
            CloseInGameUI();
            OpenMainMenuUI();
            CloseLoadingScreen();
        }
        else if (sceneIndex == sceneIndexLevel01)
        {
            OpenLoadingScreen();
            CloseMainMenuUI();
            OpenInGameUI();
            ClosePauseMenu();
            CloseGameEndMenu();
            StartFadeFromLoadingScreen();
        }
    }

    private void OnSetGameMode(int newGameModeIndex)
    {
        currentGameModeIndex = newGameModeIndex;
    }

    private void OnGameEnd(int newWinnerIndex)
    {
        ResetOffscreenTargetFollowing();

        if (currentGameModeIndex == gameModeSingleplayerIndex)
        {
            if (newWinnerIndex == 1)
            {
                pauseMenuText.GetComponentInChildren<Text>().text = winText;
            }
            else
            {
                pauseMenuText.GetComponentInChildren<Text>().text = lossText;
            }
            OpenGameEndMenu();
        }
        else if (currentGameModeIndex == gameModeNetworkMultiplayerIndex)
        {
            // TODO: Check if last ship alive is localPlayer and change text accordingly
            pauseMenuText.GetComponentInChildren<Text>().text = "NetMp GameEnd";
            OpenGameEndMenu();
        }
        else if (currentGameModeIndex == gameModeLocalMultiplayerIndex)
        {
            // TODO: Check who is the last ship alive and change text accordingly
            pauseMenuText.GetComponentInChildren<Text>().text = "LocMP GameEnd";
            OpenGameEndMenu();
        }
    }

    private void OnShipReference(GameObject newShip)
    {
        if (newShip.GetComponent<Core_LocalPlayerController>() == null)
        {
            offscreenIndicatorTargets.Add(newShip.transform);
            if (offscreenIndicatorTargets.Count > offscreenIndicatorPool.Count)
            {
                GameObject newOffscreenIndicator = Instantiate(Resources.Load("UI/HUD/HUDOffscreenIndicator", typeof(GameObject)),
                     offscreenIndicatorHolder.position, offscreenIndicatorHolder.rotation, offscreenIndicatorHolder) as GameObject;
                Transform newOffscreenIndicatorTransform = newOffscreenIndicator.transform;
                offscreenIndicatorDefaultPosition = newOffscreenIndicatorTransform.position;
                offscreenIndicatorPool.Add(newOffscreenIndicatorTransform);
                newOffscreenIndicator.SetActive(false);
            }
        }
    }
    #endregion

    #region Input subscribers
    private void OnEscapeButtonDown(int controllerIndex)
    {
        if (uiState == UIState.INGAMEDEFAULT)
        {
            OpenPauseMenu();
        }
        else if (uiState == UIState.INGAMEPAUSEMENU)
        {
            ClosePauseMenu();
        }
        else if (uiState == UIState.MAINMENUSETTINGS)
        {
            CloseMainMenuSettingsView();
            OpenMainMenuDefaultView();
        }
        else if (uiState == UIState.MAINMENUGAMEMODE)
        {
            CloseMainMenuGameModeView();
            OpenMainMenuDefaultView();
        }
    }
    #endregion
    #endregion
    #endregion

    #region FixedUpdate & LateUpdate
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
                CloseLoadingScreen();
                loadingScreenImage.color = loadingScreenOriginalColor;
            }
        }
        #endregion
    }

    private void LateUpdate()
    {
        if (followingOffscreenTargets)
        {
            FollowOffscreenTargets();
        }
    }
    #endregion

    #region UI functions
    #region Global UI
    private void OpenLoadingScreen()
    {
        loadingScreenImage.gameObject.SetActive(true);
    }

    private void CloseLoadingScreen()
    {
        loadingScreenImage.gameObject.SetActive(false);
    }
    #endregion

    #region MainMenu UI
    #region Toggle UI Elements
    private void OpenMainMenuUI()
    {
        OpenMainMenuDefaultView();
        mainMenuHolder.SetActive(true);
    }

    private void CloseMainMenuUI()
    {
        CloseMainMenuSettingsView();
        CloseMainMenuGameModeView();
        CloseMainMenuDefaultView();
        mainMenuHolder.SetActive(false);
    }

    private void OpenMainMenuDefaultView()
    {
        mainMenuPlayButton = Instantiate(Resources.Load("UI/MainMenu/MainMenuButton", typeof(GameObject)),
                mainMenuCenter.transform) as GameObject;
        mainMenuPlayButton.GetComponentInChildren<Text>().text = "PLAY";
        mainMenuPlayButton.GetComponent<Button>().onClick.AddListener(OnMainMenuPlayButtonPressed);
        mainMenuExitButton = Instantiate(Resources.Load("UI/MainMenu/MainMenuButton", typeof(GameObject)),
                mainMenuCenter.transform) as GameObject;
        mainMenuExitButton.GetComponentInChildren<Text>().text = "EXIT";
        mainMenuExitButton.GetComponent<Button>().onClick.AddListener(OnMainMenuExitButtonPressed);
        mainMenuSettingsButton = Instantiate(Resources.Load("UI/MainMenu/MainMenuSettingsButton", typeof(GameObject)),
            mainMenuRightSlotBot) as GameObject;
        mainMenuSettingsButton.GetComponent<Button>().onClick.AddListener(OnMainMenuSettingsButtonPressed);
        uiState = UIState.MAINMENUDEFAULT;
    }

    private void CloseMainMenuDefaultView()
    {
        if (mainMenuPlayButton != null)
        {
            mainMenuPlayButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(mainMenuPlayButton);
        }
        if(mainMenuExitButton != null)
        {
            mainMenuExitButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(mainMenuExitButton);
        }
        if(mainMenuSettingsButton != null)
        {
            mainMenuSettingsButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(mainMenuSettingsButton);
        }
    }

    private void OpenMainMenuSettingsView()
    {
        //InvertedHUD button
        settingsInvertedHUDButtonHolder = Instantiate(Resources.Load("UI/MainMenu/MainMenuButtonWithToggleMark", typeof(GameObject)),
                mainMenuCenter.transform) as GameObject;
        settingsInvertedHUDButtonHolder.GetComponentInChildren<Text>().text = "INVERTED HUD";
        settingsInvertHUDToggleMarkOnImage = settingsInvertedHUDButtonHolder.GetComponentInChildren<Core_ToggleMarkTag>().transform.
            GetChild(1).gameObject;
        if (invertedHUD)
            settingsInvertHUDToggleMarkOnImage.SetActive(true);
        else
            settingsInvertHUDToggleMarkOnImage.SetActive(false);
        settingsInvertedHUDButtonHolder.GetComponentInChildren<Button>(true).onClick.AddListener(OnSettingsInvertHUDButtonPressed);
        mainMenuReturnButton = Instantiate(Resources.Load("UI/MainMenu/MainMenuReturnButton", typeof(GameObject)),
                mainMenuRightSlotBot.transform) as GameObject;
        mainMenuReturnButton.GetComponent<Button>().onClick.AddListener(OnMainMenuReturnButtonPressed);
        uiState = UIState.MAINMENUSETTINGS;
    }

    private void CloseMainMenuSettingsView()
    {
        if(settingsInvertedHUDButtonHolder != null)
        {
            settingsInvertedHUDButtonHolder.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            Destroy(settingsInvertedHUDButtonHolder);
        }
        if (mainMenuReturnButton != null)
        {
            mainMenuReturnButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(mainMenuReturnButton);
        }
    }

    private void OpenMainMenuGameModeView()
    {
        mainMenuGameModeSinglePlayerButton = Instantiate(Resources.Load("UI/MainMenu/MainMenuButton", typeof(GameObject)),
                mainMenuCenter.transform) as GameObject;
        mainMenuGameModeSinglePlayerButton.GetComponentInChildren<Text>().text = "SINGLEPLAYER";
        mainMenuGameModeSinglePlayerButton.GetComponent<Button>().onClick.AddListener(OnMainMenuGameModeSinglePlayerButtonPressed);

        mainMenuGameModeNetworkMultiplayerButton = Instantiate(Resources.Load("UI/MainMenu/MainMenuButton", typeof(GameObject)),
                mainMenuCenter.transform) as GameObject;
        mainMenuGameModeNetworkMultiplayerButton.GetComponentInChildren<Text>().text = "ONLINE MULTIPLAYER";
        mainMenuGameModeNetworkMultiplayerButton.GetComponent<Button>().onClick.AddListener(OnMainMenuGameModeNetworkMultiplayerButtonPressed);

        mainMenuGameModeLocalMultiplayerButton = Instantiate(Resources.Load("UI/MainMenu/MainMenuButton", typeof(GameObject)),
                mainMenuCenter.transform) as GameObject;
        mainMenuGameModeLocalMultiplayerButton.GetComponentInChildren<Text>().text = "LOCAL MULTIPLAYER";
        mainMenuGameModeLocalMultiplayerButton.GetComponent<Button>().onClick.AddListener(OnMainMenuGameModeLocalMultiplayerButtonPressed);

        mainMenuReturnButton = Instantiate(Resources.Load("UI/MainMenu/MainMenuReturnButton", typeof(GameObject)),
                mainMenuRightSlotBot.transform) as GameObject;
        mainMenuReturnButton.GetComponent<Button>().onClick.AddListener(OnMainMenuReturnButtonPressed);

        uiState = UIState.MAINMENUGAMEMODE;
    }

    private void CloseMainMenuGameModeView()
    {
        if (mainMenuGameModeSinglePlayerButton != null)
        {
            mainMenuGameModeSinglePlayerButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(mainMenuGameModeSinglePlayerButton);
        }
        if (mainMenuGameModeNetworkMultiplayerButton != null)
        {
            mainMenuGameModeNetworkMultiplayerButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(mainMenuGameModeNetworkMultiplayerButton);
        }
        if (mainMenuGameModeLocalMultiplayerButton != null)
        {
            mainMenuGameModeLocalMultiplayerButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(mainMenuGameModeLocalMultiplayerButton);
        }
        if (mainMenuReturnButton != null)
        {
            mainMenuReturnButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(mainMenuReturnButton);
        }

        uiState = UIState.MAINMENUDEFAULT;
    }
    #endregion

    #region Button functions
    private void OnMainMenuPlayButtonPressed()
    {
        //OpenLoadingScreen();
        //em.BroadcastSetGameMode(gameModeSingleplayerIndex);
        //em.BroadcastRequestSceneSingleLevel01();
        CloseMainMenuDefaultView();
        OpenMainMenuGameModeView();
    }

    private void OnMainMenuExitButtonPressed()
    {
        Debug.Log("UIManager: MainMenuExitButton pressed");
        Application.Quit();
    }

    private void OnMainMenuSettingsButtonPressed()
    {
        CloseMainMenuDefaultView();
        OpenMainMenuSettingsView();
    }

    private void OnSettingsInvertHUDButtonPressed()
    {
        //TODO: Broadcast invertedHUD change and save it to GVL
        invertedHUD = !invertedHUD;

        if (invertedHUD)
            settingsInvertHUDToggleMarkOnImage.SetActive(true);
        else
            settingsInvertHUDToggleMarkOnImage.SetActive(false);
    }

    private void OnMainMenuReturnButtonPressed()
    {
        if (uiState == UIState.MAINMENUSETTINGS)
        {
            CloseMainMenuSettingsView();
            OpenMainMenuDefaultView();
        }
        else if (uiState == UIState.MAINMENUGAMEMODE)
        {
            CloseMainMenuGameModeView();
            OpenMainMenuDefaultView();
        }
    }

    private void OnMainMenuGameModeSinglePlayerButtonPressed()
    {
        em.BroadcastSetGameMode(gameModeSingleplayerIndex);
        OpenLoadingScreen();
        em.BroadcastRequestSceneSingleLevel01();
    }

    private void OnMainMenuGameModeNetworkMultiplayerButtonPressed()
    {
        em.BroadcastSetGameMode(gameModeNetworkMultiplayerIndex);
        Debug.Log("Game mode changed to Network Multiplayer");
        //OpenLoadingScreen();
        //em.BroadcastRequestSceneSingleLevel01();
    }

    private void OnMainMenuGameModeLocalMultiplayerButtonPressed()
    {
        em.BroadcastSetGameMode(gameModeLocalMultiplayerIndex);
        Debug.Log("Game mode changed to Local Multiplayer");
        //OpenLoadingScreen();
        //em.BroadcastRequestSceneSingleLevel01();
    }
    #endregion
    #endregion

    #region InGame UI
    #region Toggle UI Elements
    private void OpenInGameUI()
    {
        if (buildPlatform == 1)
        {
            if (!invertedHUD)
            {
                //Pause button
                hudOpenPauseMenuButton = Instantiate(Resources.Load("UI/HUD/HUDOpenPauseMenuButton", typeof(GameObject)),
                    hudRightSlotTop.position, hudRightSlotTop.rotation, hudRightSlotTop) as GameObject;
                //Movement joystick
                hudVirtualJoystickOne = Instantiate(Resources.Load("UI/HUD/HUDVirtualJoystickHolder", typeof(GameObject)),
                    hudLeftSlotBot.position, hudLeftSlotBot.rotation, hudLeftSlotBot) as GameObject;
                hudVirtualJoystickOne.GetComponent<Core_VirtualJoystick>().SetIndex(1);
                //Shooting joystick
                hudVirtualJoystickTwo = Instantiate(Resources.Load("UI/HUD/HUDVirtualJoystickHolder", typeof(GameObject)),
                    hudRightSlotBot.position, hudRightSlotBot.rotation, hudRightSlotBot) as GameObject;
                hudVirtualJoystickTwo.GetComponent<Core_VirtualJoystick>().SetIndex(2);
            }
            else
            {
                //Pause button
                hudOpenPauseMenuButton = Instantiate(Resources.Load("UI/HUD/HUDOpenPauseMenuButton", typeof(GameObject)),
                    hudLeftSlotTop.position, hudLeftSlotTop.rotation, hudLeftSlotTop) as GameObject;
                //Movement joystick
                hudVirtualJoystickOne = Instantiate(Resources.Load("UI/HUD/HUDVirtualJoystickHolder", typeof(GameObject)),
                    hudRightSlotBot.position, hudRightSlotBot.rotation, hudRightSlotBot) as GameObject;
                hudVirtualJoystickOne.GetComponent<Core_VirtualJoystick>().SetIndex(1);
                //Shooting joystick
                hudVirtualJoystickTwo = Instantiate(Resources.Load("UI/HUD/HUDVirtualJoystickHolder", typeof(GameObject)),
                    hudLeftSlotBot.position, hudLeftSlotBot.rotation, hudLeftSlotBot) as GameObject;
                hudVirtualJoystickTwo.GetComponent<Core_VirtualJoystick>().SetIndex(2);
            }

            hudOpenPauseMenuButton.GetComponent<Button>().onClick.AddListener(HUDOpenPauseMenuButtonPressed);
        }

        inGameUIHolder.SetActive(true);
        offscreenIndicatorHolder.gameObject.SetActive(true);
        HUDOnline();
        uiState = UIState.INGAMEDEFAULT;
    }

    private void CloseInGameUI()
    {
        if (buildPlatform == 1)
        {
            if (hudOpenPauseMenuButton != null)
            {
                Destroy(hudOpenPauseMenuButton.gameObject);
            }

            if (hudVirtualJoystickOne != null)
            {
                Destroy(hudVirtualJoystickOne);
            }

            if (hudVirtualJoystickTwo != null)
            {
                Destroy(hudVirtualJoystickTwo);
            }
        }

        inGameUIHolder.SetActive(false);
        ResetOffscreenTargetFollowing();
        DestroyOffscreenIndicators();
        HUDOffline();
    }

    private void OpenPauseMenu()
    {
        HUDOffline();
        em.BroadcastPauseOn();

        pauseMenuText.GetComponentInChildren<Text>().text = "PAUSE MENU";

        pauseMenuResumeButton = Instantiate(Resources.Load("UI/HUD/PauseMenu/PauseMenuButton", typeof(GameObject)),
                pauseMenuPanel.transform) as GameObject;
        pauseMenuResumeButton.GetComponentInChildren<Text>().text = "RESUME";
        pauseMenuResumeButton.GetComponent<Button>().onClick.AddListener(PauseMenuResumeButtonPressed);

        pauseMenuRestartButton = Instantiate(Resources.Load("UI/HUD/PauseMenu/PauseMenuButton", typeof(GameObject)),
                pauseMenuPanel.transform) as GameObject;
        pauseMenuRestartButton.GetComponentInChildren<Text>().text = "RESTART";
        pauseMenuRestartButton.GetComponent<Button>().onClick.AddListener(PauseMenuRestartButtonPressed);

        pauseMenuMainMenuButton = Instantiate(Resources.Load("UI/HUD/PauseMenu/PauseMenuButton", typeof(GameObject)),
                pauseMenuPanel.transform) as GameObject;
        pauseMenuMainMenuButton.GetComponentInChildren<Text>().text = "MAIN MENU";
        pauseMenuMainMenuButton.GetComponent<Button>().onClick.AddListener(PauseMenuMainMenuButtonPressed);

        pauseMenuHolder.SetActive(true);
        uiState = UIState.INGAMEPAUSEMENU;
    }

    private void ClosePauseMenu()
    {
        if (pauseMenuResumeButton != null)
        {
            pauseMenuResumeButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(pauseMenuResumeButton);
        }
        if (pauseMenuRestartButton != null)
        {
            pauseMenuRestartButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(pauseMenuRestartButton);
        }
        if (pauseMenuMainMenuButton != null)
        {
            pauseMenuMainMenuButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(pauseMenuMainMenuButton);
        }
        
        pauseMenuHolder.SetActive(false);
        em.BroadcastPauseOff();
        HUDOnline();
        uiState = UIState.INGAMEDEFAULT;
    }

    private void OpenGameEndMenu()
    {
        ClosePauseMenu();

        HUDOffline();
        em.BroadcastPauseOn();

        pauseMenuRestartButton = Instantiate(Resources.Load("UI/HUD/PauseMenu/PauseMenuButton", typeof(GameObject)),
                pauseMenuPanel.transform) as GameObject;
        pauseMenuRestartButton.GetComponentInChildren<Text>().text = "RESTART";
        pauseMenuRestartButton.GetComponent<Button>().onClick.AddListener(PauseMenuRestartButtonPressed);

        pauseMenuMainMenuButton = Instantiate(Resources.Load("UI/HUD/PauseMenu/PauseMenuButton", typeof(GameObject)),
                pauseMenuPanel.transform) as GameObject;
        pauseMenuMainMenuButton.GetComponentInChildren<Text>().text = "MAIN MENU";
        pauseMenuMainMenuButton.GetComponent<Button>().onClick.AddListener(PauseMenuMainMenuButtonPressed);

        pauseMenuHolder.SetActive(true);
        uiState = UIState.INGAMEGAMEENDMENU;
    }

    private void CloseGameEndMenu()
    {
        if (pauseMenuRestartButton != null)
        {
            pauseMenuRestartButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(pauseMenuRestartButton);
        }
        if (pauseMenuMainMenuButton != null)
        {
            pauseMenuMainMenuButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(pauseMenuMainMenuButton);
        }

        pauseMenuHolder.SetActive(false);
        em.BroadcastPauseOff();
        HUDOnline();
        uiState = UIState.INGAMEDEFAULT;
    }

    private void HUDOffline()
    {
        hudOfflineImage.SetActive(true);
    }

    private void HUDOnline()
    {
        hudOfflineImage.SetActive(false);
    }
    #endregion

    #region PauseMenu buttons
    private void PauseMenuResumeButtonPressed()
    {
        ClosePauseMenu();
    }

    private void PauseMenuRestartButtonPressed()
    {
        OpenLoadingScreen();
        ClosePauseMenu();
        ResetOffscreenTargetFollowing();
        em.BroadcastGameRestart();
    }

    private void PauseMenuMainMenuButtonPressed()
    {
        OpenLoadingScreen();
        em.BroadcastRequestSceneSingleMainMenu();
    }
    #endregion

    #region HUD buttons
    private void HUDOpenPauseMenuButtonPressed()
    {
        if (uiState == UIState.INGAMEDEFAULT)
        {
            OpenPauseMenu();
        }
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
        //TODO: Remove below if deemed permanently obsolete and implement 
        //      a check to see if the loading screen is open
        OpenLoadingScreen();

        loadingScreenOriginalColor = loadingScreenImage.color;
        loadingScreenOriginalColor.a = 1;
        loadingScreenNewColor = loadingScreenOriginalColor;
        isFadingFromLoadingScreen = true;
        loadingScreenFadeStartTime = Time.time;
    }
    #endregion

    #region Offscreen indicators
    private void FollowOffscreenTargets()
    {
        if(offscreenIndicatorTargets.Count > 0)
        {
            for (int i = 0; i < offscreenIndicatorTargets.Count; i++)
            {
                if (offscreenIndicatorTargets[i] == null)
                {
                    Debug.Log("Removing element " + i);
                    offscreenIndicatorTargets.RemoveAt(i);
                    offscreenIndicatorPool[offscreenIndicatorTargets.Count].gameObject.SetActive(false);
                    offscreenIndicatorPool[offscreenIndicatorTargets.Count].position = offscreenIndicatorDefaultPosition;
                    Debug.Log("New offscreenIndicatorTargets.Count: " + offscreenIndicatorTargets.Count);
                    i--;
                }
                else
                {
                    Transform target = offscreenIndicatorTargets[i];
                    Transform indicator = offscreenIndicatorPool[i];
                    Vector3 screenPosition = Camera.main.WorldToViewportPoint(target.position);

                    if (screenPosition.x >= (-0.08f + offscreenIndicatorSidebuffer) && screenPosition.x <= (1.08f - offscreenIndicatorSidebuffer) && 
                        screenPosition.y >= -0.08f && screenPosition.y <= 1.08f)
                    {
                        //Target is within screenspace
                        indicator.gameObject.SetActive(false);
                        indicator.position = offscreenIndicatorDefaultPosition;
                    }
                    else
                    {
                        //Target is outside of screenspace
                        if (i > offscreenIndicatorPool.Count)
                            Debug.LogError("More ships than indicators! Implement spawning system if neccessary.");
                 
                        if (!indicator.gameObject.activeSelf)
                        {
                            indicator.gameObject.SetActive(true);
                        }
                        Color indicatorColor = target.GetComponent<Core_ShipController>().GetShipColor();
                        indicatorColor.a = indicator.GetChild(0).GetComponent<Image>().color.a;
                        indicator.GetChild(0).GetComponent<Image>().color = indicatorColor;

                        #region Variant 01
                        //screenPosition.x -= 0.5f;
                        //screenPosition.y -= 0.5f;
                        //screenPosition.z = 0;
                        //float angle = Mathf.Atan2(screenPosition.x, screenPosition.y);
                        //screenPosition.x += 0.5f;
                        //screenPosition.y += 0.5f;

                        //float offset = 0.05f;
                        //screenPosition.x = Mathf.Clamp(screenPosition.x, 0.0f + offset, 1.0f - offset);
                        //screenPosition.y = Mathf.Clamp(screenPosition.y, 0.0f + offset, 1.0f - offset);
                        #endregion

                        #region Variant 02
                        screenPosition.x -= 0.5f;
                        screenPosition.y -= 0.5f;
                        screenPosition.z = 0;
                        float angle = Mathf.Atan2(screenPosition.x, screenPosition.y);

                        screenPosition.x = 0.5f * Mathf.Sin(angle) + 0.5f;
                        screenPosition.y = 0.5f * Mathf.Cos(angle) + 0.5f;
                        screenPosition.z = Camera.main.nearClipPlane + 0.01f;
                        #endregion

                        #region Variant 03 [Does not work]
                        //Vector3 screenCenter = new Vector3(1, 1, 0) / 2;
                        //screenPosition -= screenCenter;
                        //if (screenPosition.z < 0)
                        //{
                        //    screenPosition *= -1;
                        //}


                        //float angle = Mathf.Atan2(screenPosition.y, screenPosition.x);
                        //angle -= 90 * Mathf.Deg2Rad;

                        //float cos = Mathf.Cos(angle);
                        //float sin = Mathf.Sin(angle);

                        //screenPosition = screenCenter + new Vector3(sin * 150, cos * 150, 0);

                        ////Slope intercept form
                        //float m = cos / sin;

                        //Vector3 screenBounds = screenCenter * 0.9f;

                        //if (cos > 0)
                        //    screenPosition = new Vector3(screenBounds.y / m, screenBounds.y, 0);
                        //else
                        //    screenPosition = new Vector3(-screenBounds.y / m, -screenBounds.y, 0);

                        //if (screenPosition.x > screenBounds.x)
                        //    screenPosition = new Vector3(screenBounds.x, screenBounds.x * m, 0);
                        //else
                        //    screenPosition = new Vector3(-screenBounds.x, -screenBounds.x * m, 0);

                        //screenPosition += screenCenter;

                        //float offset = 0.05f;
                        //screenPosition.x = Mathf.Clamp(screenPosition.x, 0.0f + offset, 1.0f - offset);
                        //screenPosition.y = Mathf.Clamp(screenPosition.y, 0.0f + offset, 1.0f - offset);
                        #endregion

                        indicator.localEulerAngles = new Vector3(0.0f, 0.0f, -angle * Mathf.Rad2Deg);
                        screenPosition.x = Mathf.Clamp(screenPosition.x, (0 + offscreenIndicatorSidebuffer), 
                            (1 - offscreenIndicatorSidebuffer));
                        indicator.position = Camera.main.ViewportToScreenPoint(screenPosition);

                    }

                }
            }
        }
    }

    private void ResetOffscreenTargetFollowing()
    {
        followingOffscreenTargets = false;
        offscreenIndicatorTargets.Clear();
        foreach(Transform indicator in offscreenIndicatorPool)
        {
            indicator.gameObject.SetActive(false);
            offscreenIndicatorPool[offscreenIndicatorTargets.Count].position = offscreenIndicatorDefaultPosition;
        }

    }

    private void DestroyOffscreenIndicators()
    {
        int poolSize = offscreenIndicatorPool.Count;
        for (int i = 0; i < poolSize; i++)
        {
            Destroy(offscreenIndicatorPool[0].gameObject);
            offscreenIndicatorPool.RemoveAt(0);
        }
    }
    #endregion
    #endregion
    #endregion

}
