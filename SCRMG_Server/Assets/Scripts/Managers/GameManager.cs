using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Server;

public class GameManager : MonoBehaviour
{

    Toolbox toolbox;
    EventManager em;

    enum ServerState
    {
        DEFAULT,
        LOBBY,
        INGAME,
        GAMEEND
    };

    ServerState serverState;

    List<ClientData> clientsInLobby = new List<ClientData>();
    List<string> clientsAlreadyVoted = new List<string>();
    List<GameObject> currentlyAliveShips = new List<GameObject>();
    int numberOfClientsReady = -1;
    int numberOfShips = 4; //TODO: Get this from GVL (or clients?)
    List<int> usedSpawnPoints = new List<int>();
    List<int> usedShipColors = new List<int>();
    List<Transform> respawnPoints = new List<Transform>();
    bool resetUsedSpawnPointsList = false;
    bool resetUsedShipColors = false;

    List<Color> shipColorOptions = new List<Color>();
    #region Initialization
    #region Awake
    private void Awake()
    {
        serverState = ServerState.DEFAULT;

        toolbox = FindObjectOfType<Toolbox>();
        em = toolbox.GetComponent<EventManager>();
    }
    #endregion

    #region OnEnable & OnDisable
    private void OnEnable()
    {
        em.OnClientEnterLobby += OnClientEnterLobby;
        em.OnClientExitLobby += OnClientExitLobby;
        em.OnRequestMatchStart += OnRequestMatchStart;
        em.OnClientVote += OnClientVote;
    }

    private void OnDisable()
    {
        em.OnClientEnterLobby -= OnClientEnterLobby;
        em.OnClientExitLobby -= OnClientExitLobby;
        em.OnRequestMatchStart -= OnRequestMatchStart;
        em.OnClientVote -= OnClientVote;
    }
    #endregion
    #endregion

    #region Subscribers
    private void OnClientEnterLobby(ClientData newClientID)
    {
        if (serverState == ServerState.DEFAULT)
        {
            serverState = ServerState.LOBBY;
        }

        clientsInLobby.Add(newClientID);
    }

    private void OnClientExitLobby(ClientData disconnectedClientID)
    {
        clientsInLobby.Remove(disconnectedClientID);
    }

    private void OnRequestMatchStart()
    {
        if (numberOfClientsReady == clientsInLobby.Count)
        {
            InitializeGame();
        }
        else
        {
            Debug.Log("Cannot start match until all participants are ready");
        }
    }

    private void OnClientVote(string clientID, int vote)
    {
        if (serverState == ServerState.LOBBY)
        {
            if (vote == 0 || vote == 1)
            {
                if (vote == 0)
                {
                    numberOfClientsReady--;
                }
                else
                {
                    numberOfClientsReady++;
                }
            }
            else
            {
                Debug.LogError("Vote value must be 0 or 1 in yes/no events.");
            }
        }
    }
    #endregion

    #region MatchInitialization
    private void InitializeGame()
    {
        #region Instantiate ships
        for (int i = 0; i < numberOfShips; i++)
        {
            int newShipIndex = i + 1;
            Transform spawnPoint = FindAvailableSpawnPoint();
            GameObject newShip = Instantiate(Resources.Load("Ships/Ship", typeof(GameObject)),
                spawnPoint.position, spawnPoint.rotation) as GameObject;
            ShipController newShipController;
            Color newShipColor = FindNewShipColor();

            ShipInfo newShipInfo = new ShipInfo();
            newShipInfo.shipIndex = newShipIndex;
            newShipInfo.shipPosition = spawnPoint.position;

            #region Singleplayer ship instantiating
            if (i < clientsInLobby.Count)
            {
                newShipController =
                    newShip.AddComponent<NetworkPlayerController>();
                newShipInfo.ownerID = clientsInLobby[i].id;
            }
            else
            {
                newShipController =
                    newShip.AddComponent<AIPlayerController>();
                newShipInfo.ownerID = "AI" + newShipIndex;
            }
            //Give ship an index and color
            newShipController.GiveIndex(newShipIndex);
            newShipController.SetShipColor(newShipColor);
            #endregion

            currentlyAliveShips.Add(newShip);

            //shipInfoList.Add(newShipInfo);
        }

        foreach (GameObject ship in currentlyAliveShips)
        {
            //em.BroadcastShipReference(ship);
        }
        #endregion
    }
    #endregion

    #region Find available ship color
    private Color FindNewShipColor()
    {
        if (resetUsedShipColors)
        {
            usedShipColors.Clear();
            resetUsedShipColors = false;
        }

        int r = Random.Range(0, shipColorOptions.Count);
        while (usedShipColors.Contains(r))
        {
            Debug.Log("LevelManager: Inside FindNewShipColor while loop");
            r = Random.Range(0, shipColorOptions.Count);
        }
        usedShipColors.Add(r);

        if (usedShipColors.Count == shipColorOptions.Count)
        {
            resetUsedShipColors = true;
        }

        return shipColorOptions[r];
    }
    #endregion

    #region Find available spawn point
    private Transform FindAvailableSpawnPoint()
    {
        if (resetUsedSpawnPointsList)
        {
            usedSpawnPoints.Clear();
            resetUsedSpawnPointsList = false;
        }

        int r = Random.Range(0, respawnPoints.Count);
        while (usedSpawnPoints.Contains(r))
        {
            Debug.Log("LevelManager: Inside FindInitialSpawnPoint while loop");
            r = Random.Range(0, respawnPoints.Count);
        }
        usedSpawnPoints.Add(r);

        if (usedSpawnPoints.Count == respawnPoints.Count)
        {
            resetUsedSpawnPointsList = true;
        }

        return respawnPoints[r];
    }

    //public Vector3 FindRepawnPoint()
    //{
    //    int i = Random.Range(0, respawnPoints.Count);
    //    return respawnPoints[i].position;
    //}
    #endregion

}
