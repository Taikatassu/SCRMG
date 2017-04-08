using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

    Toolbox toolbox;
    EventManager em;

    GameObject canvas;
    GameObject clientInfoHolder;
    GameObject leftUIPanel;
    GameObject rightUIPanel;

    List<string> connectedClientIDs = new List<string>();

    #region Initialization
    private void Awake()
    {
        toolbox = FindObjectOfType<Toolbox>();
        em = toolbox.GetComponent<EventManager>();

        canvas = transform.GetComponentInChildren<Canvas>().gameObject;

        clientInfoHolder = canvas.transform.GetChild(0).gameObject;
        leftUIPanel = Instantiate(Resources.Load("UI/LeftPanel", typeof(GameObject)),
            canvas.transform) as GameObject;
        rightUIPanel = Instantiate(Resources.Load("UI/RightPanel", typeof(GameObject)),
            canvas.transform) as GameObject;
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }
    #endregion

    #region Subscribers
    private void OnClientConnected(string newClientID)
    {

    }

    private void OnClientDisconnected(string disconnectedClientID)
    {

    }
    #endregion

}

