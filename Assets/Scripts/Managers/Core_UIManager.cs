using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Core_UIManager : MonoBehaviour {

    /* TODO: Implement a system where UIManager (this script) controlls all of the UI related functions
    *       Level specific UIInfo-script sends information about all UI assets of 
    *           the scene to UIManager when a new scene is loaded
    *       UIManager then handles all the functions of given UI assets
    */

    #region References & variables
    Core_Toolbox toolbox;
    Core_EventManager em;
    Image fullscreenBlackImage;
    Text matchBeginTimerText;
    bool matchBeginTimerVisible = false;
    int matchBeginTimer = 0;
    float fadeFromBlackTime = 2;
    #endregion

    #region Initialization
    #region Awake
    void Awake ()
    {
        toolbox = FindObjectOfType<Core_Toolbox>();
        em = toolbox.GetComponent<Core_EventManager>();
    }
    #endregion

    #region OnEnable & OnDisable
    private void OnEnable()
    {
        em.OnMatchBeginTimerValue += OnMatchBeginTimerValue;
    }

    private void OnDisable()
    {
        em.OnMatchBeginTimerValue -= OnMatchBeginTimerValue;
    }
    #endregion

    #region UI Element setters
    public void SetFullscreenBlackImage(Image newFullscreenBlackImage)
    {
        fullscreenBlackImage = newFullscreenBlackImage;
    }

    public void SetMatchBeginTimerText(Text newMatchBeginTimerText)
    {
        matchBeginTimerText = newMatchBeginTimerText;
    }
    #endregion

    #region Subscribers
    private void OnMatchBeginTimerValue(int currentTimerValue)
    {
        UpdateMatchTimer(currentTimerValue);
    }
    #endregion
    #endregion

    #region UI functions
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

}
