using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Server;

public class GameManager : MonoBehaviour
{

    Toolbox toolbox;
    EventManager em;
    GlobalVariableLibrary lib;
    ShipInfoManager shipInfoManager;

    enum ServerState
    {
        DEFAULT,
        LOBBY,
        INGAME,
        GAMEEND
    };

    ServerState serverState;

    List<ClientData> clientsInLobby = new List<ClientData>();
    List<string> clientsReadyInLobby = new List<string>();
    List<GameObject> currentlyAliveShips = new List<GameObject>();
    int numberOfShips = 4; //TODO: Get this from GVL (or clients?)
    List<int> usedSpawnPoints = new List<int>();
    List<int> usedShipColors = new List<int>();
    List<Transform> respawnPoints = new List<Transform>();
    bool resetUsedSpawnPointsList = false;
    bool resetUsedShipColors = false;
    bool inGame = false;
    bool matchStarted = false;
    bool matchStartTimerRunning = false;
    int fixedUpdateLoopCounter = -1;
    int fixedUpdateLoopsPerSecond = -1;
    int matchStartTimerValue = -1;
    float matchTimer = -1;

    int matchStartTimerLength = -1;
    int maxNumberOfClientsInLobby = -1;
    List<Color> shipColorOptions = new List<Color>();
    #region Initialization
    #region Awake
    private void Awake()
    {
        serverState = ServerState.DEFAULT;

        toolbox = FindObjectOfType<Toolbox>();
        em = toolbox.GetComponent<EventManager>();
        lib = toolbox.GetComponent<GlobalVariableLibrary>();
        shipInfoManager = toolbox.GetComponent<ShipInfoManager>();
        GetStats();
    }
    #endregion

    #region GetStats
    private void GetStats()
    {
        matchStartTimerLength = lib.serverVariables.matchStartTimerLength;
        maxNumberOfClientsInLobby = lib.serverVariables.maxNumberOfClientsInLobby;
        shipColorOptions = lib.serverVariables.shipColorOptions;
    }
    #endregion

    #region OnEnable & OnDisable
    private void OnEnable()
    {
        em.OnClientEnterLobby += OnClientEnterLobby;
        em.OnClientExitLobby += OnClientExitLobby;
        em.OnRequestMatchStart += OnRequestMatchStart;
        em.OnClientVote += OnClientVote;
        em.OnClientDisconnected += OnClientDisconnected;
        em.OnRequestReadyClientCount += OnRequestReadyClientCount;
        em.OnMatchStartTimerStart += OnMatchStartTimerStart;
        em.OnMatchStarted += OnMatchStarted;
        em.OnMatchEnded += OnMatchEnded;
        em.OnShipDead += OnShipDead;
        em.OnRequestMatchRestart += OnRequestMatchRestart;
        em.OnRequestReturnToLobbyFromMatch += OnRequestReturnToLobbyFromMatch;
    }

    private void OnDisable()
    {
        em.OnClientEnterLobby -= OnClientEnterLobby;
        em.OnClientExitLobby -= OnClientExitLobby;
        em.OnRequestMatchStart -= OnRequestMatchStart;
        em.OnClientVote -= OnClientVote;
        em.OnClientDisconnected -= OnClientDisconnected;
        em.OnRequestReadyClientCount -= OnRequestReadyClientCount;
        em.OnMatchStartTimerStart -= OnMatchStartTimerStart;
        em.OnMatchStarted -= OnMatchStarted;
        em.OnMatchEnded -= OnMatchEnded;
        em.OnShipDead -= OnShipDead;
        em.OnRequestMatchRestart += OnRequestMatchRestart;
        em.OnRequestReturnToLobbyFromMatch -= OnRequestReturnToLobbyFromMatch;
    }
    #endregion
    #endregion

    #region Subscribers
    private void OnMatchStartTimerStart()
    {
        StartMatchStartTimer();
    }

    private int OnRequestReadyClientCount()
    {
        Debug.Log("OnRequestReadyClientCount, clientsReadyInLobby.Count: " 
            + clientsReadyInLobby.Count + "clientsInLobby.Count: " + clientsInLobby.Count);
        return clientsReadyInLobby.Count;
    }

    private void OnClientDisconnected(ClientData newClientData)
    {
        //for(int i = 0; i < clientsInLobby.Count; i++)
        //{
        //    if(clientsInLobby[i].id == newClientData.id)
        //    {
        //        em.BroadcastClientExitLobby(newClientData);
        //        break;
        //    }
        //}
    }

