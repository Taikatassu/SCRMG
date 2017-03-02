using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_GameManager : MonoBehaviour {

    /* TODO:
     * - Different gameMode implementations
     */

    #region References & variables
    //References
    public static Core_GameManager instance; 

    Core_Toolbox toolbox;
    Core_EventManager em;
    Core_GlobalVariableLibrary lib;
    List<Transform> respawnPoints = new List<Transform>();
    //Variables coming from within the script
    List<int> usedSpawnPoints = new List<int>();
    List<int> usedShipColors = new List<int>();
    List<int> currentlyAliveShipIndices = new List<int>();
    bool resetUsedSpawnPointsList = false;
    bool resetUsedShipColors = false;
    bool matchStartTimerRunning = false;
    int fixedUpdateLoopCounter = -1;
    int fixedUpdateLoopsPerSecond = -1;
    int matchStartTimerValue = -1;
    int currentGameModeIndex = -1;

    //Variables coming from globalVariableLibrary
    List<Color> shipColorOptions = new List<Color>();
    int gameModeSingleplayerIndex = -1;
    int gameModeNetworkMultiplayerIndex = -1;
    int gameModeLocalMultiplayerIndex = -1;
    int numberOfShips; //Can also be set with the public "SetNumberOfShips()"-function
    int matchStartTimerLength = -1;
    //int sceneIndexMainMenu = -1;
    int sceneIndexLevel01 = -1;
    #endregion

    #region Initialization
    void Awake ()
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

        toolbox = FindObjectOfType<Core_Toolbox>();
        em = toolbox.GetComponent<Core_EventManager>();
        lib = toolbox.GetComponent<Core_GlobalVariableLibrary>();
        GetStats();
    }

    private void GetStats()
    {
        gameModeSingleplayerIndex = lib.gameSettingVariables.gameModeSingleplayerIndex;
        gameModeNetworkMultiplayerIndex = lib.gameSettingVariables.gameModeNetworkMultiplayerIndex;
        gameModeLocalMultiplayerIndex = lib.gameSettingVariables.gameModeLocalMultiplayerIndex;
        shipColorOptions = lib.shipVariables.shipColorOptions;
        matchStartTimerLength = lib.sceneVariables.matchStartTimerLength;
        numberOfShips = lib.sceneVariables.numberOfShips;
        //sceneIndexMainMenu = lib.sceneVariables.sceneIndexMainMenu;
        sceneIndexLevel01 = lib.sceneVariables.sceneIndexLevel01;
    }

    #region OnEnable & OnDisable
    private void OnEnable()
    {
        em.OnGameRestart += OnGameRestart;
        em.OnNewSceneLoaded += OnNewSceneLoaded;
        em.OnShipDead += OnShipDead;
        em.OnSetGameMode += OnSetGameMode;
    }

    private void OnDisable()
    {
        em.OnGameRestart -= OnGameRestart;
        em.OnNewSceneLoaded -= OnNewSceneLoaded;
        em.OnShipDead -= OnShipDead;
        em.OnSetGameMode -= OnSetGameMode;
    }
    #endregion

    #region Subscribers
    private void OnNewSceneLoaded(int sceneIndex)
    {
        //TODO: Remember to implement check for all future scenes
        if (sceneIndex == sceneIndexLevel01)
        {
            resetUsedShipColors = true;
            resetUsedSpawnPointsList = true;
            InitializeGame();
            StartMatchStartTimer();
        }
    }

    private void OnGameRestart()
    {
        //Reset all
        //Destroy player
        //Respawn everything
        resetUsedShipColors = true;
        resetUsedSpawnPointsList = true;
        InitializeGame();
        StartMatchStartTimer();
    }

    private void OnShipDead(int shipIndex)
    {
        if (currentlyAliveShipIndices.Count > 1)
        {
            currentlyAliveShipIndices.Remove(shipIndex);
            
            if (currentlyAliveShipIndices.Count == 1)
            {
                em.BroadcastGameEnd(currentlyAliveShipIndices[0]);               
            }
        }
    }

    private void OnSetGameMode(int newGameModeIndex)
    {
        currentGameModeIndex = newGameModeIndex;
    }
    #endregion
    #endregion

    #region Match initialization
    private void InitializeGame()
    {
        /*TODO: Have server tell level manager how many ships to spawn, and which ship is whose
        *   - Add AIPlayerController or NetworkPlayerController to other ships
        */
        #region Instantiate ships
        currentlyAliveShipIndices.Clear();
        for (int i = 0; i < numberOfShips; i++ )
        {
            Transform spawnPoint = FindAvailableSpawnPoint();
            GameObject newShip = Instantiate(Resources.Load("Ship", typeof(GameObject)),
                spawnPoint.position, spawnPoint.rotation) as GameObject;
            Core_ShipController newShipController;
            Color newShipColor = FindNewShipColor();

            if (currentGameModeIndex == gameModeSingleplayerIndex)
            {
                #region Singleplayer ship instantiating
                if (i == 0)
                {
                    newShipController =
                        newShip.AddComponent<Core_LocalPlayerController>();
                    GameObject newPlayerIndicator = Instantiate(Resources.Load("PlayerIndicator",
                        typeof(GameObject)), newShip.transform.position, Quaternion.identity,
                        newShip.transform) as GameObject;
                    //Set playerIndicator color
                    ParticleSystem.MainModule pIMain = newPlayerIndicator.GetComponentInChildren<ParticleSystem>().main;
                    pIMain.startColor = new Color(newShipColor.r, newShipColor.g, newShipColor.b, 1);
                    GameObject newPlayerCamera = Instantiate(Resources.Load("PlayerCamera",
                        typeof(GameObject)), Vector3.zero, Quaternion.identity) as GameObject;
                    Core_CameraController newCameraScript = newPlayerCamera.GetComponentInChildren<Core_CameraController>();
                    newCameraScript.SetTarget(newShip.transform);
                    newCameraScript.SetMyShipIndex(i + 1);
                }
                else
                {
                    newShipController =
                        newShip.AddComponent<Core_AIPlayerController>();
                }
                //Set currentGameMode in shipController
                newShipController.SetGameMode(currentGameModeIndex);
                //Give ship an index and color
                newShipController.GiveIndex(i + 1);
                newShipController.SetShipColor(newShipColor);
                #endregion
            }
            else if (currentGameModeIndex == gameModeNetworkMultiplayerIndex)
            {
                #region NetworkMultiplayer ship instantiating
                if (i == 0)
                {
                    newShipController =
                           newShip.AddComponent<Core_LocalPlayerController>();
                    GameObject newPlayerIndicator = Instantiate(Resources.Load("PlayerIndicator",
                        typeof(GameObject)), newShip.transform.position, Quaternion.identity,
                        newShip.transform) as GameObject;
                    //Set playerIndicator color
                    ParticleSystem.MainModule pIMain = newPlayerIndicator.GetComponentInChildren<ParticleSystem>().main;
                    pIMain.startColor = new Color(newShipColor.r, newShipColor.g, newShipColor.b, 1);
                    GameObject newPlayerCamera = Instantiate(Resources.Load("PlayerCamera",
                        typeof(GameObject)), Vector3.zero, Quaternion.identity) as GameObject;
                    Core_CameraController newCameraScript = newPlayerCamera.GetComponentInChildren<Core_CameraController>();
                    newCameraScript.SetTarget(newShip.transform);
                    newCameraScript.SetMyShipIndex(i + 1);
                }
                else
                {
                    newShipController =
                        newShip.AddComponent<Core_NetworkPlayerController>();
                }
                //Set currentGameMode in shipController
                newShipController.SetGameMode(currentGameModeIndex);
                //Give ship an index and color
                newShipController.GiveIndex(i + 1);
                newShipController.SetShipColor(newShipColor);
                #endregion
            }
            else if (currentGameModeIndex == gameModeLocalMultiplayerIndex)
            {
                #region LocalMultiplayer ship instantiating [WIP]
                //TODO: Below is completely WIP
                newShipController =
                        newShip.AddComponent<Core_LocalPlayerController>();
                GameObject newPlayerIndicator = Instantiate(Resources.Load("PlayerIndicator",
                    typeof(GameObject)), newShip.transform.position, Quaternion.identity,
                    newShip.transform) as GameObject;
                //Set playerIndicator color
                ParticleSystem.MainModule pIMain = newPlayerIndicator.GetComponentInChildren<ParticleSystem>().main;
                pIMain.startColor = new Color(newShipColor.r, newShipColor.g, newShipColor.b, 1);
                GameObject newPlayerCamera = Instantiate(Resources.Load("PlayerCamera",
                    typeof(GameObject)), Vector3.zero, Quaternion.identity) as GameObject;
                Core_CameraController newCameraScript = newPlayerCamera.GetComponentInChildren<Core_CameraController>();
                newCameraScript.SetTarget(newShip.transform);
                newCameraScript.SetMyShipIndex(i + 1);

                //Set currentGameMode in shipController
                newShipController.SetGameMode(currentGameModeIndex);
                //Give ship an index and color
                newShipController.GiveIndex(i + 1);
                newShipController.SetShipColor(newShipColor);
                #endregion
            }
            em.BroadcastShipReference(newShip);
            currentlyAliveShipIndices.Add(i + 1);
        }
        #endregion
    }
    #endregion

    #region SetVariables
    public void SetRespawnPoints(List<Transform> newRespawnPoints)
    {
        respawnPoints = newRespawnPoints;
    }

    public void SetNumberOfShips(int newNumberOfShips)
    {
        Debug.Log("SetNumberOfShips");
        numberOfShips = newNumberOfShips;
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
            Debug.Log("Core_LevelManager: Inside FindNewShipColor while loop");
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
            Debug.Log("Core_LevelManager: Inside FindInitialSpawnPoint while loop");
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

    #region FixedUpdate
    private void FixedUpdate()
    {
        #region MatchStartTimer
        if (matchStartTimerRunning)
        {
            fixedUpdateLoopCounter++;
            if (fixedUpdateLoopCounter >= fixedUpdateLoopsPerSecond)
            {
                matchStartTimerValue--;
                em.BroadcastMatchStartTimerValue(matchStartTimerValue);
                if (matchStartTimerValue <= 0)
                {
                    matchStartTimerRunning = false;
                }
                fixedUpdateLoopCounter = 0;
            }
        }
        #endregion
    }
    #endregion

    #region MatchStartTimer initialization
    public void StartMatchStartTimer()
    {
        //StartCoroutine(BroadcastAndDecreaseMatchStartTimer(matchStartTimerLength));
        matchStartTimerRunning = true;
        fixedUpdateLoopsPerSecond = Mathf.RoundToInt(1 / Time.fixedDeltaTime);
        matchStartTimerValue = matchStartTimerLength;
        fixedUpdateLoopCounter = 0;
        em.BroadcastMatchStartTimerValue(matchStartTimerValue);
    }  
    #endregion

}
