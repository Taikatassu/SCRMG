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
    public delegate void IntVoid(int integer);
    public delegate void FloatVoid(float floatingPoint);
    public delegate void ClientDataVoid(ClientData clientData);
    public delegate void StringVoid(string string1);
    public delegate void StringIntVoid(string string1, int integer);
    public delegate bool ClientDataBool(ClientData clientData);
    public delegate int EmptyInt();
    public delegate void IntIntIntFloatVector3Void(int integer1, int integer2, int integer3, float floatingPoint, Vector3 vec3);
    public delegate void IntIntIntStringVoid(int integer1, int integer2, int integer3, string string1);
    public delegate void GameObjectVoid(GameObject gameObject);
    public delegate void IntVector3Void(int integer, Vector3 vec3);
    public delegate void IntIntVector3Vector3Void(int integer1, int integer2, Vector3 vec31, Vector3 vec32);
    public delegate void IntIntVoid(int integer1, int integer2);
    public delegate void IntIntVector3Void(int integer1, int integer2, Vector3 vec3);
    public delegate void IntIntIntFloatVoid(int integer1, int integer2, int integer3, float floatingPoint);
    public delegate void IntBoolVoid(int integer, bool boolean);
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

    public event IntVoid OnStartingMatchByServer;
    public void BroadcastStartingMatchByServer(int numberOfShips)
    {
        if (OnStartingMatchByServer != null)
        {
            OnStartingMatchByServer(numberOfShips);
        }
    }

    public event EmptyVoid OnDeniedStartMatchByServer;
    public void BroadcastDeniedStartMatchByServer()
    {
        if (OnDeniedStartMatchByServer != null)
        {
            OnDeniedStartMatchByServer();
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

    public event IntIntVoid OnShipDead;
    public void BroadcastShipDead(int shipIndex, int killerIndex)
    {
        if (OnShipDead != null)
        {
            OnShipDead(shipIndex, killerIndex);
        }
    }

    public event IntIntVoid OnShipDeadByClient;
    public void BroadcastShipDeadByClient(int shipIndex, int killerIndex)
    {
        if (OnShipDeadByClient != null)
        {
            OnShipDeadByClient(shipIndex, killerIndex);
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

    public event EmptyVoid OnMatchStartTimerStart;
    public void BroadcastMatchStartTimerStart()
    {
        if (OnMatchStartTimerStart != null)
        {
            OnMatchStartTimerStart();
        }
    }

    public event IntVoid OnMatchStartTimerValueChange;
    public void BroadcastMatchStartTimerValueChange(int currentValue)
    {
        if (OnMatchStartTimerValueChange != null)
        {
            OnMatchStartTimerValueChange(currentValue);
        }
    }

    public event EmptyVoid OnMatchStarted;
    public void BroadcastMatchStarted()
    {
        if (OnMatchStarted != null)
        {
            OnMatchStarted();
        }
    }

    public event IntVoid OnMatchEnded;
    public void BroadcastMatchEnded(int winnerIndex)
    {
        if (OnMatchEnded != null)
        {
            OnMatchEnded(winnerIndex);
        }
    }

    public event FloatVoid OnMatchTimerValueChange;
    public void BroadcastMatchTimerValueChange(float newValue)
    {
        if (OnMatchTimerValueChange != null)
        {
            OnMatchTimerValueChange(newValue);
        }
    }

    public event IntIntVector3Vector3Void OnProjectileSpawned;
    public void BroadcastProjectileSpawned(int projectileOwnerIndex, int projectileIndex, Vector3 spawnPosition, Vector3 spawnRotation)
    {
        if (OnProjectileSpawned != null)
        {
            OnProjectileSpawned(projectileOwnerIndex, projectileIndex, spawnPosition, spawnRotation);
        }
    }

    public event IntIntVector3Vector3Void OnProjectileSpawnedByClient;
    public void BroadcastProjectileSpawnedByClient(int projectileOwnerIndex, int projectileIndex, Vector3 spawnPosition, Vector3 spawnRotation)
    {
        if (OnProjectileSpawnedByClient != null)
        {
            OnProjectileSpawnedByClient(projectileOwnerIndex, projectileIndex, spawnPosition, spawnRotation);
        }
    }

    public event IntIntIntFloatVoid OnProjectileHitShip;
    public void BroadcastProjectileHitShip(int projectileOwnerIndex, int projectileIndex, int hitShipIndex, float projectileDamage)
    {
        if (OnProjectileHitShip != null)
        {
            OnProjectileHitShip(projectileOwnerIndex, projectileIndex, hitShipIndex, projectileDamage);
        }
    }

    public event IntIntIntFloatVoid OnProjectileHitShipByClient;
    public void BroadcastProjectileHitShipByClient(int projectileOwnerIndex, int projectileIndex, int hitShipIndex, float projectileDamage)
    {
        if (OnProjectileHitShipByClient != null)
        {
            OnProjectileHitShipByClient(projectileOwnerIndex, projectileIndex, hitShipIndex, projectileDamage);
        }
    }

    public event IntIntVector3Void OnProjectileDestroyed;
    public void BroadcastProjectileDestroyed(int projectileOwnerIndex, int projectileIndex, Vector3 location)
    {
        if (OnProjectileDestroyed != null)
        {
            OnProjectileDestroyed(projectileOwnerIndex, projectileIndex, location);
        }
    }

    public event IntIntVector3Void OnProjectileDestroyedByClient;
    public void BroadcastProjectileDestroyedByClient(int projectileOwnerIndex, int projectileIndex, Vector3 location)
    {
        if (OnProjectileDestroyedByClient != null)
        {
            OnProjectileDestroyedByClient(projectileOwnerIndex, projectileIndex, location);
        }
    }

    public event EmptyVoid OnRequestMatchRestart;
    public void BroadcastRequestMatchRestart()
    {
        if (OnRequestMatchRestart != null)
        {
            OnRequestMatchRestart();
        }
    }

    public event EmptyVoid OnRequestReturnToLobbyFromMatch;
    public void BroadcastRequestReturnToLobbyFromMatch()
    {
        if (OnRequestReturnToLobbyFromMatch != null)
        {
            OnRequestReturnToLobbyFromMatch();
        }
    }
    #endregion

}