    private void OnClientEnterLobby(ClientData newClientData)
    {
        if (serverState == ServerState.DEFAULT)
        {
            serverState = ServerState.LOBBY;
        }

        bool clientAlreadyInLobby = false;
        foreach (ClientData client in clientsInLobby)
        {
            if(client.id == newClientData.id)
            {
                clientAlreadyInLobby = true;
            }
        }

        if (!clientAlreadyInLobby)
        {
            clientsInLobby.Add(newClientData);
            Debug.Log("GameManager: Client added to clientsInLobby list");
        }
        else
        {
            Debug.LogWarning("GameManager: Client already on clientsInLobby list!");
        }
    }

    private void OnClientExitLobby(ClientData disconnectedClientData)
    {
        if (clientsInLobby.Count == 0)
        {
            serverState = ServerState.DEFAULT;
        }

        for (int i = 0; i < clientsInLobby.Count; i++)
        {
            if (clientsInLobby[i].id == disconnectedClientData.id)
            {
                clientsInLobby.RemoveAt(i);
            }
        }
    }

    private void OnRequestMatchStart()
    {
        Debug.Log("GameManager: OnRequestMatchStart");
        if (clientsReadyInLobby.Count == clientsInLobby.Count)
        {
            Debug.Log("GameManager: OnRequestMatchStart, all participants are ready");
            em.BroadcastStartingMatchByServer(numberOfShips);
            inGame = true;
            matchTimer = 0;
            InitializeGame();
        }
        else
        {
            Debug.Log("Cannot start match until all participants are ready");
            em.BroadcastDeniedStartMatchByServer();
        }
    }

    private void OnClientVote(string clientID, int vote)
    {
        if (serverState == ServerState.LOBBY)
        {
            if (vote == 0 || vote == 1)
            {
                if (vote == 0 && clientsReadyInLobby.Contains(clientID))
                {
                    clientsReadyInLobby.Remove(clientID);
                }
                else if (vote == 1 && !clientsReadyInLobby.Contains(clientID))
                {
                    clientsReadyInLobby.Add(clientID);
                }
                else
                {
                    Debug.LogWarning("This client hasn't yet voted and is trying to unvote, or has already voted and is trying to vote a second time!");
                }
            }
            else
            {
                Debug.LogError("Vote value must be 0 or 1 in yes/no events.");
            }
        }
    }

    private void OnMatchStarted()
    {
        matchStarted = true;
    }

    private void OnMatchEnded(int winnerIndex)
    {
        matchStarted = false;
        for(int i = 0; i < currentlyAliveShips.Count; i++)
        {
            Destroy(currentlyAliveShips[i]);
            currentlyAliveShips.RemoveAt(i);
            i--;
        }
        currentlyAliveShips.Clear();
        shipInfoManager.ClearShipInfoList();
    }

    private void OnShipDead(int shipIndex, int killerIndex)
    {
        if (matchStarted)
        {
            if (currentlyAliveShips.Count > 1)
            {
                for (int i = 0; i < currentlyAliveShips.Count; i++)
                {
                    if (currentlyAliveShips[i] == null)
                    {
                        currentlyAliveShips.RemoveAt(i);
                    }
                    else if (currentlyAliveShips[i].GetComponent<ShipController>().GetIndex() == shipIndex)
                    {
                        currentlyAliveShips.RemoveAt(i);
                    }
                }
                if (currentlyAliveShips.Count <= 1)
                {
                    if (currentlyAliveShips.Count > 0)
                    {
                        em.BroadcastMatchEnded(currentlyAliveShips[0].GetComponent<ShipController>().GetIndex());
                    }
                    else
                    {
                        em.BroadcastMatchEnded(killerIndex);
                    }
                }
            }
        }
    }

    private void OnRequestMatchRestart()
    {
        //TODO:
        //Destroy all ships
        //Clear shipInfo
        //Create new ships
        //Reset match timer
        matchStarted = false;
        matchTimer = 0;

        if (currentlyAliveShips.Count > 0)
        {
            for(int i = 0; i < currentlyAliveShips.Count; i++)
            {
                Destroy(currentlyAliveShips[i]);
                currentlyAliveShips.RemoveAt(i);
                i--;
            }
        }
        currentlyAliveShips.Clear();

        resetUsedSpawnPointsList = true;
        resetUsedShipColors = true;

        InitializeGame();
    }

    private void OnRequestReturnToLobbyFromMatch()
    {
        inGame = false;
        matchStarted = false;
        matchTimer = 0;

        if (currentlyAliveShips.Count > 0)
        {
            for (int i = 0; i < currentlyAliveShips.Count; i++)
            {
                Destroy(currentlyAliveShips[i]);
                currentlyAliveShips.RemoveAt(i);
                i--;
            }
        }
        currentlyAliveShips.Clear();
        shipInfoManager.ClearShipInfoList();

        resetUsedSpawnPointsList = true;
        resetUsedShipColors = true;
    }
    #endregion

