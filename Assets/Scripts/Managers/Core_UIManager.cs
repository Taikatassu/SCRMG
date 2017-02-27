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
    Core_Toolbox toolbox;
    Core_GlobalVariableLibrary lib;
    Core_EventManager em;
    GameObject canvas;

    //MainMenu UI
    GameObject mainMenuHolder;
    Button playButton;
    int mainMenuIndex = 0;
    int level01Index = 0;

    //InGame UI
    GameObject inGameUIHolder;
    GameObject pauseMenuHolder;
    Image fullscreenBlackImage;
    Text matchBeginTimerText;
    bool matchBeginTimerVisible = false;
    int matchBeginTimer = 0;
    //Variables coming from globalVariableLibrary
    float fadeFromBlackTime = 0;
    #endregion

    #region Initialization
    #region Awake
    void Awake ()
    {
        toolbox = FindObjectOfType<Core_Toolbox>();
        em = toolbox.GetComponent<Core_EventManager>();
        lib = toolbox.GetComponent<Core_GlobalVariableLibrary>();
        GetStats();

        canvas = GameObject.FindWithTag("Canvas");
        mainMenuHolder = canvas.GetComponentInChildren<Core_MainMenuHolderTag>().gameObject;
        inGameUIHolder = canvas.GetComponentInChildren<Core_InGameUIHolderTag>().gameObject;
        pauseMenuHolder = inGameUIHolder.GetComponentInChildren<Core_PauseMenuHolderTag>().gameObject;

        playButton = mainMenuHolder.GetComponentInChildren<Core_MainMenuPlayButtonTag>().
            GetComponent<Button>();
        fullscreenBlackImage = inGameUIHolder.GetComponentInChildren<Core_FullscreenBlackImageTag>().
            GetComponent<Image>();
        matchBeginTimerText = inGameUIHolder.GetComponentInChildren<Core_MatchBeginTimerTag>().
            GetComponent<Text>();
        matchBeginTimerText.gameObject.SetActive(false);

        OpenMainMenuUI();
        ClosePauseMenu();
        CloseInGameUI();
    }
    #endregion

    #region GetStats
    private void GetStats()
    {
        mainMenuIndex = lib.sceneVariables.mainMenuIndex;
        level01Index = lib.sceneVariables.level01Index;
        fadeFromBlackTime = lib.sceneVariables.fadeFromBlackTime;
    }
    #endregion

    #region OnEnable & OnDisable
    private void OnEnable()
    {
        em.OnMatchBeginTimerValue += OnMatchBeginTimerValue;
        em.OnGameRestart += OnGameRestart;
        em.OnLoadingNewScene += OnLoadingNewScene;
    }

    private void OnDisable()
    {
        em.OnMatchBeginTimerValue -= OnMatchBeginTimerValue;
        em.OnGameRestart -= OnGameRestart;
        em.OnLoadingNewScene -= OnLoadingNewScene;
    }
    #endregion

    #region UI Element setters
    //MainMenu UI
    public void SetPlayButton(Button newPlayButton)
    {
        playButton = newPlayButton;
    }

    //InGame UI
    public void SetFullscreenBlackImage(Image newFullscreenBlackImage)
    {
        fullscreenBlackImage = newFullscreenBlackImage;
        fullscreenBlackImage.gameObject.SetActive(true);
    }

    public void SetMatchBeginTimerText(Text newMatchBeginTimerText)
    {
        matchBeginTimerText = newMatchBeginTimerText;
        matchBeginTimerText.gameObject.SetActive(false);
    }
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
        matchBeginTimerText.gameObject.SetActive(false);
    }

    private void OnLoadingNewScene(int sceneIndex)
    {
        Debug.Log("Loading scene with index: " + sceneIndex);
        //TODO: Add checks for all future scenes
        if (sceneIndex == mainMenuIndex)
        {
            //Close InGameUI
            ClosePauseMenu();
            CloseInGameUI();
            //Open MainMenuUI
            OpenMainMenuUI();
        }
        else if (sceneIndex == level01Index)
        {
            //Close main menu UI
            CloseMainMenuUI();
            //Open InGameUI
            OpenInGameUI();
            ClosePauseMenu();
        }
    }
    #endregion
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
        Debug.Log("PlayButton press detected");
        em.BroadcastLoadingNewScene(level01Index);
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
        pauseMenuHolder.SetActive(true);
    }

    private void ClosePauseMenu()
    {
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
    #endregion
    #endregion

}
