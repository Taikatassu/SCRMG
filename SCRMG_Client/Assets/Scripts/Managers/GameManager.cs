using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    /* TODO:
     * - Different gameMode implementations
     */

    #region References & variables
    //References
    public static GameManager instance;

    Toolbox toolbox;
    EventManager em;
    GlobalVariableLibrary lib;
    ShipInfoManager shipInfoManager;
    List<Transform> respawnPoints = new List<Transform>();
    List<Transform> powerUpPositions = new List<Transform>();
    List<GameObject> currentlyAliveShips = new List<GameObject>();
    List<GameObject> currentlyExistingPowerUps = new List<GameObject>();
    GameObject currentPlayerCamera;
    //Variables coming from within the script
    List<int> usedSpawnPoints = new List<int>();
    List<int> usedShipColors = new List<int>();
    //List<int> currentlyAliveShipIndices = new List<int>();
    List<ShipInfo> shipsToSpawn = new List<ShipInfo>();
    bool resetUsedSpawnPointsList = false;
    bool resetUsedShipColors = false;
    bool matchStartTimerRunning = false;
    bool isPaused = false;
    bool matchStarted = false;
    bool inGame = false;
    bool readyToSpawnShips = false;
    bool nextFrame = false;
    bool waitingTillNextFrame = false;
    int fixedUpdateLoopCounter = -1;
    int fixedUpdateLoopsPerSecond = -1;
    int matchStartTimerValue = -1;
    int currentGameModeIndex = -1;
    int numberOfPlayersInLobby = -1;
    int numberOfLobbyParticipantsReady = -1;
    float matchTimer = 0;

    //Variables coming from globalVariableLibrary
    List<Color> shipColorOptions = new List<Color>();
    int gameModeSingleplayerIndex = -1;
    int gameModeNetworkMultiplayerIndex = -1;
    int gameModeLocalMultiplayerIndex = -1;
    int numberOfShips; //Can also be set with the public "SetNumberOfShips()"-function
    int matchStartTimerLength = -1;
    int sceneIndexMainMenu = -1;
    int sceneIndexLevel01 = -1;
    bool powerUpsDisabled = false;
    #endregion

    #region Initialization
    #region Awake
    void Awake()
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
        gameModeSingleplayerIndex = lib.gameSettingVariables.gameModeSingleplayerIndex;
        gameModeNetworkMultiplayerIndex = lib.gameSettingVariables.gameModeNetworkMultiplayerIndex;
        gameModeLocalMultiplayerIndex = lib.gameSettingVariables.gameModeLocalMultiplayerIndex;
        shipColorOptions = lib.shipVariables.shipColorOptions;
        matchStartTimerLength = lib.sceneVariables.matchStartTimerLength;
        numberOfShips = lib.sceneVariables.numberOfShips;
        sceneIndexMainMenu = lib.sceneVariables.sceneIndexMainMenu;
        sceneIndexLevel01 = lib.sceneVariables.sceneIndexLevel01;
        powerUpsDisabled = lib.powerUpVariables.powerUpsDisabled;

        numberOfPlayersInLobby = 0;
        numberOfLobbyParticipantsReady = 0;
    }
    #endregion

    #region OnEnable & OnDisable
    private void OnEnable()
    {
        em.OnMatchStarted += OnMatchStarted;
        em.OnMatchEnded += OnMatchEnded;
        em.OnGameRestart += OnGameRestart;
        em.OnNewSceneLoading += OnNewSceneLoading;
        em.OnNewSceneLoaded += OnNewSceneLoaded;
        em.OnShipDead += OnShipDead;
        em.OnSetGameMode += OnSetGameMode;
        em.OnPauseOn += OnPauseOn;
        em.OnPauseOff += OnPauseOff;
        //em.OnPowerUpPickedUp += OnPowerUpPickedUp;
        em.OnPowerUpOnline += OnPowerUpOnline;
        em.OnConnectionToNetworkLost += OnConnectionToNetworkLost;
        em.OnShipSpawnByServer += OnShipSpawnByServer;
        em.OnClientCountInLobbyChange += OnClientCountInLobbyChange;
        em.OnReadyCountInLobbyChange += OnReadyCountInLobbyChange;
        em.OnStartingMatchByServer += OnStartingMatchByServer;
        em.OnRestartingMatchByServer += OnRestartingMatchByServer;
        em.OnRequestCurrentGameModeIndex += OnRequestCurrentGameModeIndex;
        em.OnNetworkMultiplayerStartMatchStartTimer += OnNetworkMultiplayerStartMatchStartTimer;
        em.OnMatchEndedByServer += OnMatchEndedByServer;
        em.OnReturnToLobbyFromMatch += OnReturnToLobbyFromMatch;
    }

    private void OnDisable()
    {
        em.OnMatchStarted -= OnMatchStarted;
        em.OnMatchEnded -= OnMatchEnded;
        em.OnGameRestart -= OnGameRestart;
        em.OnNewSceneLoading -= OnNewSceneLoading;
        em.OnNewSceneLoaded -= OnNewSceneLoaded;
        em.OnShipDead -= OnShipDead;
        em.OnSetGameMode -= OnSetGameMode;
        em.OnPauseOn -= OnPauseOn;
        em.OnPauseOff -= OnPauseOff;
        //em.OnPowerUpPickedUp -= OnPowerUpPickedUp;
        em.OnPowerUpOnline -= OnPowerUpOnline;
        em.OnConnectionToNetworkLost -= OnConnectionToNetworkLost;
        em.OnShipSpawnByServer -= OnShipSpawnByServer;
        em.OnClientCountInLobbyChange -= OnClientCountInLobbyChange;
        em.OnReadyCountInLobbyChange -= OnReadyCountInLobbyChange;
        em.OnStartingMatchByServer -= OnStartingMatchByServer;
        em.OnRestartingMatchByServer -= OnRestartingMatchByServer;
        em.OnRequestCurrentGameModeIndex -= OnRequestCurrentGameModeIndex;
        em.OnNetworkMultiplayerStartMatchStartTimer -= OnNetworkMultiplayerStartMatchStartTimer;
        em.OnMatchEndedByServer -= OnMatchEndedByServer;
        em.OnReturnToLobbyFromMatch -= OnReturnToLobbyFromMatch;
    }
    #endregion
    #endregion

    #region Subscribers
    #region Network event subscribers
    private void OnNetworkMultiplayerStartMatchStartTimer()
    {
        StartMatchStartTimer();
        readyToSpawnShips = false;
    }

    private void OnStartingMatchByServer(int newNumberOfShips)
    {
        em.BroadcastRequestSceneSingleLevel01();
        numberOfShips = newNumberOfShips;
        matchTimer = 0;
    }

    private void OnRestartingMatchByServer(int newNumberOfShips)
    {
        numberOfShips = newNumberOfShips;
        matchTimer = 0;

        inGame = true;
        DestroyAllObjects();
        resetUsedShipColors = true;
        resetUsedSpawnPointsList = true;
        readyToSpawnShips = true;
        shipInfoManager.ClearShipInfoList();

        if (shipsToSpawn.Count > 0)
        {
            for (int i = 0; i < shipsToSpawn.Count; i++)
            {
                SpawnShip(shipsToSpawn[i].shipIndex, shipsToSpawn[i].spawnPointIndex,
                    shipsToSpawn[i].shipColorIndex, shipsToSpawn[i].ownerID);
                shipsToSpawn.RemoveAt(i);
                i--;
            }
            em.BroadcastNetworkMultiplayerMatchInitialized();
            Debug.Log("GameManager: multiplayer match initialization broadcasted");
        }
    }

    private void OnConnectionToNetworkLost()
    {
        if (em.BroadcastRequestCurrentSceneIndex() == sceneIndexLevel01)
        {
            Debug.Log("GameManager, OnConnectionToNetworkLost: Currently in game, requesting main menu scene");
            em.BroadcastRequestSceneSingleMainMenu();
        }
    }

    private void OnShipSpawnByServer(int shipIndex, int spawnPointIndex, int shipColorIndex, string ownerID)
    {
        Debug.Log("OnShipSpawnByServer");
        if (currentGameModeIndex == gameModeNetworkMultiplayerIndex)
        {
            if (readyToSpawnShips)
            {
                Debug.Log("readyToSpawnShips = true, spawning ship instantly");
                SpawnShip(shipIndex, spawnPointIndex, shipColorIndex, ownerID);
                if (currentlyAliveShips.Count == numberOfShips)
                {
                    em.BroadcastNetworkMultiplayerMatchInitialized();
                    Debug.Log("GameManager: multiplayer match initialization broadcasted, numberOfShips: " + numberOfShips);
                }
            }
            else
            {
                Debug.Log("readyToSpawnShips = false, adding shipInfo on shipsToSpawn list");
                ShipInfo newShipInfo = new ShipInfo();
                newShipInfo.shipIndex = shipIndex;
                newShipInfo.spawnPointIndex = spawnPointIndex;
                newShipInfo.shipColorIndex = shipColorIndex;
                newShipInfo.ownerID = ownerID;

                shipsToSpawn.Add(newShipInfo);
            }
        }
        else
        {
            Debug.LogError("Server tried spawning ship even though not in NetworkMultiplayer gameMode");
        }
    }

    private void OnClientCountInLobbyChange(int change)
    {
        numberOfPlayersInLobby = change;
    }

    private void OnReadyCountInLobbyChange(int change)
    {
        numberOfLobbyParticipantsReady = change;
    }

    private void OnMatchEndedByServer(string winnerName, bool localPlayerWins)
    {
        matchStarted = false;
        for (int i = 0; i < currentlyAliveShips.Count; i++)
        {
            Destroy(currentlyAliveShips[i]);
            currentlyAliveShips.RemoveAt(i);
        }
        currentlyAliveShips.Clear();
        shipInfoManager.ClearShipInfoList();
    }

    private void OnReturnToLobbyFromMatch()
    {
        em.BroadcastRequestSceneSingleMainMenu();
    }
    #endregion

    #region Game event subscribers
    private int OnRequestCurrentGameModeIndex()
    {
        return currentGameModeIndex;
    }

    private void OnMatchStarted()
    {
        matchStarted = true;
    }

    private void OnMatchEnded(int winnerIndex, float matchLength)
    {
        matchStarted = false;
        shipInfoManager.ClearShipInfoList();
    }

    private void OnPauseOn()
    {
        if (currentGameModeIndex == gameModeSingleplayerIndex)
        {
            isPaused = true;
            Time.timeScale = 0;
        }
        else if (currentGameModeIndex == gameModeNetworkMultiplayerIndex)
        {
            // TODO: Decide how to manage pausing in NetMP gameMode
        }
        else if (currentGameModeIndex == gameModeLocalMultiplayerIndex)
        {
            // TODO: Decide how to manage pausing in LocMP gameMode
        }
    }

    private void OnPauseOff()
    {
        if (currentGameModeIndex == gameModeSingleplayerIndex)
        {
            isPaused = false;
            Time.timeScale = 1;
        }
        else if (currentGameModeIndex == gameModeNetworkMultiplayerIndex)
        {
            // TODO: Decide how to manage pausing in NetMP gameMode
        }
        else if (currentGameModeIndex == gameModeLocalMultiplayerIndex)
        {
            // TODO: Decide how to manage pausing in LocMP gameMode
        }
    }

    private void OnNewSceneLoading(int sceneIndex)
    {
        //TODO: Remember to implement check for all future scenes
        if (sceneIndex == sceneIndexMainMenu)
        {
            readyToSpawnShips = false;
            matchStarted = false;
            inGame = false;
            DestroyAllObjects();
            shipInfoManager.ClearShipInfoList();
        }
        else if (sceneIndex == sceneIndexLevel01)
        {
            DestroyAllObjects();
            resetUsedShipColors = true;
            resetUsedSpawnPointsList = true;
        }
    }

    private void OnNewSceneLoaded(int sceneIndex)
    {
        //TODO: Remember to implement check for all future scenes
        if (sceneIndex == sceneIndexMainMenu)
        {

        }
        else if (sceneIndex == sceneIndexLevel01 && currentGameModeIndex == gameModeSingleplayerIndex)
        {
            inGame = true;
            DestroyAllObjects();
            resetUsedShipColors = true;
            resetUsedSpawnPointsList = true;
            InitializeGame();
            StartMatchStartTimer();
        }
        else if (sceneIndex == sceneIndexLevel01 && currentGameModeIndex == gameModeNetworkMultiplayerIndex)
        {
            inGame = true;
            //DestroyAllObjects();
            resetUsedShipColors = true;
            resetUsedSpawnPointsList = true;
        }
    }

    private void OnGameRestart()
    {
        DestroyAllObjects();
        //Reset all
        resetUsedShipColors = true;
        resetUsedSpawnPointsList = true;
        shipInfoManager.ClearShipInfoList();
        waitingTillNextFrame = true;
    }

    private void OnShipDead(int shipIndex, int killerIndex, float lifetime)
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
            if (currentGameModeIndex != gameModeNetworkMultiplayerIndex)
            {
                if (currentlyAliveShips.Count == 1)
                {
                    //em.BroadcastMatchEnded(currentlyAliveShips[0].GetComponent<ShipController>().GetIndex());
                    em.BroadcastMatchEnded(killerIndex, matchTimer);
                }
            }
        }
    }

    private void OnSetGameMode(int newGameModeIndex)
    {
        currentGameModeIndex = newGameModeIndex;
        if (currentGameModeIndex == gameModeNetworkMultiplayerIndex)
        {
            powerUpsDisabled = true;
            Debug.Log("Disabling powerUps for Network Multiplayer mode");
        }
        else if (currentGameModeIndex == gameModeSingleplayerIndex)
        {
            powerUpsDisabled = lib.powerUpVariables.powerUpsDisabled;
            Debug.Log("PowerUpsDisabled state reset for Singleplayer mode");
        }
    }

    private void OnPowerUpPickedUp(int shipIndex, int powerUpBaseIndex, int powerUpType)
    {
        //Debug.Log("GameManager: PowerUp index: " + powerUpBaseIndex + " (Type: " + powerUpType + ") picked up by ship " + shipIndex);
    }

    private void OnPowerUpOnline(int powerUpBaseIndex, int powerUpType)
    {
        //TODO: Do something with this
        //Debug.Log("GameManager: PowerUp index: " + powerUpBaseIndex + " back online with type: " + powerUpType);
    }
    #endregion
    #endregion

    #region Match initialization
    #region DestroyAllObjects
    private void DestroyAllObjects()
    {
        //Destroy all existing ships and clear ship list
        if (currentlyAliveShips.Count > 0)
        {
            for (int i = 0; i < currentlyAliveShips.Count; i++)
            {
                if (currentlyAliveShips[0] != null)
                {
                    Destroy(currentlyAliveShips[0]);
                }
                currentlyAliveShips.RemoveAt(0);
                i--;
            }
            currentlyAliveShips.Clear();
        }

        //Destroy player camera
        if (currentPlayerCamera != null)
        {
            Destroy(currentPlayerCamera);
        }

        //Destroy all existing powerUps and clear powerUp list
        if (currentlyExistingPowerUps.Count > 0)
        {
            foreach (GameObject powerUpObject in currentlyExistingPowerUps)
            {
                Destroy(powerUpObject);
            }
            currentlyExistingPowerUps.Clear();
        }
    }
    #endregion

    #region InitializeGame
    private void InitializeGame()
    {
        matchStarted = false;
        matchTimer = 0;
        em.BroadcastMatchTimerValueChange(matchTimer);

        #region Instantiate ships
        for (int i = 0; i < numberOfShips; i++)
        {
            int newShipIndex = i + 1;
            Transform spawnPoint = FindAvailableSpawnPoint();
            GameObject newShip = Instantiate(Resources.Load("Ships/Ship", typeof(GameObject)),
                spawnPoint.position, spawnPoint.rotation) as GameObject;
            ShipController newShipController;
            Color newShipColor = FindNewShipColor();

            if (currentGameModeIndex == gameModeSingleplayerIndex)
            {
                #region Singleplayer ship instantiating
                if (i == 0)
                {
                    newShipController =
                        newShip.AddComponent<LocalPlayerController>();
                    GameObject newPlayerIndicator = Instantiate(Resources.Load("Effects/PlayerIndicator",
                        typeof(GameObject)), newShip.transform.position, Quaternion.identity,
                        newShip.transform) as GameObject;
                    //Set playerIndicator color
                    ParticleSystem.MainModule pIMain = newPlayerIndicator.GetComponentInChildren<ParticleSystem>().main;
                    pIMain.startColor = new Color(newShipColor.r, newShipColor.g, newShipColor.b, 1);
                    currentPlayerCamera = Instantiate(Resources.Load("Cameras/PlayerCamera",
                        typeof(GameObject)), Vector3.zero, Quaternion.identity) as GameObject;
                    CameraController currentCameraScript = currentPlayerCamera.GetComponentInChildren<CameraController>();
                    currentCameraScript.SetTarget(newShip.transform);
                    currentCameraScript.SetMyShipIndex(newShipIndex);
                }
                else
                {
                    newShipController =
                        newShip.AddComponent<AIPlayerController>();
                }
                //Set currentGameMode in shipController
                newShipController.SetGameMode(currentGameModeIndex);
                //Give ship an index and color
                newShipController.GiveIndex(newShipIndex);
                newShipController.SetShipColor(newShipColor);
                #endregion
            }
            else if (currentGameModeIndex == gameModeNetworkMultiplayerIndex)
            {
                #region NetworkMultiplayer ship instantiating
                if (i == 0)
                {
                    newShipController =
                        newShip.AddComponent<LocalPlayerController>();
                    GameObject newPlayerIndicator = Instantiate(Resources.Load("Effects/PlayerIndicator",
                        typeof(GameObject)), newShip.transform.position, Quaternion.identity,
                        newShip.transform) as GameObject;
                    //Set playerIndicator color
                    ParticleSystem.MainModule pIMain = newPlayerIndicator.GetComponentInChildren<ParticleSystem>().main;
                    pIMain.startColor = new Color(newShipColor.r, newShipColor.g, newShipColor.b, 1);
                    currentPlayerCamera = Instantiate(Resources.Load("Cameras/PlayerCamera",
                        typeof(GameObject)), Vector3.zero, Quaternion.identity) as GameObject;
                    CameraController currentCameraScript = currentPlayerCamera.GetComponentInChildren<CameraController>();
                    currentCameraScript.SetTarget(newShip.transform);
                    currentCameraScript.SetMyShipIndex(newShipIndex);
                }
                else
                {
                    newShipController =
                        newShip.AddComponent<NetworkPlayerController>();
                }
                //Set currentGameMode in shipController
                newShipController.SetGameMode(currentGameModeIndex);
                //Give ship an index and color
                newShipController.GiveIndex(newShipIndex);
                newShipController.SetShipColor(newShipColor);
                #endregion
            }
            else if (currentGameModeIndex == gameModeLocalMultiplayerIndex)
            {
                #region LocalMultiplayer ship instantiating [WIP]
                //TODO: Below is completely WIP
                newShipController =
                        newShip.AddComponent<LocalPlayerController>();
                GameObject newPlayerIndicator = Instantiate(Resources.Load("Effects/PlayerIndicator",
                    typeof(GameObject)), newShip.transform.position, Quaternion.identity,
                    newShip.transform) as GameObject;
                //Set playerIndicator color
                ParticleSystem.MainModule pIMain = newPlayerIndicator.GetComponentInChildren<ParticleSystem>().main;
                pIMain.startColor = new Color(newShipColor.r, newShipColor.g, newShipColor.b, 1);
                currentPlayerCamera = Instantiate(Resources.Load("Cameras/PlayerCamera",
                    typeof(GameObject)), Vector3.zero, Quaternion.identity) as GameObject;
                CameraController currentCameraScript = currentPlayerCamera.GetComponentInChildren<CameraController>();
                currentCameraScript.SetTarget(newShip.transform);
                currentCameraScript.SetMyShipIndex(newShipIndex);

                //Set currentGameMode in shipController
                newShipController.SetGameMode(currentGameModeIndex);
                //Give ship an index and color
                newShipController.GiveIndex(newShipIndex);
                newShipController.SetShipColor(newShipColor);
                #endregion
            }
            currentlyAliveShips.Add(newShip);

            ShipInfo newShipInfo = new ShipInfo();
            newShipInfo.shipIndex = newShipIndex;
            newShipInfo.shipPosition = spawnPoint.position;
            shipInfoManager.shipInfoList.Add(newShipInfo);
        }

        foreach (GameObject ship in currentlyAliveShips)
        {
            em.BroadcastShipReference(ship);
        }
        #endregion

        #region Instantiate PowerUps
        if (!powerUpsDisabled)
        {
            int powerUpBaseIndexCounter = 0;
            foreach (Transform position in powerUpPositions)
            {
                powerUpBaseIndexCounter++;
                GameObject newPowerUp = Instantiate(Resources.Load("PowerUps/PowerUpPlatform", typeof(GameObject)), position.position,
                    Quaternion.identity) as GameObject;
                currentlyExistingPowerUps.Add(newPowerUp);
                newPowerUp.GetComponent<PowerUpController>().SetPowerUpPlatformIndex(powerUpBaseIndexCounter);
            }
        }
        #endregion
    }
    #endregion

    #region Network game ship spawning
    //TODO: Finish implementing ship spawning from server
    private void SpawnShip(int shipIndex, int spawnPointIndex, int shipColorIndex, string ownerID)
    {
        Debug.Log("SpawnShip, ownerID: " + ownerID);
        Transform spawnPoint = FindSpawnPointWithIndex(spawnPointIndex);
        GameObject newShip = Instantiate(Resources.Load("Ships/Ship", typeof(GameObject)),
            spawnPoint.position, spawnPoint.rotation) as GameObject;
        ShipController newShipController;
        Color newShipColor = FindShipColorWithIndex(shipColorIndex);

        if (ownerID == em.BroadcastRequestMyNetworkID())
        {
            newShipController =
                newShip.AddComponent<LocalPlayerController>();
            GameObject newPlayerIndicator = Instantiate(Resources.Load("Effects/PlayerIndicator",
                typeof(GameObject)), newShip.transform.position, Quaternion.identity,
                newShip.transform) as GameObject;
            //Set playerIndicator color
            ParticleSystem.MainModule pIMain = newPlayerIndicator.GetComponentInChildren<ParticleSystem>().main;
            pIMain.startColor = new Color(newShipColor.r, newShipColor.g, newShipColor.b, 1);
            currentPlayerCamera = Instantiate(Resources.Load("Cameras/PlayerCamera",
                typeof(GameObject)), Vector3.zero, Quaternion.identity) as GameObject;
            CameraController currentCameraScript = currentPlayerCamera.GetComponentInChildren<CameraController>();
            currentCameraScript.SetTarget(newShip.transform);
            currentCameraScript.SetMyShipIndex(shipIndex);
            Debug.Log("Spawned ship with LocalPlayerController");
        }
        else
        {
            newShipController =
                newShip.AddComponent<NetworkPlayerController>();
            Debug.Log("Spawned ship with NetworkPlayerController");
        }
        //Set currentGameMode in shipController
        newShipController.SetGameMode(currentGameModeIndex);
        //Give ship an index and color
        newShipController.GiveIndex(shipIndex);
        newShipController.SetShipColor(newShipColor);

        currentlyAliveShips.Add(newShip);

        ShipInfo newShipInfo = new ShipInfo();
        newShipInfo.shipIndex = shipIndex;
        newShipInfo.shipPosition = spawnPoint.position;
        newShipInfo.ownerID = ownerID;
        shipInfoManager.shipInfoList.Add(newShipInfo);
    }
    #endregion
    #endregion

    #region SetVariables
    public void SetRespawnPoints(List<Transform> newRespawnPoints)
    {
        Debug.Log("SetRespawnPoints");
        respawnPoints = newRespawnPoints;
        readyToSpawnShips = true;

        if (currentGameModeIndex == gameModeNetworkMultiplayerIndex)
        {
            if (shipsToSpawn.Count > 0)
            {
                for (int i = 0; i < shipsToSpawn.Count; i++)
                {
                    SpawnShip(shipsToSpawn[i].shipIndex, shipsToSpawn[i].spawnPointIndex,
                        shipsToSpawn[i].shipColorIndex, shipsToSpawn[i].ownerID);
                    shipsToSpawn.RemoveAt(i);
                    i--;
                }
                em.BroadcastNetworkMultiplayerMatchInitialized();
                Debug.Log("GameManager: multiplayer match initialization broadcasted");
            }
        }
    }

    public void SetPowerUpPositions(List<Transform> newPowerUpPositions)
    {
        powerUpPositions = newPowerUpPositions;
    }

    public void SetNumberOfShips(int newNumberOfShips)
    {
        Debug.Log("SetNumberOfShips");
        numberOfShips = newNumberOfShips;
    }
    #endregion

    #region Find available ship color
    private Color FindShipColorWithIndex(int shipColorIndex)
    {
        if (resetUsedShipColors)
        {
            usedShipColors.Clear();
            resetUsedShipColors = false;
        }

        Color shipColor;
        shipColor = shipColorOptions[shipColorIndex];
        usedShipColors.Add(shipColorIndex);

        if (usedShipColors.Count == shipColorOptions.Count)
        {
            resetUsedShipColors = true;
        }

        return shipColor;
    }

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
    private Transform FindSpawnPointWithIndex(int spawnPointIndex)
    {
        if (resetUsedSpawnPointsList)
        {
            usedSpawnPoints.Clear();
            resetUsedSpawnPointsList = false;
        }

        Transform respawnPoint;
        respawnPoint = respawnPoints[spawnPointIndex];
        usedSpawnPoints.Add(spawnPointIndex);

        if (usedSpawnPoints.Count == respawnPoints.Count)
        {
            resetUsedSpawnPointsList = true;
        }

        return respawnPoint;
    }

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

    #region Update & FixedUpdate
    private void Update()
    {
        if (inGame)
        {
            #region HUD Timer
            if (matchStarted && (!isPaused || currentGameModeIndex == gameModeNetworkMultiplayerIndex))
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
            if (!isPaused || currentGameModeIndex == gameModeNetworkMultiplayerIndex)
            {
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
            }
            #endregion
        }

        if (waitingTillNextFrame)
        {
            WaitRestartingTillNextFrame();
        }
    }
    #endregion

    #region Waiting a frame to restart match
    private void WaitRestartingTillNextFrame()
    {
        if (nextFrame)
        {
            Debug.Log("Next frame; restarting");
            waitingTillNextFrame = false;
            nextFrame = false;

            //Respawn everything
            InitializeGame();
            StartMatchStartTimer();
            matchStarted = false;
        }
        else
        {
            Debug.Log("Waiting till next frame to restart");
            nextFrame = true;
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
