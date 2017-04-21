using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{

    #region References & variables
    public static UIManager instance;

    Toolbox toolbox;
    GlobalVariableLibrary lib;
    EventManager em;
    GameObject canvas;
    GameObject uiNotification;
    GameObject loadingIcon;

    //MainMenu UI
    GameObject mainMenuHolder;
    GameObject mainMenuCenter;
    GameObject mainMenuLeftPanel;
    GameObject mainMenuRightPanel;
    GameObject mainMenuTitle;
    GameObject mainMenuPlayButton;
    GameObject mainMenuExitButton;
    GameObject mainMenuSettingsButton;
    GameObject mainMenuConnectButton;
    GameObject mainMenuReturnButton;
    GameObject settingsInvertedHUDButtonHolder;
    GameObject settingsInvertHUDToggleMarkOnImage;
    GameObject gameModeSinglePlayerButton;
    GameObject gameModeNetworkMultiplayerButton;
    GameObject gameModeLocalMultiplayerButton;
    GameObject lobbyReadyButtonHolder;
    GameObject lobbyReadyToggleMarkOnImage;
    GameObject lobbyStartMatchButton;
    GameObject lobbyParticipantCountDisplay;
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
    GameObject hudTopPanel;
    GameObject hudTimer;
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
    Text hudTimerText;
    List<Transform> offscreenIndicatorPool = new List<Transform>();
    List<Transform> offscreenIndicatorTargets = new List<Transform>();

    enum UIState
    {
        MAINMENUDEFAULT,
        MAINMENUSETTINGS,
        MAINMENUGAMEMODE,
        MAINMENUONLINELOBBY,
        INGAMEDEFAULT,
        INGAMEPAUSEMENU,
        INGAMEGAMEENDMENU
    };

    UIState uiState;

    Color loadingScreenNewColor;
    Color loadingScreenOriginalColor;
    Vector3 offscreenIndicatorDefaultPosition;
    bool isFadingFromLoadingScreen = false;
    bool matchStarted = false;
    bool connectedToNetwork = false;
    bool invertedHUD = false;
    bool lobbyReadyButtonPressed = false;
    bool loading = false;
    int matchStartTimerValue = -1;
    int currentGameModeIndex = -1;
    int numberOfPlayersInLobby = -1;
    int numberOfLobbyParticipantsReady = -1;
    float loadingScreenFadeStartTime = -1;
    float offscreenIndicatorSidebuffer = -1;
    float matchTimer = -1;
    List<string> notificationQueue = new List<string>();
    //Variables coming from globalVariableLibrary
    string winText;
    string lossText;
    int gameModeSingleplayerIndex = -1;
    int gameModeNetworkMultiplayerIndex = -1;
    int gameModeLocalMultiplayerIndex = -1;
    int sceneIndexMainMenu = -1;
    int sceneIndexLevel01 = -1;
    int buildPlatform = -1; //0 = PC, 1 = Android
    int maxNumberOfPlayersInLobby = -1;
    float loadingScreenFadeTime = -1;
    float offscreenIndicatorSidebufferPC = -1;
    float offscreenIndicatorSidebufferAndroid = -1;
    bool networkFunctionalityDisabled = false;
    bool returningToLobby = false;
    #endregion

    #region Initialization
    #region Awake
    void Awake()
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
        toolbox = FindObjectOfType<Toolbox>();
        em = toolbox.GetComponent<EventManager>();
        lib = toolbox.GetComponent<GlobalVariableLibrary>();
        GetStats();

        //General UI
        canvas = transform.GetComponentInChildren<Canvas>(true).gameObject;
        loadingScreenImage = canvas.GetComponentInChildren<LoadingScreenImageTag>(true).
            GetComponent<Image>();
        loadingIcon = Instantiate(Resources.Load("UI/LoadingIcon", typeof(GameObject)),
            canvas.transform) as GameObject;
        CloseLoadingIcon();

        //MainMenu
        mainMenuHolder = canvas.GetComponentInChildren<MainMenuHolderTag>(true).gameObject;


        mainMenuCenter = Instantiate(Resources.Load("UI/MainMenu/MainMenuCenter", typeof(GameObject)),
                mainMenuHolder.transform) as GameObject;
        mainMenuRightPanel = Instantiate(Resources.Load("UI/MainMenu/MainMenuRightPanel", typeof(GameObject)),
            mainMenuHolder.transform) as GameObject;
        mainMenuRightSlotTop = mainMenuRightPanel.GetComponentInChildren<UISlotTopTag>(true).transform;
        mainMenuRightSlotBot = mainMenuRightPanel.GetComponentInChildren<UISlotBotTag>(true).transform;
        mainMenuLeftPanel = Instantiate(Resources.Load("UI/MainMenu/MainMenuLeftPanel", typeof(GameObject)),
            mainMenuHolder.transform) as GameObject;
        mainMenuLeftSlotTop = mainMenuLeftPanel.GetComponentInChildren<UISlotTopTag>(true).transform;
        mainMenuLeftSlotBot = mainMenuLeftPanel.GetComponentInChildren<UISlotBotTag>(true).transform;
        mainMenuTitle = Instantiate(Resources.Load("UI/MainMenu/MainMenuTitle", typeof(GameObject)),
                mainMenuCenter.transform) as GameObject;

        //HUD
        inGameUIHolder = canvas.GetComponentInChildren<InGameUIHolderTag>(true).gameObject;
        hudHolder = inGameUIHolder.GetComponentInChildren<HUDHolderTag>(true).transform;
        hudHolder.gameObject.SetActive(true);
        matchStartTimerText = inGameUIHolder.GetComponentInChildren<MatchBeginTimerTag>(true).
            GetComponent<Text>();
        offscreenIndicatorHolder = inGameUIHolder.GetComponentInChildren<OffscreenIndicatorHolderTag>(true).
            transform;
        offscreenIndicatorHolder.gameObject.SetActive(true);
        hudOfflineImage = inGameUIHolder.GetComponentInChildren<HUDOffilneImageTag>(true).gameObject;

        if (buildPlatform < 0)
        {
            Debug.LogError("UIManager: BuildPlatform not set!");
        }
        if (buildPlatform == 0)
        {
            Debug.Log("UIManager: BuildPlatform set to PC");

            hudTopPanel = Instantiate(Resources.Load("UI/HUD/HUDTopPanel", typeof(GameObject)),
                hudHolder.transform) as GameObject;

            offscreenIndicatorSidebuffer = offscreenIndicatorSidebufferPC;
        }
        else if (buildPlatform == 1)
        {
            Debug.Log("UIManager: BuildPlatform set to Android");
            hudLeftPanel = Instantiate(Resources.Load("UI/HUD/HUDLeftPanel", typeof(GameObject)),
                hudHolder.transform) as GameObject;

            hudRightPanel = Instantiate(Resources.Load("UI/HUD/HUDRightPanel", typeof(GameObject)),
                hudHolder.transform) as GameObject;

            hudTopPanel = Instantiate(Resources.Load("UI/HUD/HUDTopPanel", typeof(GameObject)),
                hudHolder.transform) as GameObject;

            hudLeftSlotTop = hudLeftPanel.GetComponentInChildren<UISlotTopTag>(true).transform;
            hudLeftSlotMid = hudLeftPanel.GetComponentInChildren<UISlotMidTag>(true).transform;
            hudLeftSlotBot = hudLeftPanel.GetComponentInChildren<UISlotBotTag>(true).transform;
            hudRightSlotTop = hudRightPanel.GetComponentInChildren<UISlotTopTag>(true).transform;
            hudRightSlotMid = hudRightPanel.GetComponentInChildren<UISlotMidTag>(true).transform;
            hudRightSlotBot = hudRightPanel.GetComponentInChildren<UISlotBotTag>(true).transform;

            offscreenIndicatorSidebuffer = offscreenIndicatorSidebufferAndroid;
        }
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
        networkFunctionalityDisabled = lib.networkingVariables.networkFunctionalityDisabled;
        maxNumberOfPlayersInLobby = lib.networkingVariables.maxNumberOfPlayerInOnlineMatch;

        connectedToNetwork = em.BroadcastRequestNetworkConnectionStatus();


        numberOfPlayersInLobby = 0;
        numberOfLobbyParticipantsReady = 0;
    }
    #endregion

    #region OnEnable & OnDisable
    private void OnEnable()
    {
        em.OnMatchStartTimerValueChange += OnMatchStartTimerValueChange;
        em.OnGameRestart += OnGameRestart;
        em.OnNewSceneLoading += OnNewSceneLoading;
        em.OnNewSceneLoaded += OnNewSceneLoaded;
        em.OnEscapeButtonDown += OnEscapeButtonDown;
        em.OnSetGameMode += OnSetGameMode;
        em.OnMatchStarted += OnMatchStarted;
        em.OnMatchEnded += OnMatchEnded;
        em.OnShipReference += OnShipReference;
        em.OnMatchTimerValueChange += OnMatchTimerValueChange;
        em.OnConnectingToNetworkSucceeded += OnConnectingToNetworkSucceeded;
        em.OnConnectionToNetworkLost += OnConnectionToNetworkLost;
        em.OnClientCountInLobbyChange += OnClientCountInLobbyChange;
        em.OnReadyCountInLobbyChange += OnReadyCountInLobbyChange;
        em.OnRequestUINotification += OnRequestUINotification;
        em.OnLobbyEnterSuccessful += OnLobbyEnterSuccessful;
        em.OnLobbyEnterDenied += OnLobbyEnterDenied;
        //em.OnConnectingToNetworkFailed += OnConnectingToNetworkFailed;
        em.OnRequestLoadingIconOn += OnRequestLoadingIconOn;
        em.OnRequestLoadingIconOff += OnRequestLoadingIconOff;
        em.OnNetworkMultiplayerStartMatchStartTimer += OnNetworkMultiplayerStartMatchStartTimer;
        em.OnMatchEndedByServer += OnMatchEndedByServer;
        em.OnReturnToLobbyFromMatch += OnReturnToLobbyFromMatch;
    }

    private void OnDisable()
    {
        em.OnMatchStartTimerValueChange -= OnMatchStartTimerValueChange;
        em.OnGameRestart -= OnGameRestart;
        em.OnNewSceneLoading -= OnNewSceneLoading;
        em.OnNewSceneLoaded -= OnNewSceneLoaded;
        em.OnEscapeButtonDown -= OnEscapeButtonDown;
        em.OnSetGameMode -= OnSetGameMode;
        em.OnMatchStarted -= OnMatchStarted;
        em.OnMatchEnded -= OnMatchEnded;
        em.OnShipReference -= OnShipReference;
        em.OnMatchTimerValueChange -= OnMatchTimerValueChange;
        em.OnConnectingToNetworkSucceeded -= OnConnectingToNetworkSucceeded;
        em.OnConnectionToNetworkLost -= OnConnectionToNetworkLost;
        em.OnClientCountInLobbyChange -= OnClientCountInLobbyChange;
        em.OnReadyCountInLobbyChange -= OnReadyCountInLobbyChange;
        em.OnRequestUINotification -= OnRequestUINotification;
        em.OnLobbyEnterSuccessful -= OnLobbyEnterSuccessful;
        em.OnLobbyEnterDenied -= OnLobbyEnterDenied;
        //em.OnConnectingToNetworkFailed -= OnConnectingToNetworkFailed;
        em.OnRequestLoadingIconOn -= OnRequestLoadingIconOn;
        em.OnRequestLoadingIconOff -= OnRequestLoadingIconOff;
        em.OnNetworkMultiplayerStartMatchStartTimer -= OnNetworkMultiplayerStartMatchStartTimer;
        em.OnMatchEndedByServer -= OnMatchEndedByServer;
        em.OnReturnToLobbyFromMatch -= OnReturnToLobbyFromMatch;
    }
    #endregion

    #region Subscribers
    #region Network event subscribers
    private void OnReturnToLobbyFromMatch()
    {
        if(uiState != UIState.MAINMENUONLINELOBBY)
        {
            if(uiState == UIState.MAINMENUDEFAULT)
            {
                OpenMainMenuOnlineLobbyView();
            }
            else
            {
                returningToLobby = true;
            }
        }
    }

    private void OnNetworkMultiplayerStartMatchStartTimer()
    {
        Debug.LogWarning("UIManager: OnNetworkMultiplayerStartMatchStartTimer");
        CloseLoadingIcon();
        StartFadeFromLoadingScreen();
    }

    private void OnConnectingToNetworkSucceeded(string ip)
    {
        Debug.Log("OnConnectingToNetworkSucceeded");
        connectedToNetwork = true;

        if (mainMenuConnectButton != null)
        {
            mainMenuConnectButton.transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    private void OnConnectingToNetworkFailed(string ip)
    {
        //TODO: Remove if deemed permanently obsolete
    }

    private void OnConnectionToNetworkLost()
    {
        connectedToNetwork = false;

        if (mainMenuConnectButton != null)
        {
            mainMenuConnectButton.transform.GetChild(1).gameObject.SetActive(true);
        }

        if (uiState == UIState.MAINMENUONLINELOBBY)
        {
            CloseMainMenuOnlineLobbyView();
            OpenMainMenuGameModeView();
        }
    }

    private void OnClientCountInLobbyChange(int newCount)
    {
        Debug.Log("OnClientCountInLobbyChange");
        numberOfPlayersInLobby = newCount;
        UpdateParticipantCountDisplay();
    }

    private void OnReadyCountInLobbyChange(int newCount)
    {
        Debug.Log("OnReadyCountInLobbyChange");
        numberOfLobbyParticipantsReady = newCount;
        UpdateParticipantCountDisplay();
    }

    private void OnLobbyEnterSuccessful()
    {
        Debug.Log("OnLobbyEnterSuccessful: Opening online lobby, Game mode changed to Network Multiplayer");
        em.BroadcastSetGameMode(gameModeNetworkMultiplayerIndex);
        CloseMainMenuDefaultView();
        CloseMainMenuSettingsView();
        CloseMainMenuGameModeView();
        OpenMainMenuOnlineLobbyView();
        CloseLoadingIcon();
    }

    private void OnLobbyEnterDenied()
    {
        Debug.Log("OnLobbyEnterDenied: Closing loading icon");
        CloseLoadingIcon();
    }

    private void OnMatchEndedByServer(string winnerName, bool localPlayerWins)
    {
        if (localPlayerWins)
        {
            pauseMenuText.GetComponentInChildren<Text>().text = "Victory!";
        }
        else
        {
            pauseMenuText.GetComponentInChildren<Text>().text = "Defeat! " + winnerName + " wins.";
        }

        OpenGameEndMenu();
    }
    #endregion

    #region GameEvent subscribers
    private void OnRequestLoadingIconOn()
    {
        OpenLoadingIcon();
    }

    private void OnRequestLoadingIconOff()
    {
        CloseLoadingIcon();
    }

    private void OnMatchStartTimerValueChange(int currentTimerValue)
    {
        UpdateMatchStartTimer(currentTimerValue);
    }

    private void OnGameRestart()
    {
        //Reset neccessary values
        matchStarted = false;
        OpenLoadingScreen();
        CloseInGameUI();
        OpenInGameUI();
        StartFadeFromLoadingScreen();
    }

    private void OnNewSceneLoading(int sceneIndex)
    {
        //TODO: Add checks for all future scenes
        if (sceneIndex == sceneIndexMainMenu)
        {
            OpenLoadingScreen();
            OpenLoadingIcon();
            matchStarted = false;
        }
        else if (sceneIndex == sceneIndexLevel01)
        {
            OpenLoadingScreen();
            OpenLoadingIcon();
        }
    }

    private void OnNewSceneLoaded(int sceneIndex)
    {
        //TODO: Add checks for all future scenes
        if (sceneIndex == sceneIndexMainMenu)
        {
            CloseInGameUI();
            OpenMainMenuUI();
            
            if (returningToLobby)
            {
                CloseMainMenuDefaultView();
                OpenMainMenuOnlineLobbyView();
            }

            CloseLoadingIcon();
            CloseLoadingScreen();
        }
        else if (sceneIndex == sceneIndexLevel01
            && em.BroadcastRequestCurrentGameModeIndex() == gameModeSingleplayerIndex)
        {
            OpenLoadingScreen();
            CloseMainMenuUI();
            OpenInGameUI();
            CloseLoadingIcon();
            StartFadeFromLoadingScreen();
        }
        else if (sceneIndex == sceneIndexLevel01
            && em.BroadcastRequestCurrentGameModeIndex() == gameModeNetworkMultiplayerIndex)
        {
            OpenLoadingScreen();
            CloseMainMenuUI();
            OpenInGameUI();

            //CloseLoadingIcon();
            //CloseLoadingScreen();
        }
    }

    private void OnSetGameMode(int newGameModeIndex)
    {
        currentGameModeIndex = newGameModeIndex;
    }

    private void OnMatchStarted()
    {
        matchStarted = true;
    }

    private void OnMatchEnded(int newWinnerIndex, float matchLength)
    {
        matchStarted = false;
        ResetOffscreenTargetFollowing();
        DestroyOffscreenIndicators();

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
        else if (currentGameModeIndex == gameModeLocalMultiplayerIndex)
        {
            // TODO: Check who is the last ship alive and change text accordingly
            pauseMenuText.GetComponentInChildren<Text>().text = "LocMP GameEnd";
            OpenGameEndMenu();
        }
    }

    private void OnShipReference(GameObject newShip)
    {
        if (newShip.GetComponent<LocalPlayerController>() == null)
        {
            offscreenIndicatorTargets.Add(newShip.transform);
        }
    }

    private void OnMatchTimerValueChange(float newValue)
    {
        matchTimer = newValue;

        int minutes = Mathf.FloorToInt(matchTimer / 60f);
        int seconds = Mathf.FloorToInt(matchTimer - minutes * 60);
        int milliseconds = Mathf.FloorToInt((matchTimer - seconds - minutes * 60) * 100);

        if (minutes > 99)
        {
            minutes = 99;
        }

        string t = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);

        if (hudTimerText != null)
            hudTimerText.text = t.ToString();
    }

    private void OnRequestUINotification(string notificationContent)
    {
        notificationQueue.Add(notificationContent);
        OpenUINotification();
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
        else if (buildPlatform == 1 && uiState == UIState.MAINMENUDEFAULT)
        {
            em.BroadcastRequestApplicationExit();
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
        if (matchStarted)
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

    private void OpenUINotification()
    {
        if (notificationQueue.Count > 0)
        {
            if (uiNotification == null)
            {
                uiNotification = Instantiate(Resources.Load("UI/UINotification", typeof(GameObject)),
                    canvas.transform) as GameObject;
                uiNotification.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = notificationQueue[0];
                uiNotification.transform.GetChild(1).GetChild(1).GetComponent<Button>().onClick.AddListener(CloseUINotification);
            }
        }
        else
        {
            Debug.LogError("Notification called but no notification content in queue!");
        }
    }

    private void CloseUINotification()
    {
        notificationQueue.RemoveAt(0);

        if (notificationQueue.Count > 0)
        {
            uiNotification.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = notificationQueue[0];
        }
        else
        {
            uiNotification.transform.GetChild(1).GetChild(1).GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(uiNotification);
            uiNotification = null;
        }
    }

    private void OpenLoadingIcon()
    {
        loadingIcon.SetActive(true);
        loading = true;
    }

    private void CloseLoadingIcon()
    {
        loadingIcon.SetActive(false);
        loading = false;
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
        CloseMainMenuOnlineLobbyView();
        mainMenuHolder.SetActive(false);
    }

    private void OpenMainMenuDefaultView()
    {
        uiState = UIState.MAINMENUDEFAULT;

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

        if (!networkFunctionalityDisabled)
        {
            mainMenuConnectButton = Instantiate(Resources.Load("UI/MainMenu/MainMenuConnectButton", typeof(GameObject)),
            mainMenuLeftSlotTop) as GameObject;
            mainMenuConnectButton.GetComponent<Button>().onClick.AddListener(OnMainMenuConnectButtonPressed);

            if (connectedToNetwork)
            {
                mainMenuConnectButton.transform.GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                mainMenuConnectButton.transform.GetChild(1).gameObject.SetActive(true);
            }
        }
    }

    private void CloseMainMenuDefaultView()
    {
        if (mainMenuPlayButton != null)
        {
            mainMenuPlayButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(mainMenuPlayButton);
        }

        if (mainMenuExitButton != null)
        {
            mainMenuExitButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(mainMenuExitButton);
        }

        if (mainMenuSettingsButton != null)
        {
            mainMenuSettingsButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(mainMenuSettingsButton);
        }

        if (mainMenuConnectButton != null)
        {
            mainMenuConnectButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(mainMenuConnectButton);
        }
    }

    private void OpenMainMenuSettingsView()
    {
        uiState = UIState.MAINMENUSETTINGS;

        //InvertedHUD button
        settingsInvertedHUDButtonHolder = Instantiate(Resources.Load("UI/MainMenu/MainMenuButtonWithToggleMark", typeof(GameObject)),
                mainMenuCenter.transform) as GameObject;
        settingsInvertedHUDButtonHolder.GetComponentInChildren<Text>().text = "INVERTED HUD";
        settingsInvertHUDToggleMarkOnImage = settingsInvertedHUDButtonHolder.GetComponentInChildren<ToggleMarkTag>().transform.
            GetChild(1).gameObject;
        if (invertedHUD)
            settingsInvertHUDToggleMarkOnImage.SetActive(true);
        else
            settingsInvertHUDToggleMarkOnImage.SetActive(false);
        settingsInvertedHUDButtonHolder.GetComponentInChildren<Button>(true).onClick.AddListener(OnSettingsInvertHUDButtonPressed);

        mainMenuReturnButton = Instantiate(Resources.Load("UI/MainMenu/MainMenuReturnButton", typeof(GameObject)),
                mainMenuRightSlotBot.transform) as GameObject;
        mainMenuReturnButton.GetComponent<Button>().onClick.AddListener(OnMainMenuReturnButtonPressed);
    }

    private void CloseMainMenuSettingsView()
    {
        if (settingsInvertedHUDButtonHolder != null)
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
        uiState = UIState.MAINMENUGAMEMODE;

        gameModeSinglePlayerButton = Instantiate(Resources.Load("UI/MainMenu/MainMenuButton", typeof(GameObject)),
                mainMenuCenter.transform) as GameObject;
        gameModeSinglePlayerButton.GetComponentInChildren<Text>().text = "SINGLEPLAYER";
        gameModeSinglePlayerButton.GetComponent<Button>().onClick.AddListener(OnGameModeSinglePlayerButtonPressed);

        gameModeNetworkMultiplayerButton = Instantiate(Resources.Load("UI/MainMenu/MainMenuButton", typeof(GameObject)),
                mainMenuCenter.transform) as GameObject;
        gameModeNetworkMultiplayerButton.GetComponentInChildren<Text>().text = "ONLINE MULTIPLAYER";
        gameModeNetworkMultiplayerButton.GetComponent<Button>().onClick.AddListener(OnGameModeNetworkMultiplayerButtonPressed);

        gameModeLocalMultiplayerButton = Instantiate(Resources.Load("UI/MainMenu/MainMenuButton", typeof(GameObject)),
                mainMenuCenter.transform) as GameObject;
        gameModeLocalMultiplayerButton.GetComponentInChildren<Text>().text = "LOCAL MULTIPLAYER";
        gameModeLocalMultiplayerButton.GetComponent<Button>().onClick.AddListener(OnGameModeLocalMultiplayerButtonPressed);

        mainMenuReturnButton = Instantiate(Resources.Load("UI/MainMenu/MainMenuReturnButton", typeof(GameObject)),
                mainMenuRightSlotBot.transform) as GameObject;
        mainMenuReturnButton.GetComponent<Button>().onClick.AddListener(OnMainMenuReturnButtonPressed);

        if (!networkFunctionalityDisabled)
        {
            mainMenuConnectButton = Instantiate(Resources.Load("UI/MainMenu/MainMenuConnectButton", typeof(GameObject)),
            mainMenuLeftSlotTop) as GameObject;
            mainMenuConnectButton.GetComponent<Button>().onClick.AddListener(OnMainMenuConnectButtonPressed);

            if (connectedToNetwork)
            {
                mainMenuConnectButton.transform.GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                mainMenuConnectButton.transform.GetChild(1).gameObject.SetActive(true);
            }
        }
    }

    private void CloseMainMenuGameModeView()
    {
        if (gameModeSinglePlayerButton != null)
        {
            gameModeSinglePlayerButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(gameModeSinglePlayerButton);
        }

        if (gameModeNetworkMultiplayerButton != null)
        {
            gameModeNetworkMultiplayerButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(gameModeNetworkMultiplayerButton);
        }

        if (gameModeLocalMultiplayerButton != null)
        {
            gameModeLocalMultiplayerButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(gameModeLocalMultiplayerButton);
        }

        if (mainMenuReturnButton != null)
        {
            mainMenuReturnButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(mainMenuReturnButton);
        }

        if (mainMenuConnectButton != null)
        {
            mainMenuConnectButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(mainMenuConnectButton);
        }
    }

    private void OpenMainMenuOnlineLobbyView()
    {
        uiState = UIState.MAINMENUONLINELOBBY;

        numberOfPlayersInLobby = Mathf.Clamp(numberOfPlayersInLobby, 1, maxNumberOfPlayersInLobby);
        numberOfLobbyParticipantsReady = Mathf.Clamp(numberOfLobbyParticipantsReady, 0, numberOfPlayersInLobby);

        lobbyParticipantCountDisplay = Instantiate(Resources.Load("UI/MainMenu/MainMenuInfoDisplay", typeof(GameObject)),
                mainMenuCenter.transform) as GameObject;
        lobbyParticipantCountDisplay.GetComponentInChildren<Text>().text = "PARTICIPANTS READY: "
            + numberOfLobbyParticipantsReady + "/" + numberOfPlayersInLobby;

        lobbyReadyButtonHolder = Instantiate(Resources.Load("UI/MainMenu/MainMenuButtonWithToggleMark", typeof(GameObject)),
                mainMenuCenter.transform) as GameObject;
        lobbyReadyButtonHolder.GetComponentInChildren<Text>().text = "READY";
        lobbyReadyToggleMarkOnImage = lobbyReadyButtonHolder.GetComponentInChildren<ToggleMarkTag>().transform.
            GetChild(1).gameObject;
        if (lobbyReadyButtonPressed)
            lobbyReadyToggleMarkOnImage.SetActive(true);
        else
            lobbyReadyToggleMarkOnImage.SetActive(false);
        lobbyReadyButtonHolder.GetComponentInChildren<Button>(true).onClick.AddListener(OnLobbyReadyButtonPressed);

        mainMenuReturnButton = Instantiate(Resources.Load("UI/MainMenu/MainMenuReturnButton", typeof(GameObject)),
                mainMenuRightSlotBot.transform) as GameObject;
        mainMenuReturnButton.GetComponent<Button>().onClick.AddListener(OnMainMenuReturnButtonPressed);
    }

    private void UpdateParticipantCountDisplay()
    {
        //numberOfPlayersInLobby = Mathf.Clamp(numberOfPlayersInLobby, 1, maxNumberOfPlayersInLobby);
        //numberOfLobbyParticipantsReady = Mathf.Clamp(numberOfLobbyParticipantsReady, 0, numberOfPlayersInLobby);

        if (lobbyParticipantCountDisplay != null)
        {
            lobbyParticipantCountDisplay.GetComponentInChildren<Text>().text = "PARTICIPANTS READY: "
                + numberOfLobbyParticipantsReady + "/" + numberOfPlayersInLobby;
        }
        Debug.Log("LobbyParticipantCountDisplay updated");
    }

    private void CloseMainMenuOnlineLobbyView()
    {
        if (lobbyParticipantCountDisplay != null)
        {
            Destroy(lobbyParticipantCountDisplay);
        }

        if (lobbyReadyButtonHolder != null)
        {
            if (lobbyReadyButtonHolder.GetComponent<Button>())
            {
                lobbyReadyButtonHolder.GetComponent<Button>().onClick.RemoveAllListeners();
            }
            Destroy(lobbyReadyButtonHolder);
            lobbyReadyButtonPressed = false;
        }

        CloseLobbyStartMatchButton();

        if (mainMenuReturnButton != null)
        {
            mainMenuReturnButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(mainMenuReturnButton);
        }
    }

    private void OpenLobbyStartMatchButton()
    {
        if (uiState == UIState.MAINMENUONLINELOBBY)
        {
            lobbyStartMatchButton = Instantiate(Resources.Load("UI/MainMenu/MainMenuButton", typeof(GameObject)),
                    mainMenuCenter.transform) as GameObject;
            lobbyStartMatchButton.GetComponentInChildren<Text>().text = "START MATCH";
            lobbyStartMatchButton.GetComponent<Button>().onClick.AddListener(OnLobbyStartMatchButtonPressed);
        }
        else
        {
            Debug.LogError("Cannot spawn LobbyStartMatchButton if not in lobby!");
        }
    }

    private void CloseLobbyStartMatchButton()
    {
        if (lobbyStartMatchButton != null)
        {
            lobbyStartMatchButton.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(lobbyStartMatchButton);
        }
    }
    #endregion

    #region Button functions
    private void OnMainMenuPlayButtonPressed()
    {
        if (!loading)
        {
            CloseMainMenuDefaultView();
            OpenMainMenuGameModeView();
        }
    }

    private void OnMainMenuExitButtonPressed()
    {
        em.BroadcastRequestApplicationExit();
    }

    private void OnMainMenuSettingsButtonPressed()
    {
        if (!loading)
        {
            CloseMainMenuDefaultView();
            OpenMainMenuSettingsView();
        }
    }

    private void OnMainMenuConnectButtonPressed()
    {
        if (!loading)
        {
            if (!connectedToNetwork)
            {
                string serverIPAddress = em.BroadcastRequestServerIPAddress();
                Debug.Log("UIManager, OnMainMenuConnectButtonPressed serverIPAddress: " + serverIPAddress);
                em.BroadcastRequestConnectToNetwork(serverIPAddress);
            }
            else
            {
                Debug.Log("connected to network, requesting disconnect");
                em.BroadcastRequestDisconnectFromNetwork();
            }
        }
    }

    private void OnSettingsInvertHUDButtonPressed()
    {
        if (!loading)
        {
            //TODO: Broadcast invertedHUD change and save it to GVL
            invertedHUD = !invertedHUD;

            if (invertedHUD)
                settingsInvertHUDToggleMarkOnImage.SetActive(true);
            else
                settingsInvertHUDToggleMarkOnImage.SetActive(false);
        }
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
        else if (uiState == UIState.MAINMENUONLINELOBBY)
        {
            em.BroadcastRequestLobbyExit();
            CloseMainMenuOnlineLobbyView();
            OpenMainMenuGameModeView();
        }
    }

    private void OnGameModeSinglePlayerButtonPressed()
    {
        if (!loading)
        {
            em.BroadcastSetGameMode(gameModeSingleplayerIndex);
            Debug.Log("Game mode set to SinglePlayer");
            OpenLoadingScreen();
            em.BroadcastRequestSceneSingleLevel01();
        }
    }

    private void OnGameModeNetworkMultiplayerButtonPressed()
    {
        if (!loading)
        {
            if (!networkFunctionalityDisabled)
            {
                if (connectedToNetwork)
                {
                    Debug.Log("Connected to network: Requesting lobby access");
                    em.BroadcastRequestLobbyEnter();
                    OpenLoadingIcon();

                    //em.BroadcastSetGameMode(gameModeNetworkMultiplayerIndex);
                    //CloseMainMenuGameModeView();
                    //OpenMainMenuOnlineLobbyView();
                }
                else
                {
                    Debug.Log("Not connected to network: Cannot open online lobby");
                }
            }
            else
            {
                em.BroadcastSetGameMode(gameModeNetworkMultiplayerIndex);
                Debug.Log("Game mode set to Network Multiplayer(networkFunctionality disabled)");
            }
        }
    }

    private void OnLobbyReadyButtonPressed()
    {
        if (!loading)
        {
            lobbyReadyButtonPressed = !lobbyReadyButtonPressed;
            if (lobbyReadyButtonPressed)
            {
                lobbyReadyToggleMarkOnImage.SetActive(true);
                //numberOfLobbyParticipantsReady++;
                UpdateParticipantCountDisplay();
                OpenLobbyStartMatchButton();
            }
            else
            {
                lobbyReadyToggleMarkOnImage.SetActive(false);
                //numberOfLobbyParticipantsReady--;
                UpdateParticipantCountDisplay();
                CloseLobbyStartMatchButton();
            }

            em.BroadcastLobbyReadyStateChange(lobbyReadyButtonPressed);
        }
    }

    private void OnLobbyStartMatchButtonPressed()
    {
        if (!loading)
        {
            if (!networkFunctionalityDisabled)
            {
                if (connectedToNetwork)
                {
                    Debug.Log("Connected to network: Starting Network Multiplayer game");
                    //OpenLoadingScreen();
                    //em.BroadcastRequestSceneSingleLevel01();
                    em.BroadcastRequestOnlineMatchStart();
                }
                else
                {
                    Debug.Log("Not connected to network: Cannot start Network Multiplayer game");
                }
            }
            else
            {
                Debug.Log("NetworkFunctionality disabled: Cannost start Network Multiplayer game");
            }
        }
    }

    private void OnGameModeLocalMultiplayerButtonPressed()
    {
        if (!loading)
        {
            em.BroadcastSetGameMode(gameModeLocalMultiplayerIndex);
            Debug.Log("Game mode changed to Local Multiplayer");
            //OpenLoadingScreen();
            //em.BroadcastRequestSceneSingleLevel01();
        }
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
                hudVirtualJoystickOne.GetComponent<VirtualJoystick>().SetIndex(1);
                //Shooting joystick
                hudVirtualJoystickTwo = Instantiate(Resources.Load("UI/HUD/HUDVirtualJoystickHolder", typeof(GameObject)),
                    hudRightSlotBot.position, hudRightSlotBot.rotation, hudRightSlotBot) as GameObject;
                hudVirtualJoystickTwo.GetComponent<VirtualJoystick>().SetIndex(2);
            }
            else
            {
                //Pause button
                hudOpenPauseMenuButton = Instantiate(Resources.Load("UI/HUD/HUDOpenPauseMenuButton", typeof(GameObject)),
                    hudLeftSlotTop.position, hudLeftSlotTop.rotation, hudLeftSlotTop) as GameObject;
                //Movement joystick
                hudVirtualJoystickOne = Instantiate(Resources.Load("UI/HUD/HUDVirtualJoystickHolder", typeof(GameObject)),
                    hudRightSlotBot.position, hudRightSlotBot.rotation, hudRightSlotBot) as GameObject;
                hudVirtualJoystickOne.GetComponent<VirtualJoystick>().SetIndex(1);
                //Shooting joystick
                hudVirtualJoystickTwo = Instantiate(Resources.Load("UI/HUD/HUDVirtualJoystickHolder", typeof(GameObject)),
                    hudLeftSlotBot.position, hudLeftSlotBot.rotation, hudLeftSlotBot) as GameObject;
                hudVirtualJoystickTwo.GetComponent<VirtualJoystick>().SetIndex(2);
            }

            hudOpenPauseMenuButton.GetComponent<Button>().onClick.AddListener(HUDOpenPauseMenuButtonPressed);
        }

        hudTimer = Instantiate(Resources.Load("UI/HUD/HUDTimer", typeof(GameObject)),
            hudTopPanel.transform) as GameObject;
        hudTimerText = hudTimer.GetComponentInChildren<Text>();

        pauseMenuHolder = Instantiate(Resources.Load("UI/HUD/PauseMenu/PauseMenuHolder", typeof(GameObject)),
            inGameUIHolder.transform) as GameObject;
        pauseMenuPanel = pauseMenuHolder.transform.GetChild(0);
        pauseMenuText = Instantiate(Resources.Load("UI/HUD/PauseMenu/PauseMenuText", typeof(GameObject)),
                pauseMenuPanel.transform) as GameObject;

        inGameUIHolder.SetActive(true);
        offscreenIndicatorHolder.gameObject.SetActive(true);

        ClosePauseMenu();
        CloseGameEndMenu();

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
        if (hudTimer != null)
        {
            Destroy(hudTimer);
        }

        CloseGameEndMenu();
        ClosePauseMenu();

        if (pauseMenuHolder != null)
        {
            Destroy(pauseMenuHolder);
        }
        inGameUIHolder.SetActive(false);
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

        if (pauseMenuHolder != null)
        {
            pauseMenuHolder.SetActive(false);
        }
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

        if (pauseMenuHolder != null)
        {
            pauseMenuHolder.SetActive(false);
        }
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
        if (!loading)
        {
            ClosePauseMenu();
            Debug.Log("Closing pause menu");
        }
    }

    private void PauseMenuRestartButtonPressed()
    {
        if (!loading)
        {
            if(currentGameModeIndex != gameModeNetworkMultiplayerIndex)
            {
                OpenLoadingScreen();
                ClosePauseMenu();
                ResetOffscreenTargetFollowing();
                DestroyOffscreenIndicators();
                em.BroadcastGameRestart();
            }
            else
            {
                OpenLoadingScreen();
                ClosePauseMenu();
                ResetOffscreenTargetFollowing();
                DestroyOffscreenIndicators();
                em.BroadcastRequestRestartFromServer();
            }
        }
    }

    private void PauseMenuMainMenuButtonPressed()
    {
        if(currentGameModeIndex != gameModeNetworkMultiplayerIndex)
        {
            OpenLoadingScreen();
            ClosePauseMenu();
            ResetOffscreenTargetFollowing();
            DestroyOffscreenIndicators();
            em.BroadcastRequestSceneSingleMainMenu();
        }
        else
        {
            //TODO: Broadcast lobby exit to the server
            Debug.Log("Exiting to lobby");
            em.BroadcastExitNetworkMultiplayerMidGame();
        }
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

    #region MatchStartTimer & Loading screen fade
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
        if (offscreenIndicatorTargets.Count > 0)
        {
            for (int i = 0; i < offscreenIndicatorTargets.Count; i++)
            {
                if (offscreenIndicatorTargets[i] == null)
                {
                    offscreenIndicatorTargets.RemoveAt(i);
                    //Destroy obsolete indicators
                    if (offscreenIndicatorPool.Count >= offscreenIndicatorTargets.Count)
                    {
                        if (offscreenIndicatorPool[offscreenIndicatorTargets.Count].gameObject != null)
                        {
                            Destroy(offscreenIndicatorPool[offscreenIndicatorTargets.Count].gameObject);
                        }
                        offscreenIndicatorPool.RemoveAt(offscreenIndicatorTargets.Count);
                    }
                    i--;
                }
                else
                {
                    //If not enough indicators, create a new one
                    if (offscreenIndicatorTargets.Count > offscreenIndicatorPool.Count)
                    {
                        GameObject newOffscreenIndicator = Instantiate(Resources.Load("UI/HUD/HUDOffscreenIndicator", typeof(GameObject)),
                             offscreenIndicatorHolder.position, offscreenIndicatorHolder.rotation, offscreenIndicatorHolder) as GameObject;
                        Transform newOffscreenIndicatorTransform = newOffscreenIndicator.transform;
                        offscreenIndicatorDefaultPosition = newOffscreenIndicatorTransform.position;
                        offscreenIndicatorPool.Add(newOffscreenIndicatorTransform);
                        newOffscreenIndicator.SetActive(false);
                    }

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
                        if (!indicator.gameObject.activeSelf)
                        {
                            indicator.gameObject.SetActive(true);
                        }
                        Color indicatorColor = target.GetComponent<ShipController>().GetShipColor();
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

                        #region Indicator modifications based on target distance
                        //Calculating a factor depending on target distance, to modify indicators accordingly
                        Vector3 screenCenterInWorldSpace = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
                        Vector3 cameraPositionOnArena = new Vector3(screenCenterInWorldSpace.x,
                            screenCenterInWorldSpace.y - 30, screenCenterInWorldSpace.z);
                        Debug.DrawRay(cameraPositionOnArena, Vector3.up);

                        float indicatorTargetDistance = Vector3.Distance(cameraPositionOnArena, target.position);

                        float maxIndicatorTargetDistance = 60;
                        float minIndicatorTargetDistance = 20;
                        float minIndicatorDistanceFactorValue = 0.2f;
                        float maxIndicatorDistanceFactorValue = 1;

                        indicatorTargetDistance = Mathf.Clamp(indicatorTargetDistance, minIndicatorTargetDistance,
                            maxIndicatorTargetDistance);
                        indicatorTargetDistance -= minIndicatorTargetDistance;

                        float indicatorTargetDistanceFactor = indicatorTargetDistance /
                            (maxIndicatorTargetDistance - minIndicatorTargetDistance);

                        indicatorTargetDistanceFactor = Mathf.Clamp((1 - indicatorTargetDistanceFactor),
                            minIndicatorDistanceFactorValue, maxIndicatorDistanceFactorValue);

                        //Horizontal size scaling depending on target distance
                        RectTransform indicatorRectTransform = indicator.GetChild(0).GetComponent<RectTransform>();
                        indicatorRectTransform.localScale = new Vector3(indicatorTargetDistanceFactor,
                            1, 1);

                        ////Color fade depending on target distance
                        //Image indicatorImage = indicator.GetChild(0).GetComponent<Image>();
                        //Color newColor = indicatorImage.color;
                        //newColor.a = indicatorTargetDistanceFactor;
                        //indicatorImage.color = newColor;
                        #endregion
                    }

                }
            }
        }
    }

    private void ResetOffscreenTargetFollowing()
    {
        offscreenIndicatorTargets.Clear();
        foreach (Transform indicator in offscreenIndicatorPool)
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
