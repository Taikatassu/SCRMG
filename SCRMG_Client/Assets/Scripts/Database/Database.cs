using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using Mono.Data.Sqlite;

public class Database : MonoBehaviour
{
    //TODO: 
    // - Create event for calling various sql queries 
    //      - Player lifetime stats (accuracy per projectile type and combined, lifetime, win/loss ratio etc.)
    //      - Last match stats (winner, powerUps used, match length, etc.)
    //      - Overall match stats (playerWins / AIWins, powerUp platform frequency, average match length, etc.)

    Toolbox toolbox;
    EventManager em;
    GlobalVariableLibrary lib;
    List<PlayerData> currentPlayers = new List<PlayerData>();
    List<PlayerData> lastPlayers = new List<PlayerData>();
    MatchData currentMatchData;
    MatchData lastMatchData;
    int currentMatchID = -1;
    int currentGameModeIndex = -1;

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

    private void StoreDataInDatabase()
    {
        string conn = "URI=file:" + Application.dataPath + "/SCRMG_Database.db";

        IDbConnection dbconn;
        dbconn = (IDbConnection)new SqliteConnection(conn);
        IDbCommand dbcmd = dbconn.CreateCommand();
        IDataReader reader;
        string sqlQuery;
        dbconn.Open();

        #region Storing matchData
        sqlQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='MatchData' ";
        dbcmd.CommandText = sqlQuery;
        reader = dbcmd.ExecuteReader();

        if (!reader.Read())
        {
            try
            {
                reader.Close();
                reader = null;
                sqlQuery = "CREATE TABLE MatchData (PK INTEGER PRIMARY KEY, matchID INT, startDate CHAR,"
                    + " startTime CHAR, matchLength REAL, gameMode INT, humanPlayerWon INT, powerUpsOverall INT,"
                    + " powerUpBase1 INT, powerUpBase2 INT, powerUpBase3 INT, powerUpBase4 INT)";
                dbcmd.CommandText = sqlQuery;
                dbcmd.ExecuteNonQuery();
            }
            catch
            {
                Debug.LogWarning("Error while trying to create new table");
            }
        }

        if (reader != null)
        {
            reader.Close();
            reader = null;
        }

        int myMatchID = lastMatchData.matchID;
        string myStartDate = lastMatchData.startDate;
        string myStartTime = lastMatchData.startTime;
        float myMatchLength = lastMatchData.matchLength;
        int myGameMode = lastMatchData.gameMode;
        int myHumanPlayerWon = 0;
        if (lastMatchData.humanPlayerWon)
        {
            myHumanPlayerWon = 1;
        }

        int myPowerUpsPickedUpOverall = lastMatchData.powerUpsPickedUpOverall;
        int myPowerUpPlatformOneUsed = lastMatchData.powerUpPlatformOneUsed;
        int myPowerUpPlatformTwoUsed = lastMatchData.powerUpPlatformTwoUsed;
        int myPowerUpPlatformThreeUsed = lastMatchData.powerUpPlatformThreeUsed;
        int myPowerUpPlatformFourUsed = lastMatchData.powerUpPlatformFourUsed;

        try
        {
            sqlQuery = "INSERT INTO MatchData (matchID, startDate, startTime, matchLength, gameMode, humanPlayerWon, powerUpsOverall, "
                        + "powerUpBase1, powerUpBase2, powerUpBase3, powerUpBase4) "
                        + "VALUES (" + myMatchID + ", '" + myStartDate + "', '" + myStartTime + "', " + myMatchLength + ", " + myGameMode + ", "
                        + myHumanPlayerWon + ", " + myPowerUpsPickedUpOverall + ", " + myPowerUpPlatformOneUsed + ", " + myPowerUpPlatformTwoUsed
                        + ", " + myPowerUpPlatformThreeUsed + ", " + myPowerUpPlatformFourUsed + ")";

            dbcmd.CommandText = sqlQuery;
            dbcmd.ExecuteNonQuery();
        }
        catch
        {
            Debug.LogWarning("Error while writing to database");
        }

        if (reader != null)
        {
            reader.Close();
            reader = null;
        }
        
        #endregion

        #region Storing PlayerData
        sqlQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='PlayerData' ";
        dbcmd.CommandText = sqlQuery;
        reader = dbcmd.ExecuteReader();

        if (!reader.Read())
        {
            Debug.Log("PlayerData table not found, creating table");

            reader.Close();
            reader = null;
            sqlQuery = "CREATE TABLE PlayerData (PK INTEGER PRIMARY KEY, shipIndex INT, matchID INT, lifetime REAL,"
                + " victory INT, projectileType1Spawns INT, projectileType2Spawns INT, projectileType3Spawns INT,"
                + " projectileType4Spawns INT, projectileType1Hits INT, projectileType2Hits INT, projectileType3Hits INT,"
                + " projectileType4Hits INT, powerUp1PickUps INT, powerUp2PickUps INT, powerUp3PickUps INT)";
            dbcmd.CommandText = sqlQuery;
            dbcmd.ExecuteNonQuery();

            //try
            //{
            //}
            //catch
            //{
            //    Debug.Log("Error while trying to create new PlayerData table");
            //}
        }

        if (reader != null)
        {
            reader.Close();
            reader = null;
        }
        
        foreach (PlayerData playerData in lastPlayers)
        {
            int myShipIndex = playerData.playerShipIndex;
            int myPlayerMatchID = playerData.matchID;
            float myLifetime = playerData.lifetime;

            int myVictory = 0;
            if (playerData.victory)
            {
                myVictory = 1;
            }

            int myProjectileTypeZeroSpawns = playerData.projectileTypeZeroSpawns;
            int myProjectileTypeOneSpawns = playerData.projectileTypeOneSpawns;
            int myProjectileTypeTwopawns = playerData.projectileTypeTwopawns;
            int myProjectileTypeThreeSpawns = playerData.projectileTypeThreeSpawns;

            int myProjectileTypeZeroHits = playerData.projectileTypeZeroHits;
            int myProjectileTypeOneHits = playerData.projectileTypeOneHits;
            int myProjectileTypeTwoHits = playerData.projectileTypeTwoHits;
            int myProjectileTypeThreeHits = playerData.projectileTypeThreeHits;

            int myTimesPickedUpPowerUpOne = playerData.timesPickedUpPowerUpOne;
            int myTimesPickedUpPowerUpTwo = playerData.timesPickedUpPowerUpTwo;
            int myTimesPickedUpPowerUpThree = playerData.timesPickedUpPowerUpThree;

            try
            {
                sqlQuery = "INSERT INTO PlayerData (shipIndex, matchID, lifetime, victory, projectileType1Spawns,"
                    + " projectileType2Spawns, projectileType3Spawns, projectileType4Spawns, projectileType1Hits,"
                    + " projectileType2Hits, projectileType3Hits, projectileType4Hits, powerUp1PickUps,"
                    + " powerUp2PickUps, powerUp3PickUps) VALUES (" + myShipIndex + ", " + myPlayerMatchID
                    + ", " + myLifetime + ", " + myVictory + ", " + myProjectileTypeZeroSpawns + ", " + myProjectileTypeOneSpawns
                    + ", " + myProjectileTypeTwopawns + ", " + myProjectileTypeThreeSpawns + ", " + myProjectileTypeZeroHits
                    + ", " + myProjectileTypeOneHits + ", " + myProjectileTypeTwoHits + ", " + myProjectileTypeThreeHits
                    + ", " + myTimesPickedUpPowerUpOne + ", " + myTimesPickedUpPowerUpTwo + ", " + myTimesPickedUpPowerUpThree + ")";

                dbcmd.CommandText = sqlQuery;
                dbcmd.ExecuteNonQuery();
            }
            catch
            {
                Debug.LogWarning("Error while writing to database");
            }

            if (reader != null)
            {
                reader.Close();
                reader = null;
            }
        }
        #endregion

        if (reader != null)
        {
            reader.Close();
            reader = null;
        }
        dbcmd.Dispose();
        dbcmd = null;
        dbconn.Dispose();
        dbconn = null;

        lastMatchData = null;
        lastPlayers.Clear();
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
        em.OnGameRestart += OnGameRestart;
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
        em.OnGameRestart -= OnGameRestart;
    }

