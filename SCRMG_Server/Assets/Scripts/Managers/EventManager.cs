using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Server;

public class EventManager : MonoBehaviour {

    #region Initialization
    public static EventManager instance;
    private void Awake()
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
    }
    #endregion

    #region Delegates
    public delegate void EmptyVoid();
    public delegate void ClientDataVoid(ClientData clientData);
    public delegate void StringVoid(string string1);
    public delegate void StringIntVoid(string string1, int integer);
    public delegate bool ClientDataBool(ClientData clientData);
    #endregion

    #region Events
    public event ClientDataVoid OnClientConnected;
    public void BroadcastClientConnected(ClientData clientData)
    {
        if (OnClientConnected != null)
        {
            OnClientConnected(clientData);
        }
    }

    public event StringVoid OnClientDisconnected;
    public void BroadcastClientDisconnected(string clientID)
    {
        if (OnClientDisconnected != null)
        {
            OnClientDisconnected(clientID);
        }
    }

    public event ClientDataBool OnClientRequestLobbyAccess;
    public bool BroadcastClientRequestLobbyAccess(ClientData clientData)
    {
        if (OnClientRequestLobbyAccess != null)
        {
            return OnClientRequestLobbyAccess(clientData);
        }

        return false;
    }

    public event ClientDataVoid OnClientEnterLobby;
    public void BroadcastClientEnterLobby(ClientData clientData)
    {
        if (OnClientEnterLobby != null)
        {
            OnClientEnterLobby(clientData);
        }
    }

    public event StringVoid OnClientExitLobby;
    public void BroadcastClientExitLobby(string clientID)
    {
        if (OnClientExitLobby != null)
        {
            OnClientExitLobby(clientID);
        }
    }

    public event EmptyVoid OnRequestMatchStart;
    public void BroadcastRequestMatchStart()
    {
        if (OnRequestMatchStart != null)
        {
            OnRequestMatchStart();
        }
    }

    public event StringIntVoid OnClientVote;
    public void BroadcastClientVote(string clientID, int vote)
    {
        if (OnClientVote != null)
        {
            OnClientVote(clientID, vote);
        }
    }
    #endregion

}
