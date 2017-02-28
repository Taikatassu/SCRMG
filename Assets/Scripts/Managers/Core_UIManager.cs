using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Core_UIManager : MonoBehaviour {

    /* TODO: Pause menu, Game over menu
     *      "Resume", "Restart" "Return to main menu"
     *      Full gameloop with starting a match, restarting it, returning to main menu, and starting
     *      all over again
    *       
    *  Enemy indicators showing enemy directions at the screen boarders
    */

    #region References & variables
    public static Core_UIManager instance;

    Core_Toolbox toolbox;
    Core_GlobalVariableLibrary lib;
    Core_EventManager em;
    GameObject canvas;

    //MainMenu UI
    GameObject mainMenuHolder;
    Button playButton;
    int sceneIndexMainMenu = -1;
    int sceneIndexLevel01 = -1;

    //InGame UI
    GameObject inGameUIHolder;
    GameObject pauseMenuHolder;
    Image fullscreenBlackImage;
    Text matchBeginTimerText;
    Button pauseMenuResumeButton;
    Button pauseMenuRestartButton;
    Button pauseMenuMainMenuButton;
    bool matchBeginTimerVisible = false;
    int matchBeginTimer = -1;
    //Variables coming from globalVariableLibrary
    float fadeFromBlackTime = -1;
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
        fullscreenBlackImage = inGameUIHolder.GetComponentInChildren<Core_FullscreenBlackImageTag>(true).
            GetComponent<Image>();
        matchBeginTimerText = inGameUIHolder.GetComponentInChildren<Core_MatchBeginTimerTag>(true).
            GetComponent<Text>();
        pauseMenuResumeButton = inGameUIHolder.GetComponentInChildren<Core_PauseMenuResumeButtonTag>().
            GetComponent<Button>();
        pauseMenuRestartButton = inGameUIHolder.GetComponentInChildren<Core_PauseMenuRestartButtonTag>().
           GetComponent<Button>();
        pauseMenuMainMenuButton = inGameUIHolder.GetComponentInChildren<Core_PauseMenuMainMenuButtonTag>().
           GetComponent<Button>();
        #endregion

        //matchBeginTimerText.gameObject.SetActive(false);

        OpenMainMenuUI();
        ClosePauseMenu();
        CloseInGameUI();
    }
    #endregion

    #region GetStats
    private void GetStats()
    {
        sceneIndexMainMenu = lib.sceneVariables.sceneIndexMainMenu;
        sceneIndexLevel01 = lib.sceneVariables.sceneIndexLevel01;
        fadeFromBlackTime = lib.sceneVariables.fadeFromBlackTime;
    }
    #endregion

    // TODO: Add tags for pause menu buttons
    // Add functionality for pause menu buttons

    #region OnEnable & OnDisable
    private void OnEnable()
    {
        em.OnMatchBeginTimerValue += OnMatchBeginTimerValue;
        em.OnGameRestart += OnGameRestart;
        em.OnNewSceneLoading += OnNewSceneLoading;
        em.OnEscapeButtonDown += OnEscapeButtonDown;
    }

    private void OnDisable()
    {
        em.OnMatchBeginTimerValue -= OnMatchBeginTimerValue;
        em.OnGameRestart -= OnGameRestart;
        em.OnNewSceneLoading -= OnNewSceneLoading;
        em.OnEscapeButtonDown -= OnEscapeButtonDown;
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
        Debug.Log("PauseMenuOpened");
        pauseMenuResumeButton.onClick.AddListener(OnPauseMenuResumeButtonPressed);
        pauseMenuRestartButton.onClick.AddListener(OnPauseMenuRestartButtonPressed);
        pauseMenuMainMenuButton.onClick.AddListener(OnPauseMenuMainMenuButtonPressed);
        pauseMenuHolder.SetActive(true);
    }

    private void ClosePauseMenu()
    {
        pauseMenuResumeButton.onClick.RemoveAllListeners();
        pauseMenuRestartButton.onClick.RemoveAllListeners();
        pauseMenuMainMenuButton.onClick.RemoveAllListeners();
        pauseMenuHolder.SetActive(false);
    }

    private void UpdateMatchTimer(int newtimerValue)
    {
        matchBeginTimer = newtimerValue;
        if (!matchBeginTimerVisible)
        {
            //TODO: Set matchBeginTimer visible
            matchBeginTimerText.gameObject.SetActive(true);
            StartCoroutine(FadeFromBlack(fadeFromBlackTime));
        }

        //TODO: Update matchBeginTimer value
        matchBeginTimerText.text = matchBeginTimer.ToString();
        if (matchBeginTimer == 0)
        {
            matchBeginTimerText.gameObject.SetActive(false);
        }
    }

    IEnumerator FadeFromBlack(float fadeTime)
    {
        Color newColor = fullscreenBlackImage.color;
        float originalAlpha = newColor.a;
        for (float i = 0.0f; i < 1.0f; i += Time.deltaTime / fadeTime)
        {
            newColor.a = Mathf.Lerp(originalAlpha, 0, i);
            fullscreenBlackImage.color = newColor;
            yield return new WaitForEndOfFrame();
        }
        fullscreenBlackImage.gameObject.SetActive(false);
    }

    private void OnPauseMenuResumeButtonPressed()
    {
        Debug.Log("Resume button pressed");
        //Close pauseMenu
        ClosePauseMenu();
        //Resume game (if pause implemented and in effect)
    }

    private void OnPauseMenuRestartButtonPressed()
    {
        Debug.Log("Restart button pressed");
        //Close pauseMenu
        ClosePauseMenu();
        //Restart game
        em.BroadcastGameRestart();
    }

    private void OnPauseMenuMainMenuButtonPressed()
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

    #region Subscribers
    private void OnMatchBeginTimerValue(int currentTimerValue)
    {
        UpdateMatchTimer(currentTimerValue);
    }

    private void OnGameRestart()
    {
        //Reset all
        //fullscreenBlackImage.gameObject.SetActive(true);
        //matchBeginTimerText.gameObject.SetActive(false);
    }

    private void OnNewSceneLoading(int sceneIndex)
    {
        //TODO: Change part of 
        //TODO: Add checks for all future scenes
        if (sceneIndex == sceneIndexMainMenu)
        {
            //Close InGameUI
            ClosePauseMenu();
            CloseInGameUI();
            //Open MainMenuUI
            OpenMainMenuUI();
        }
        else if (sceneIndex == sceneIndexLevel01)
        {
            //Close main menu UI
            CloseMainMenuUI();
            //Open InGameUI
            fullscreenBlackImage.gameObject.SetActive(true);
            OpenInGameUI();
            ClosePauseMenu();
        }
    }

    private void OnEscapeButtonDown(int controllerIndex)
    {
        if (inGameUIHolder.activeSelf)
        {
            if (pauseMenuHolder.activeSelf){

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
}