    #region Creating new MatchIndex
    private int CreateNewMatchIndex()
    {
        int newMatchIndex = -1;

        string conn = "URI=file:" + Application.dataPath + "/SCRMG_Database.db";

        IDbConnection dbconn;
        dbconn = (IDbConnection)new SqliteConnection(conn);
        IDbCommand dbcmd = dbconn.CreateCommand();
        string sqlQuery;
        dbconn.Open();

        sqlQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='MatchData' ";
        dbcmd.CommandText = sqlQuery;
        IDataReader reader = dbcmd.ExecuteReader();

        if (!reader.Read())
        {
            Debug.Log("Table not found, starting MatchIndices from one");
            reader.Close();
            reader = null;
            newMatchIndex = 1;
        }
        else
        {
            reader.Close();
            reader = null;
            sqlQuery = "SELECT matchID FROM MatchData ORDER BY matchID DESC LIMIT 1;";
            dbcmd.CommandText = sqlQuery;
            reader = dbcmd.ExecuteReader();

            if (!reader.Read())
            {
                reader.Close();
                reader = null;
                newMatchIndex = 1;
            }
            else if (reader.GetInt32(0) == 0)
            {
                reader.Close();
                reader = null;
                newMatchIndex = 1;
            }
            else
            {
                newMatchIndex = reader.GetInt32(0) + 1;
                reader.Close();
                reader = null;
            }
        }

        if (reader != null)
        {
            reader.Close();
            reader = null;
        }
        dbcmd.Dispose();
        dbcmd = null;
        dbconn.Dispose();
        dbconn = null;
        
        return newMatchIndex;
    }
    #endregion
    
