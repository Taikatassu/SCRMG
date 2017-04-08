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
    public delegate void StringIntVoid(string string1, int integer);
    #endregion

    #region Events
    public event ClientDataVoid OnClientConnected;
    public void BroadcastClientConnected(ClientData clientID)
    {
        if (OnClientConnected != null)
        {
            OnClientConnected(clientID);
        }
    }

    public event ClientDataVoid OnClientDisconnected;
    public void BroadcastClientDisconnected(ClientData clientID)
    {
        if (OnClientDisconnected != null)
        {
            OnClientDisconnected(clientID);
        }
    }

    public event ClientDataVoid OnClientEnterLobby;
    public void BroadcastClientEnterLobby(ClientData clientID)
    {
        if (OnClientEnterLobby != null)
        {
            OnClientEnterLobby(clientID);
        }
    }

    public event ClientDataVoid OnClientExitLobby;
    public void BroadcastClientExitLobby(ClientData clientID)
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
