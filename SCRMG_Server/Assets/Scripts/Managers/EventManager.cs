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
    public delegate int EmptyInt();
    public delegate void IntIntIntStringVoid(int integer1, int integer2, int integer3, string string1);
    public delegate void GameObjectVoid(GameObject gameObject);
    public delegate void IntVector3Void(int integer, Vector3 vec3);
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

    public event ClientDataVoid OnClientDisconnected;
    public void BroadcastClientDisconnected(ClientData clientData)
    {
        if (OnClientDisconnected != null)
        {
            OnClientDisconnected(clientData);
        }
    }

    public event ClientDataVoid OnClientEnterLobby;
    public void BroadcastClientEnterLobby(ClientData clientData)
    {
        if (OnClientEnterLobby != null)
        {
            OnClientEnterLobby(clientData);
        }
    }

    public event ClientDataVoid OnClientExitLobby;
    public void BroadcastClientExitLobby(ClientData clientData)
    {
        if (OnClientExitLobby != null)
        {
            OnClientExitLobby(clientData);
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

    public event EmptyVoid OnStartingMatchByServer;
    public void BroadcastStartingMatchByServer()
    {
        if (OnStartingMatchByServer != null)
        {
            OnStartingMatchByServer();
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

    public event EmptyInt OnRequestReadyClientCount;
    public int BroadcastRequestReadyClientCount()
    {
        if (OnRequestReadyClientCount != null)
        {
            return OnRequestReadyClientCount();
        }

        return 0;
    }

    public event IntIntIntStringVoid OnShipSpawnByServer;
    public void BroadcastShipSpawnByServer(int shipIndex, int spawnPointIndex, int shipColorIndex, string ownerID)
    {
        if (OnShipSpawnByServer != null)
        {

            OnShipSpawnByServer(shipIndex, spawnPointIndex, shipColorIndex, ownerID);
        }
    }
    
    public event GameObjectVoid OnShipReference;
    public void BroadcastShipReference(GameObject newShip)
    {
        if (OnShipReference != null)
        {
            OnShipReference(newShip);
        }
    }

    public event IntVector3Void OnShipPositionUpdate;
    public void BroadcastShipPositionUpdate(int shipIndex, Vector3 shipPosition)
    {
        if (OnShipPositionUpdate != null)
        {
            OnShipPositionUpdate(shipIndex, shipPosition);
        }
    }
    #endregion

}
