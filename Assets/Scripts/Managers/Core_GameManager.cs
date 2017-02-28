using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core_GameManager : MonoBehaviour {

    /* TODO: Game loop!
     *      Victory and loss state -> match end -> restart or return to main menu
     *      
     * Pause menu
     *      If in singleplayer, pause game
     *      If in multiplayer, display "in pause menu" icon above the player's head
     *      Options: "Resume", "Restart" and "Return to main menu"
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
    bool resetUsedSpawnPointsList = false;
    bool resetUsedShipColors = false;
    bool resetMatchTimer = false;
    int matchBeginTimer = 0;

    //Variables coming from globalVariableLibrary
    List<Color> shipColorOptions = new List<Color>();
    int numberOfShips; //Can also be set with the public "SetNumberOfShips()"-function
    int matchBeginTimerLength = -1;
    int sceneIndexMainMenu = -1;
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
        //shipColorOptions.Add(Color.magenta);
        //shipColorOptions.Add(Color.cyan);
        //shipColorOptions.Add(Color.yellow);
        //shipColorOptions.Add(Color.green);
        //shipColorOptions.Add(Color.black);
        //shipColorOptions.Add(Color.white);
        GetStats();
    }

    private void GetStats()
    {
        shipColorOptions = lib.shipVariables.shipColorOptions;
        matchBeginTimerLength = lib.sceneVariables.matchBeginTimerLength;
        numberOfShips = lib.sceneVariables.numberOfShips;
        sceneIndexMainMenu = lib.sceneVariables.sceneIndexMainMenu;
        sceneIndexLevel01 = lib.sceneVariables.sceneIndexLevel01;
    }

    #region OnEnable & OnDisable
    private void OnEnable()
    {
        em.OnGameRestart += OnGameRestart;
        em.OnNewSceneLoaded += OnNewSceneLoaded;
    }

    private void OnDisable()
    {
        em.OnGameRestart -= OnGameRestart;
        em.OnNewSceneLoaded -= OnNewSceneLoaded;
    }
    #endregion

    #region Subscribers
    private void OnNewSceneLoaded(int sceneIndex)
    {
        Debug.Log("GameManager: OnNewSceneLoaded with index: " + sceneIndex);
        //TODO: Remember to implement check for all future scenes
        if (sceneIndex == sceneIndexLevel01)
        {
            InitializeGame();
        }
    }

    private void OnGameRestart()
    {
        Debug.Log("GameManager: OnGameRestart");
        //Reset all
        //Destroy player
        //Respawn everything
        resetMatchTimer = true;
        resetUsedShipColors = true;
        resetUsedSpawnPointsList = true;
        InitializeGame();
        StartMatchBeginTimer();
    }
    #endregion
    #endregion

    #region InitializeGame
    public void InitializeGame()
    {
        /*TODO: Have server tell level manager how many ships to spawn, and which ship is whose
        *   - Add AIPlayerController or NetworkPlayerController to other ships
        */
        #region Instantiate ships
        for (int i = 0; i < numberOfShips; i++ )
        {
            Transform spawnPoint = FindAvailableSpawnPoint();
            GameObject newShip = Instantiate(Resources.Load("Ship", typeof(GameObject)),
                spawnPoint.position, spawnPoint.rotation) as GameObject;
            Core_ShipController newShipController;
            Color newShipColor = FindNewShipColor();

            if (i == 0)
            {
                newShipController = 
                    newShip.AddComponent<Core_LocalPlayerController>();
                GameObject newPlayerIndicator = Instantiate(Resources.Load("PlayerIndicator", 
                    typeof(GameObject)), newShip.transform.position, Quaternion.identity, 
                    newShip.transform) as GameObject;

                ParticleSystem.MainModule pIMain = newPlayerIndicator.GetComponentInChildren<ParticleSystem>().main;
                pIMain.startColor = new Color(newShipColor.r, newShipColor.g, newShipColor.b, 1);

                GameObject newPlayerCamera = Instantiate(Resources.Load("PlayerCamera",
                    typeof(GameObject)), Vector3.zero, Quaternion.identity) as GameObject;
                Core_CameraController newCameraScript = newPlayerCamera.GetComponentInChildren<Core_CameraController>();
                newCameraScript.SetTarget(newShip.transform);
                newCameraScript.SetMyShipIndex(i + 1);
            }
            else //TODO: Add a check whether starting a AI or Online match, 
                        //and add corresponding playerControllers to other ships
            {
                newShipController =
                    newShip.AddComponent<Core_AIPlayerController>();
            }
            newShipController.GiveIndex(i + 1);
            newShipController.SetShipColor(newShipColor);
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
            Debug.Log("All available color used, resetting list");
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

    #region Match beginning
    public void StartMatchBeginTimer()
    {
        StartCoroutine(BroadcastAndDecreaseMatchBeginTimer(matchBeginTimerLength));
    }

    IEnumerator BroadcastAndDecreaseMatchBeginTimer(int timerLength)
    {
        // TODO: Fix this timer's functionality
        // Find a way to stop this timer instantly and completely when the game is restarted
        matchBeginTimer = timerLength;
        em.BroadcastMatchBeginTimerValue(matchBeginTimer);
        for (int i = 0; i < timerLength; i++)
        {
            Debug.Log("in for-loop. i: " + i);
            yield return new WaitForSeconds(1);
            if (!resetMatchTimer)
            {
                matchBeginTimer--;
                if (matchBeginTimer >= 0)
                    em.BroadcastMatchBeginTimerValue(matchBeginTimer);
            }
        }
        resetMatchTimer = false;
    }
    #endregion

}
