using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using Mono.Data.Sqlite;

public class Database : MonoBehaviour
{
    Toolbox toolbox;
    EventManager em;
    GlobalVariableLibrary lib;
    List<PlayerData> currentPlayers = new List<PlayerData>();
    MatchData currentMatchData;
    int currentMatchID = -1;


    int sceneIndexMainMenu = -1;
    int sceneIndexLevel01 = -1;
    int gameModeSingleplayerIndex = 0;
    //int gameModeNetworkMultiplayerIndex = 1;
    //int gameModeLocalMultiplayerIndex = 2;

    private void Awake()
    {
        toolbox = FindObjectOfType<Toolbox>();
        em = toolbox.GetComponent<EventManager>();
        lib = toolbox.GetComponent<GlobalVariableLibrary>();
    }

    private void Start()
    {
        //TODO: Open connection to database. If no database was found / database was empty, create new database

        #region SQLite stuff
        string conn = "URI=file:" + Application.dataPath + "/SCRMG_Database.db";


        IDbConnection dbconn;
        dbconn = (IDbConnection)new SqliteConnection(conn);
        dbconn.Open();
        IDbCommand dbcmd = dbconn.CreateCommand();

        string sqlQuery = "SELECT value, name, randomSequence " + "from PlaceSequence";
        dbcmd.CommandText = sqlQuery;
        IDataReader reader = dbcmd.ExecuteReader();

        while (reader.Read())
        {
            int value = reader.GetInt32(0);
            string name = reader.GetString(1);
            int rand = reader.GetInt32(2);

            Debug.Log("value = " + value + ", name = " + name + ", random = " + rand);
        }

        reader.Close();
        reader = null;
        dbcmd.Dispose();
        dbcmd = null;
        dbconn.Dispose();
        dbconn = null;
        #endregion
    }

    private void GetStats()
    {
        sceneIndexMainMenu = lib.sceneVariables.sceneIndexMainMenu;
        sceneIndexLevel01 = lib.sceneVariables.sceneIndexLevel01;
        gameModeSingleplayerIndex = lib.gameSettingVariables.gameModeSingleplayerIndex;
        //gameModeNetworkMultiplayerIndex = lib.gameSettingVariables.gameModeNetworkMultiplayerIndex;
        //gameModeLocalMultiplayerIndex = lib.gameSettingVariables.gameModeLocalMultiplayerIndex;
    }

    private void OnEnable()
    {
        em.OnSetGameMode += OnSetGameMode;
    }

    private void OnDisable()
    {
        em.OnSetGameMode -= OnSetGameMode;
    }

    private void SubscribeToDataEvents()
    {
        em.OnNewSceneLoading += OnNewSceneLoading;
        em.OnShipReference += OnShipReference;
        em.OnPowerUpPickedUp += OnPowerUpPickedUp;
        em.OnProjectileSpawned += OnProjectileSpawned;
        em.OnProjectileDestroyed += OnProjectileDestroyed;
        em.OnShipDead += OnShipDead;
        em.OnMatchEnded += OnMatchEnded;
    }

    private void UnSubscribeFromDataEvents()
    {
        em.OnNewSceneLoading -= OnNewSceneLoading;
        em.OnShipReference -= OnShipReference;
        em.OnPowerUpPickedUp -= OnPowerUpPickedUp;
        em.OnProjectileSpawned -= OnProjectileSpawned;
        em.OnProjectileDestroyed -= OnProjectileDestroyed;
        em.OnShipDead -= OnShipDead;
        em.OnMatchEnded -= OnMatchEnded;
    }

    private int CreateNewMatchIndex()
    {
        //TODO: Read all match indices from the database and create one that is not already in use (lastMatchIndex + 1)
        return 1;
    }

    private void StoreMatchData()
    {
        //TODO: Implement this
    }

    private void StorePlayerData()
    {
        //TODO: Implement this
    }

    private PlayerData FindPlayerDataWithShipIndex(int shipIndex)
    {
        PlayerData myPlayerData;
        if (currentPlayers[shipIndex - 1].playerShipIndex == shipIndex)
        {
            myPlayerData = currentPlayers[shipIndex - 1];
            return myPlayerData;
        }
        else
        {
            for (int i = 0; i < currentPlayers.Count; i++)
            {
                if (currentPlayers[i].playerShipIndex == shipIndex)
                {
                    myPlayerData = currentPlayers[i];
                    return myPlayerData;
                }
            }
        }
        return new PlayerData(-1, -1);
    }

    #region Subscribers
    private void OnSetGameMode(int newGameModeIndex)
    {
        if (newGameModeIndex == gameModeSingleplayerIndex)
        {
            SubscribeToDataEvents();
            currentMatchID = CreateNewMatchIndex();

            currentMatchData = new MatchData(currentMatchID, newGameModeIndex);
        }
        else
        {
            UnSubscribeFromDataEvents();
        }
    }

    private void OnNewSceneLoading(int sceneIndex)
    {
        if (sceneIndex == sceneIndexLevel01)
        {
            //Get ready to start recording playerData and matchData
            //currentMatchID = CreateNewMatchIndex();
        }
        else if (sceneIndex == sceneIndexMainMenu)
        {
            UnSubscribeFromDataEvents();
        }
    }

