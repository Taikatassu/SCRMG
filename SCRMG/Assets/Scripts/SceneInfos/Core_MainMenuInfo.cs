using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Core_MainMenuInfo : MonoBehaviour
{
    //TODO: Send UI element info to UI_Manager and have UI_Manager handle all UI functionality
    Core_Toolbox toolbox;
    Core_EventManager em;
    Button playButton;

	void Awake ()
    {
        toolbox = FindObjectOfType<Core_Toolbox>();
        em = toolbox.GetComponent<Core_EventManager>();
        playButton = transform.GetComponentInChildren<Core_MainMenuPlayButtonTag>().GetComponent<Button>();
	}

    private void Start()
    {
        playButton.onClick.AddListener(OnPlayButtonPressed);
    }

    private void OnPlayButtonPressed()
    {
        Debug.Log("PlayButton press detected");
        em.BroadcastRequestSceneSingleLevel01();
    }
}