    #region SetVariables
    public void SetRespawnPoints(List<Transform> newRespawnPoints)
    {
        respawnPoints = newRespawnPoints;
    }
    #endregion

    #region MatchInitialization
    private void InitializeGame()
    {
        Debug.Log("GameManager: InitializeGame");
        shipInfoManager.ClearShipInfoList();
        matchTimer = 0;
        em.BroadcastMatchTimerValueChange(matchTimer);

        #region Instantiate ships
        for (int i = 0; i < numberOfShips; i++)
        {
            int newShipIndex = i + 1;
            string newShipOwner;
            int newSpawnPointIndex = FindAvailableSpawnPoint();
            Debug.Log("GameManager: Accessing respawnPoints FindAvailableSpawnPoint() element");
            Transform spawnPoint = respawnPoints[newSpawnPointIndex];
            Debug.Log("GameManager: RespawnPoints list accessed succesfully");
            GameObject newShip = Instantiate(Resources.Load("Ships/Ship", typeof(GameObject)),
                spawnPoint.position, spawnPoint.rotation) as GameObject;
            ShipController newShipController;
            int newShipColorIndex = FindNewShipColor();
            Debug.Log("GameManager: Accessing shipColorOptions FindNewShipColor() element");
            Color newShipColor = shipColorOptions[newShipColorIndex];
            Debug.Log("GameManager: ShipColorOptions list accessed succesfully");

            ShipInfo newShipInfo = new ShipInfo();
            newShipInfo.shipIndex = newShipIndex;
            newShipInfo.shipPosition = spawnPoint.position;

            Debug.Log("GameManager: Assigning shipControllers");
            if (i < clientsInLobby.Count)
            {
                newShipController =
                    newShip.AddComponent<NetworkPlayerController>();
                newShipOwner = clientsInLobby[i].id;
                newShipInfo.ownerID = newShipOwner;
                newShipInfo.isControlledByServer = false;
                Debug.Log("GameManager: Player ship created");
            }
            else
            {
                newShipController =
                    newShip.AddComponent<AIPlayerController>();
                newShipOwner = "AI" + newShipIndex;
                newShipInfo.ownerID = newShipOwner;
                newShipInfo.isControlledByServer = true;
                Debug.Log("GameManager: AI ship created");
            }
            //Give ship an index and color
            newShipController.GiveIndex(newShipIndex);
            newShipController.SetShipColor(newShipColor);
            currentlyAliveShips.Add(newShip);
            em.BroadcastShipSpawnByServer(newShipIndex, newSpawnPointIndex, newShipColorIndex, newShipOwner);

            shipInfoManager.shipInfoList.Add(newShipInfo);
        }

        foreach (GameObject ship in currentlyAliveShips)
        {
            em.BroadcastShipReference(ship);
        }
        #endregion
    }
    #endregion

    #region Find available ship color
    private int FindNewShipColor()
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

        return r;
    }
    #endregion

    #region Find available spawn point
    private int FindAvailableSpawnPoint()
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

        return r;
    }

    //public Vector3 FindRepawnPoint()
    //{
    //    int i = Random.Range(0, respawnPoints.Count);
    //    return respawnPoints[i].position;
    //}
    #endregion

    #region Update & FixedUpdate
    private void Update()
    {
        if (inGame)
        {
            #region HUD Timer
            if (matchStarted)
            {
                matchTimer += Time.deltaTime;
                em.BroadcastMatchTimerValueChange(matchTimer);
            }
            #endregion
        }
    }

    private void FixedUpdate()
    {
        if (inGame)
        {
            #region MatchStartTimer
            if (matchStartTimerRunning)
            {
                fixedUpdateLoopCounter++;
                if (fixedUpdateLoopCounter >= fixedUpdateLoopsPerSecond)
                {
                    matchStartTimerValue--;
                    em.BroadcastMatchStartTimerValueChange(matchStartTimerValue);
                    if (matchStartTimerValue <= 0)
                    {
                        em.BroadcastMatchStarted();
                        matchStartTimerRunning = false;
                    }
                    fixedUpdateLoopCounter = 0;
                }
            }
            #endregion
        }
    }
    #endregion

    #region MatchStartTimer initialization
    public void StartMatchStartTimer()
    {
        matchStartTimerRunning = true;
        fixedUpdateLoopsPerSecond = Mathf.RoundToInt(1 / Time.fixedDeltaTime);
        matchStartTimerValue = matchStartTimerLength;
        fixedUpdateLoopCounter = 0;
        em.BroadcastMatchStartTimerValueChange(matchStartTimerValue);
    }
    #endregion

}