    private PlayerData FindPlayerDataWithShipIndex(int shipIndex)
    {
        PlayerData myPlayerData;
        if (currentPlayers.Count > 0)
        {
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
        }
        return new PlayerData(-1, -1);
    }

    #region Subscribers
    private void OnSetGameMode(int newGameModeIndex)
    {
        currentGameModeIndex = newGameModeIndex;

        if (currentGameModeIndex == gameModeSingleplayerIndex)
        {
            UnSubscribeFromDataEvents();
            SubscribeToDataEvents();

            currentMatchData = null;
            currentPlayers.Clear();
            currentMatchID = CreateNewMatchIndex();
            currentMatchData = new MatchData(currentMatchID, currentGameModeIndex);
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

        }
        else if (sceneIndex == sceneIndexMainMenu)
        {
            UnSubscribeFromDataEvents();

            currentMatchData = null;
            currentPlayers.Clear();
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
        if (myPlayerData.playerShipIndex != -1)
        {
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
                if (hitShip)
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
                }

                myPlayerData.spawnedProjectiles.RemoveAt(projectileElement);
            }
        }
        else
        {
            Debug.LogWarning("Database.OnProjectileDestroyed: Player not found with projectileOwnerIndex!");
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
        if (winnerIndex == 1)
        {
            currentMatchData.humanPlayerWon = true;
        }
        else
        {
            currentMatchData.humanPlayerWon = false;
        }
        #endregion
        
        lastMatchData = currentMatchData;
        currentMatchData = null;
        lastPlayers.Clear();

        foreach (PlayerData playerData in currentPlayers)
        {
            lastPlayers.Add(playerData);
        }

        currentPlayers.Clear();

        StoreDataInDatabase();
    }

    private void OnGameRestart()
    {
        currentMatchData = null;
        currentPlayers.Clear();

        currentMatchData = new MatchData(currentMatchID, currentGameModeIndex);
    }
    #endregion
}