    private void OnShipReference(GameObject newShip)
    {
        int newShipIndex = newShip.GetComponent<ShipController>().GetIndex();
        #region PlayerData
        currentPlayers.Add(new PlayerData(newShipIndex, currentMatchID));
        bool isHumanControlled = false;
        if (newShipIndex == 1)
        {
            isHumanControlled = true;
        }
        #endregion

        #region MatchData
        currentMatchData.playersAndIfHumanControlled.Add(isHumanControlled);
        #endregion
    }

    private void OnPowerUpPickedUp(int shipIndex, int powerUpBaseIndex, int powerUpType)
    {
        #region PlayerData
        PlayerData myPlayerData = FindPlayerDataWithShipIndex(shipIndex);
        if (myPlayerData.playerShipIndex == -1)
        {
            Debug.LogError("PlayerData could not be found!");
        }
        else
        {
            if (powerUpType == 1)
            {
                myPlayerData.timesPickedUpPowerUpOne++;
            }
            else if (powerUpType == 2)
            {
                myPlayerData.timesPickedUpPowerUpTwo++;
            }
            else if (powerUpType == 3)
            {
                myPlayerData.timesPickedUpPowerUpThree++;
            }
        }
        #endregion

        #region MatchData
        currentMatchData.powerUpsPickedUpOverall++;

        if (powerUpBaseIndex == 1)
        {
            currentMatchData.powerUpPlatformOneUsed++;
        }
        else if (powerUpBaseIndex == 2)
        {
            currentMatchData.powerUpPlatformTwoUsed++;
        }
        else if (powerUpBaseIndex == 3)
        {
            currentMatchData.powerUpPlatformThreeUsed++;
        }
        else if (powerUpBaseIndex == 4)
        {
            currentMatchData.powerUpPlatformFourUsed++;
        }
        #endregion
    }

    private void OnProjectileSpawned(int projectileOwnerIndex, int projectileIndex, int projectileType, Vector3 spawnPosition, Vector3 spawnRotation, bool isControlledByServer)
    {
        PlayerData myPlayerData = FindPlayerDataWithShipIndex(projectileOwnerIndex);
        ProjectileInfo newProjectileInfo = new ProjectileInfo();
        newProjectileInfo.projectileOwnerIndex = projectileOwnerIndex;
        newProjectileInfo.projectileIndex = projectileIndex;
        newProjectileInfo.projectileType = projectileType;

        myPlayerData.spawnedProjectiles.Add(newProjectileInfo);

        if (projectileType == 0)
        {
            myPlayerData.projectileTypeZeroSpawns++;
        }
        else if (projectileType == 1)
        {
            myPlayerData.projectileTypeOneSpawns++;
        }
        else if (projectileType == 2)
        {
            myPlayerData.projectileTypeTwopawns++;
        }
        else if (projectileType == 3)
        {
            myPlayerData.projectileTypeThreeSpawns++;
        }
    }

    private void OnProjectileDestroyed(int projectileOwnerIndex, int projectileIndex, Vector3 location, bool hitShip)
    {
        PlayerData myPlayerData = FindPlayerDataWithShipIndex(projectileOwnerIndex);
        ProjectileInfo myProjectile = new ProjectileInfo();
        int projectileElement = -1;
        for (int i = 0; i < myPlayerData.spawnedProjectiles.Count; i++)
        {
            if (myPlayerData.spawnedProjectiles[i].projectileIndex == projectileIndex)
            {
                myProjectile = myPlayerData.spawnedProjectiles[i];
                projectileElement = i;
                break;
            }
        }

        if (projectileElement == -1)
        {
            Debug.LogError("Database: ProjectileNotFoundWithIndex!");
        }
        else
        {
            if (myProjectile.projectileType == 0)
            {
                myPlayerData.projectileTypeZeroHits++;
            }
            else if (myProjectile.projectileType == 1)
            {
                myPlayerData.projectileTypeOneHits++;
            }
            else if (myProjectile.projectileType == 2)
            {
                myPlayerData.projectileTypeTwoHits++;
            }
            else if (myProjectile.projectileType == 3)
            {
                myPlayerData.projectileTypeThreeHits++;
            }

            myPlayerData.spawnedProjectiles.RemoveAt(projectileElement);
        }
    }

    private void OnShipDead(int shipIndex, int killerIndex, float lifetime)
    {
        PlayerData myPlayerData = FindPlayerDataWithShipIndex(shipIndex);
        myPlayerData.lifetime = lifetime;
    }

    private void OnMatchEnded(int winnerIndex, float matchLength)
    {
        #region PlayerData
        foreach (PlayerData playerData in currentPlayers)
        {
            if (playerData.lifetime == -1)
            {
                playerData.lifetime = matchLength;
            }

            if (playerData.playerShipIndex == winnerIndex)
            {
                playerData.victory = true;
            }
        }
        #endregion

        #region MatchData
        currentMatchData.matchLength = matchLength;
        if(winnerIndex == 1)
        {
            currentMatchData.humanPlayerWon = true;
        }
        else
        {
            currentMatchData.humanPlayerWon = false;
        }
        #endregion

        //TODO: Store all collected data to database
        StoreMatchData();
        StorePlayerData();
    }
    #endregion
}
