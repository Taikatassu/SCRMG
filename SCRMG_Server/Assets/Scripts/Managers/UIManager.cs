﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Server;

public class UIManager : MonoBehaviour {

    Toolbox toolbox;
    EventManager em;

    GameObject canvas;
    GameObject clientInfoHolder;
    GameObject leftUIPanel;
    GameObject rightUIPanel;
    GameObject hudHolder;
    Text matchStartTimerText; //TODO: Set this in Awake
    Text hudTimerText; //TODO: Set this in Awake

    List<GameObject> connectedClientInfos = new List<GameObject>();
    List<GameObject> lobbyClientInfos = new List<GameObject>();

    int matchStartTimerValue = -1;
    float matchTimer = -1;

    #region Initialization
    private void Awake()
    {
        toolbox = FindObjectOfType<Toolbox>();
        em = toolbox.GetComponent<EventManager>();

        canvas = transform.GetComponentInChildren<Canvas>().gameObject;

        clientInfoHolder = canvas.transform.GetChild(0).gameObject;
        leftUIPanel = Instantiate(Resources.Load("UI/LeftPanel", typeof(GameObject)),
            clientInfoHolder.transform) as GameObject;
        rightUIPanel = Instantiate(Resources.Load("UI/RightPanel", typeof(GameObject)),
            clientInfoHolder.transform) as GameObject;
        hudHolder = canvas.transform.GetChild(1).gameObject;
        GameObject matchStartTimer = Instantiate(Resources.Load("UI/MatchStartTimer", typeof(GameObject)),
            hudHolder.transform) as GameObject;
        matchStartTimerText = matchStartTimer.GetComponent<Text>();
        GameObject hudTimer = Instantiate(Resources.Load("UI/MatchTimer", typeof(GameObject)),
            hudHolder.transform) as GameObject;
        hudTimerText = hudTimer.GetComponent<Text>();

        matchStartTimer.SetActive(false);
        hudTimer.SetActive(false);
    }

    private void OnEnable()
    {
        em.OnClientConnected += OnClientConnected;
        em.OnClientDisconnected += OnClientDisconnected;
        em.OnClientEnterLobby += OnClientEnterLobby;
        em.OnClientExitLobby += OnClientExitLobby;
        em.OnMatchStartTimerValueChange += OnMatchStartTimerValueChange;
        em.OnMatchTimerValueChange += OnMatchTimerValueChange;
    }

    private void OnDisable()
    {
        em.OnClientConnected -= OnClientConnected;
        em.OnClientDisconnected -= OnClientDisconnected;
        em.OnClientEnterLobby -= OnClientEnterLobby;
        em.OnClientExitLobby -= OnClientExitLobby;
        em.OnMatchStartTimerValueChange -= OnMatchStartTimerValueChange;
        em.OnMatchTimerValueChange -= OnMatchTimerValueChange;
    }
    #endregion

    #region Subscribers
    private void OnClientConnected(ClientData newClientData)
    {
        GameObject newClientInfo = Instantiate(Resources.Load("UI/ClientInfo", typeof(GameObject)),
            leftUIPanel.transform) as GameObject;
        newClientInfo.transform.GetChild(0).GetComponent<Text>().text = newClientData.id;
        newClientInfo.transform.GetChild(1).GetComponent<Text>().text = newClientData.clientName;

        connectedClientInfos.Add(newClientInfo);
    }

    private void OnClientDisconnected(ClientData disconnectedClientData)
    {
        for (int i = 0; i < connectedClientInfos.Count; i++)
        {
            if (connectedClientInfos[i].transform.GetChild(0).GetComponent<Text>().text == disconnectedClientData.id)
            {
                Destroy(connectedClientInfos[i]);
                connectedClientInfos.RemoveAt(i);
                i--;
            }
        }
    }

    private void OnClientEnterLobby(ClientData newClientData)
    {
        GameObject newClientInfo = Instantiate(Resources.Load("UI/ClientInfo", typeof(GameObject)),
            rightUIPanel.transform) as GameObject;
        newClientInfo.transform.GetChild(0).GetComponent<Text>().text = newClientData.id;
        newClientInfo.transform.GetChild(1).GetComponent<Text>().text = newClientData.clientName;

        lobbyClientInfos.Add(newClientInfo);
    }

    private void OnClientExitLobby(ClientData disconnectedClientData)
    {
        for (int i = 0; i < lobbyClientInfos.Count; i++)
        {
            if (lobbyClientInfos[i].transform.GetChild(0).GetComponent<Text>().text == disconnectedClientData.id)
            {
                Destroy(lobbyClientInfos[i]);
                lobbyClientInfos.RemoveAt(i);
                i--;
            }
        }
    }

    private void OnMatchStartTimerValueChange(int currentTimerValue)
    {
        UpdateMatchStartTimer(currentTimerValue);
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
        {
            hudTimerText.text = t.ToString();
        }
    }
    #endregion

    #region MatchStartTimer
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
            if (!hudTimerText.gameObject.activeSelf)
            {
                hudTimerText.gameObject.SetActive(true);
            }
        }
    }
    #endregion

}

