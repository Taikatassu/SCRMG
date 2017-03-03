using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    string canvasTag;

    //MainMenu UI
    GameObject mainMenuHolder;
    Button playButton;

    //InGame UI
    GameObject inGameUIHolder;
    GameObject pauseMenuHolder;
    GameObject gameEndMenuHolder;
    GameObject hudHolder;
    Transform offscreenIndicatorHolder;
    Image loadingScreenImage;
    Text matchStartTimerText;
    Text gameEndMenuText;
    Button hudPauseMenuButton;
    Button pauseMenuResumeButton;
    Button pauseMenuRestartButton;
    Button pauseMenuMainMenuButton;
    Button gameEndMenuRestartButton;
    Button gameEndMenuMainMenuButton;
    Color loadingScreenNewColor;
    Color loadingScreenOriginalColor;
    List<Transform> offscreenIndicatorPool = new List<Transform>();
    List<Transform> offscreenIndicatorTargets = new List<Transform>();
    Vector3 offscreenIndicatorDefaultPosition;
    bool isFadingFromLoadingScreen = false;
    bool followingOffscreenTargets = false;
    int matchStartTimerValue = -1;
    int currentGameModeIndex = -1;
    float loadingScreenFadeStartTime = -1;
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
        hudHolder = inGameUIHolder.GetComponentInChildren<Core_HUDHolderTag>(true).gameObject;

        playButton = mainMenuHolder.GetComponentInChildren<Core_MainMenuPlayButtonTag>(true).
            GetComponent<Button>();
        loadingScreenImage = inGameUIHolder.GetComponentInChildren<Core_LoadingScreenImageTag>(true).
            GetComponent<Image>();
        matchStartTimerText = inGameUIHolder.GetComponentInChildren<Core_MatchBeginTimerTag>(true).
            GetComponent<Text>();

        offscreenIndicatorHolder = inGameUIHolder.GetComponentInChildren<Core_OffscreenIndicatorHolderTag>(true).
            transform;

        hudPauseMenuButton = hudHolder.GetComponentInChildren<Core_HUDPauseMenuButtonTag>(true).
            GetComponent<Button>();

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
            Debug.Log("followingOffscreenTargets = true");
            followingOffscreenTargets = true;
        }
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
        ResetOffscreenTargetFollowing();

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

    private void OnShipReference(GameObject newShip)
    {
        if (newShip.GetComponent<Core_LocalPlayerController>() == null)
        {
            offscreenIndicatorTargets.Add(newShip.transform);
            if (offscreenIndicatorTargets.Count > offscreenIndicatorPool.Count)
            {
                GameObject newOffscreenIndicator = Instantiate(Resources.Load("OffscreenIndicator", typeof(GameObject)),
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
                loadingScreenImage.gameObject.SetActive(false);
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
        //OpenHUD();
        offscreenIndicatorHolder.gameObject.SetActive(true);
    }

    private void CloseInGameUI()
    {
        //CloseHUD();
        inGameUIHolder.SetActive(false);
        ResetOffscreenTargetFollowing();
        DestroyOffscreenIndicators();
    }

    private void OpenPauseMenu()
    {
        //TODO: Implement game pausing if in singleplayer
        //CloseHUD();
        pauseMenuResumeButton.onClick.AddListener(PauseMenuResumeButtonPressed);
        pauseMenuRestartButton.onClick.AddListener(PauseMenuRestartButtonPressed);
        pauseMenuMainMenuButton.onClick.AddListener(PauseMenuMainMenuButtonPressed);
        pauseMenuHolder.SetActive(true);
        em.BroadcastPauseOn();
    }

    private void ClosePauseMenu()
    {
        //OpenHUD();
        pauseMenuResumeButton.onClick.RemoveAllListeners();
        pauseMenuRestartButton.onClick.RemoveAllListeners();
        pauseMenuMainMenuButton.onClick.RemoveAllListeners();
        pauseMenuHolder.SetActive(false);
        em.BroadcastPauseOff();
    }

    private void OpenGameEndMenu()
    {
        //CloseHUD();
        ClosePauseMenu();
        gameEndMenuRestartButton.onClick.AddListener(GameEndMenuRestartButtonPressed);
        gameEndMenuMainMenuButton.onClick.AddListener(GameEndMenuMainMenuButtonPressed);
        gameEndMenuHolder.SetActive(true);
        em.BroadcastPauseOn();
    }

    private void CloseGameEndMenu()
    {
        //OpenHUD();
        gameEndMenuText.text = "GAME END MENU";
        gameEndMenuRestartButton.onClick.RemoveAllListeners();
        gameEndMenuMainMenuButton.onClick.RemoveAllListeners();
        gameEndMenuHolder.SetActive(false);
        em.BroadcastPauseOff();
    }

    //TODO: Find out why getting null reference error if using HUD

    //private void OpenHUD()
    //{

    //    hudHolder.SetActive(true);
    //    hudPauseMenuButton.onClick.AddListener(HUDPauseMenuButtonPressed);
    //    hudPauseMenuButton.gameObject.SetActive(true);
    //}

    //private void CloseHUD()
    //{
    //    hudHolder.SetActive(false);
    //    hudPauseMenuButton.gameObject.SetActive(false);
    //    hudPauseMenuButton.onClick.RemoveAllListeners();
    //}
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
        ResetOffscreenTargetFollowing();
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
        ResetOffscreenTargetFollowing();
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

    #region HUD buttons
    private void HUDPauseMenuButtonPressed()
    {
        OpenPauseMenu();
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

                    if (screenPosition.x >= -0.08f && screenPosition.x <= 1.08f && 
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
                        indicator.position = Camera.main.ViewportToScreenPoint(screenPosition);

                    }

                }
            }
        }
    }

    private void ResetOffscreenTargetFollowing()
    {
        Debug.Log("ResetOffscreenTargetFollowing");
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
