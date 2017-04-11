using System.Collections;
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

    List<GameObject> connectedClientInfos = new List<GameObject>();
    List<GameObject> lobbyClientInfos = new List<GameObject>();

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
        em.OnClientConnected += OnClientConnected;
        em.OnClientDisconnected += OnClientDisconnected;
        em.OnClientEnterLobby += OnClientEnterLobby;
        em.OnClientExitLobby += OnClientExitLobby;
    }

    private void OnDisable()
    {
        em.OnClientConnected -= OnClientConnected;
        em.OnClientDisconnected -= OnClientDisconnected;
        em.OnClientEnterLobby -= OnClientEnterLobby;
        em.OnClientExitLobby -= OnClientExitLobby;
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
    #endregion

}

