using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using Mono.Data.Sqlite;

public class Database : MonoBehaviour
{
    #region References & variables
    Toolbox toolbox;
    EventManager em;
    GlobalVariableLibrary lib;
    List<PlayerData> currentPlayers = new List<PlayerData>();
    List<PlayerData> lastPlayers = new List<PlayerData>();
    MatchData currentMatchData;
    MatchData lastMatchData;
    int currentMatchID = -1;
    int currentGameModeIndex = -1;
    //Variables coming from GlobalVariableLibrary
    int sceneIndexMainMenu = -1;
    int sceneIndexLevel01 = -1;
    int gameModeSingleplayerIndex = 0;
    #endregion

    #region Awake & GetStats
    private void Awake()
    {
        toolbox = FindObjectOfType<Toolbox>();
        em = toolbox.GetComponent<EventManager>();
        lib = toolbox.GetComponent<GlobalVariableLibrary>();
    }

    private void GetStats()
    {
        sceneIndexMainMenu = lib.sceneVariables.sceneIndexMainMenu;
        sceneIndexLevel01 = lib.sceneVariables.sceneIndexLevel01;
        gameModeSingleplayerIndex = lib.gameSettingVariables.gameModeSingleplayerIndex;
    }
    #endregion

    #region OnEnable & OnDisable
    private void OnEnable()
    {
        em.OnSetGameMode += OnSetGameMode;
        em.OnRequestDataFromDatabase += OnRequestDataFromDatabase;
    }

    private void OnDisable()
    {
        em.OnSetGameMode -= OnSetGameMode;
        em.OnRequestDataFromDatabase -= OnRequestDataFromDatabase;
    }
    #endregion

    #region Subscribing and unsubscribing to data events
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
    #endregion

    #region Subscribers
    private void OnRequestDataFromDatabase(int requestType)
    {
        Debug.Log("Database: OnRequestDataFromDatabase");
        switch (requestType)
        {
            case 0:
                ReadGameOverallDataFromDatabase();
                break;
            case 1:
                ReadPlayerLastMatchDataFromDatabase();
                break;
            case 2:
                ReadPlayerOverallDataFromDatabase();
                break;
            case 3:
                ReadMatchLastMatchDataFromDatabase();
                break;
            case 4:
                ReadMatchOverallDataFromDatabase();
                break;
        }
    }

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

    private void OnMatchEnded(int winnerIndex, float matchDuration)
    {
        #region PlayerData
        foreach (PlayerData playerData in currentPlayers)
        {
            if (playerData.lifetime == -1)
            {
                playerData.lifetime = matchDuration;
            }

            if (playerData.playerShipIndex == winnerIndex)
            {
                playerData.victory = true;
            }
        }
        #endregion

        #region MatchData
        currentMatchData.matchDuration = matchDuration;
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

    #region Finding playerData with ship index
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
    #endregion

    #region Storing data in database
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
                    + " startTime CHAR, matchDuration REAL, gameMode INT, humanPlayerWon INT, powerUpsOverall INT,"
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
        float myMatchDuration = lastMatchData.matchDuration;
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
            sqlQuery = "INSERT INTO MatchData (matchID, startDate, startTime, matchDuration, gameMode, humanPlayerWon, powerUpsOverall, "
                        + "powerUpBase1, powerUpBase2, powerUpBase3, powerUpBase4) "
                        + "VALUES (" + myMatchID + ", '" + myStartDate + "', '" + myStartTime + "', " + myMatchDuration + ", " + myGameMode + ", "
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
            try
            {
                reader.Close();
                reader = null;
                sqlQuery = "CREATE TABLE PlayerData (PK INTEGER PRIMARY KEY, shipIndex INT, matchID INT, lifetime REAL,"
                    + " victory INT, projectileType0Spawns INT, projectileType1Spawns INT, projectileType2Spawns INT,"
                    + " projectileType3Spawns INT, projectileType0Hits INT, projectileType1Hits INT, projectileType2Hits INT,"
                    + " projectileType3Hits INT, powerUp1PickUps INT, powerUp2PickUps INT, powerUp3PickUps INT)";
                dbcmd.CommandText = sqlQuery;
                dbcmd.ExecuteNonQuery();
            }
            catch
            {
                Debug.Log("Error while trying to create new PlayerData table");
            }
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
                sqlQuery = "INSERT INTO PlayerData (shipIndex, matchID, lifetime, victory, projectileType0Spawns,"
                    + " projectileType1Spawns, projectileType2Spawns, projectileType3Spawns, projectileType0Hits,"
                    + " projectileType1Hits, projectileType2Hits, projectileType3Hits, powerUp1PickUps,"
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
    #endregion

    #region Reading requested data from database
    private void ReadGameOverallDataFromDatabase()
    {
        DatabaseData databaseData = new DatabaseData();
        databaseData.dataType = 0;

        string conn = "URI=file:" + Application.dataPath + "/SCRMG_Database.db";

        IDbConnection dbconn;
        dbconn = (IDbConnection)new SqliteConnection(conn);
        IDbCommand dbcmd = dbconn.CreateCommand();
        IDataReader reader;
        string sqlQuery;
        dbconn.Open();

        #region Reading data from MatchData table
        sqlQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='MatchData' ";
        dbcmd.CommandText = sqlQuery;
        reader = dbcmd.ExecuteReader();

        if (!reader.Read())
        {
            reader.Close();
            reader = null;
            databaseData.dataType = -1;
        }
        else
        {
            reader.Close();
            reader = null;

            #region Overall time spent in matches
            sqlQuery = "SELECT matchDuration FROM MatchData";
            dbcmd.CommandText = sqlQuery;
            reader = dbcmd.ExecuteReader();
            float totalTimeSpentInMatches = 0;
            while (reader.Read())
            {
                totalTimeSpentInMatches += reader.GetFloat(0);
            }
            reader.Close();
            reader = null;

            databaseData.dbDataFloats.Add(totalTimeSpentInMatches);
            #endregion

            #region Overall match count
            sqlQuery = "SELECT max(PK) FROM MatchData";
            dbcmd.CommandText = sqlQuery;
            reader = dbcmd.ExecuteReader();
            int readInt = 0;
            while (reader.Read())
            {
                readInt = reader.GetInt32(0);
            }
            reader.Close();
            reader = null;

            databaseData.dbDataInts.Add(readInt);
            #endregion

            #region Overall powerUps picked up
            sqlQuery = "SELECT powerUpsOverall FROM MatchData";
            dbcmd.CommandText = sqlQuery;
            reader = dbcmd.ExecuteReader();
            //List<int> readInts = new List<int>();
            int overallPowerUpsPickedUp = 0;
            while (reader.Read())
            {
                overallPowerUpsPickedUp += reader.GetInt32(0);
                //readInts.Add(reader.GetInt32(0));
            }
            reader.Close();
            reader = null;

            //int overallPowerUpsPickedUp = 0;
            //foreach (int value in readInts)
            //{
            //    overallPowerUpsPickedUp += value;
            //}

            databaseData.dbDataInts.Add(overallPowerUpsPickedUp);
            #endregion
        }
        #endregion

        if (databaseData.dataType != -1)
        {
            #region Reading data from PlayerData table
            sqlQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='PlayerData' ";
            dbcmd.CommandText = sqlQuery;
            reader = dbcmd.ExecuteReader();

            if (!reader.Read())
            {
                reader.Close();
                reader = null;
                databaseData.dataType = -1;
            }
            else
            {
                reader.Close();
                reader = null;

                #region Overall projectiles spawned 
                sqlQuery = "SELECT projectileType0Spawns, projectileType1Spawns, projectileType2Spawns, projectileType3Spawns FROM PlayerData";
                dbcmd.CommandText = sqlQuery;
                reader = dbcmd.ExecuteReader();
                List<int> readInts = new List<int>();
                while (reader.Read())
                {
                    readInts.Add(reader.GetInt32(0));
                    readInts.Add(reader.GetInt32(1));
                    readInts.Add(reader.GetInt32(2));
                    readInts.Add(reader.GetInt32(3));
                }
                reader.Close();
                reader = null;

                int overallProjectilesSpawned = 0;
                foreach (int value in readInts)
                {
                    overallProjectilesSpawned += value;
                }

                databaseData.dbDataInts.Add(overallProjectilesSpawned);
                #endregion
            }
            #endregion
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

        em.BroadcastReturnDataFromDatabase(databaseData);
    }

    private void ReadPlayerLastMatchDataFromDatabase()
    {
        DatabaseData databaseData = new DatabaseData();
        databaseData.dataType = 1;

        string conn = "URI=file:" + Application.dataPath + "/SCRMG_Database.db";

        IDbConnection dbconn;
        dbconn = (IDbConnection)new SqliteConnection(conn);
        IDbCommand dbcmd = dbconn.CreateCommand();
        IDataReader reader;
        string sqlQuery;
        dbconn.Open();

        #region Reading data from MatchData table
        sqlQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='MatchData' ";
        dbcmd.CommandText = sqlQuery;
        reader = dbcmd.ExecuteReader();

        if (!reader.Read())
        {
            reader.Close();
            reader = null;
            databaseData.dataType = -1;
        }
        else
        {
            reader.Close();
            reader = null;

            #region Last match duration
            sqlQuery = "SELECT matchDuration FROM MatchData WHERE PK=(SELECT MAX(PK) FROM MatchData)";
            dbcmd.CommandText = sqlQuery;
            reader = dbcmd.ExecuteReader();
            float readFloat = -1;
            while (reader.Read())
            {
                readFloat = reader.GetFloat(0);
            }
            reader.Close();
            reader = null;

            databaseData.dbDataFloats.Add(readFloat);
            #endregion
        }
        #endregion

        if (databaseData.dataType != -1)
        {
            #region Reading data from PlayerData table
            sqlQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='PlayerData' ";
            dbcmd.CommandText = sqlQuery;
            reader = dbcmd.ExecuteReader();

            if (!reader.Read())
            {
                reader.Close();
                reader = null;
                databaseData.dataType = -1;
            }
            else
            {
                reader.Close();
                reader = null;
                int lastMatchPlayerDataRowPK = -1;

                #region Finding the right row
                sqlQuery = "SELECT MAX(PK) FROM PlayerData";
                dbcmd.CommandText = sqlQuery;
                reader = dbcmd.ExecuteReader();
                while (reader.Read())
                {
                    lastMatchPlayerDataRowPK = reader.GetInt32(0);
                }
                lastMatchPlayerDataRowPK -= 3;
                reader.Close();
                reader = null;
                #endregion

                #region Last match victory state
                sqlQuery = "SELECT victory FROM PlayerData WHERE PK=" + lastMatchPlayerDataRowPK;
                dbcmd.CommandText = sqlQuery;
                reader = dbcmd.ExecuteReader();
                int readInt = -1;
                while (reader.Read())
                {
                    readInt = reader.GetInt32(0);
                }
                reader.Close();
                reader = null;

                databaseData.dbDataInts.Add(readInt);
                #endregion

                #region Last match player lifetime
                sqlQuery = "SELECT lifetime FROM PlayerData WHERE PK=" + lastMatchPlayerDataRowPK;
                dbcmd.CommandText = sqlQuery;
                reader = dbcmd.ExecuteReader();
                float readFloat = -1;
                while (reader.Read())
                {
                    readFloat = reader.GetFloat(0);
                }
                reader.Close();
                reader = null;

                databaseData.dbDataFloats.Add(readFloat);
                #endregion

                #region Projectiles spawned and hit per type
                sqlQuery = "SELECT projectileType0Spawns, projectileType1Spawns, projectileType2Spawns,"
                    + " projectileType3Spawns, projectileType0Hits, projectileType1Hits, projectileType2Hits,"
                    + " projectileType3Hits FROM PlayerData WHERE PK=" + lastMatchPlayerDataRowPK;
                dbcmd.CommandText = sqlQuery;
                reader = dbcmd.ExecuteReader();
                int readInt0 = -1;
                int readInt1 = -1;
                int readInt2 = -1;
                int readInt3 = -1;
                int readInt4 = -1;
                int readInt5 = -1;
                int readInt6 = -1;
                int readInt7 = -1;

                while (reader.Read())
                {
                    readInt0 = reader.GetInt32(0);
                    readInt1 = reader.GetInt32(1);
                    readInt2 = reader.GetInt32(2);
                    readInt3 = reader.GetInt32(3);
                    readInt4 = reader.GetInt32(4);
                    readInt5 = reader.GetInt32(5);
                    readInt6 = reader.GetInt32(6);
                    readInt7 = reader.GetInt32(7);
                }
                reader.Close();
                reader = null;

                databaseData.dbDataInts.Add(readInt0);
                databaseData.dbDataInts.Add(readInt1);
                databaseData.dbDataInts.Add(readInt2);
                databaseData.dbDataInts.Add(readInt3);
                databaseData.dbDataInts.Add(readInt4);
                databaseData.dbDataInts.Add(readInt5);
                databaseData.dbDataInts.Add(readInt6);
                databaseData.dbDataInts.Add(readInt7);
                #endregion
            }
            #endregion
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

        em.BroadcastReturnDataFromDatabase(databaseData);
    }

    private void ReadPlayerOverallDataFromDatabase()
    {
        DatabaseData databaseData = new DatabaseData();
        databaseData.dataType = 2;

        string conn = "URI=file:" + Application.dataPath + "/SCRMG_Database.db";

        IDbConnection dbconn;
        dbconn = (IDbConnection)new SqliteConnection(conn);
        IDbCommand dbcmd = dbconn.CreateCommand();
        IDataReader reader;
        string sqlQuery;
        dbconn.Open();

        #region Reading data from MatchData table
        sqlQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='MatchData' ";
        dbcmd.CommandText = sqlQuery;
        reader = dbcmd.ExecuteReader();

        if (!reader.Read())
        {
            reader.Close();
            reader = null;
            databaseData.dataType = -1;
        }
        else
        {
            reader.Close();
            reader = null;

            #region Overall match duration
            sqlQuery = "SELECT matchDuration FROM MatchData";
            dbcmd.CommandText = sqlQuery;
            reader = dbcmd.ExecuteReader();
            List<float> readFloats = new List<float>();
            while (reader.Read())
            {
                readFloats.Add(reader.GetFloat(0));
            }
            reader.Close();
            reader = null;

            foreach (float value in readFloats)
            {
                databaseData.dbDataFloats.Add(value);
            }
            #endregion
        }
        #endregion

        if (databaseData.dataType != -1)
        {
            #region Reading data from PlayerData table
            sqlQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='PlayerData' ";
            dbcmd.CommandText = sqlQuery;
            reader = dbcmd.ExecuteReader();

            if (!reader.Read())
            {
                reader.Close();
                reader = null;
                databaseData.dataType = -1;
            }
            else
            {
                reader.Close();
                reader = null;

                #region Overall match victory state
                sqlQuery = "SELECT victory FROM PlayerData WHERE shipIndex=1";
                dbcmd.CommandText = sqlQuery;
                reader = dbcmd.ExecuteReader();
                int victories = 0;
                while (reader.Read())
                {
                    if (reader.GetInt32(0) == 1)
                    {
                        victories++;
                    }
                }
                reader.Close();
                reader = null;

                databaseData.dbDataInts.Add(victories);
                #endregion

                #region All player lifetimes
                sqlQuery = "SELECT lifetime FROM PlayerData WHERE shipIndex=1";
                dbcmd.CommandText = sqlQuery;
                reader = dbcmd.ExecuteReader();
                List<float> readFloats = new List<float>();
                while (reader.Read())
                {
                    readFloats.Add(reader.GetFloat(0));
                }
                reader.Close();
                reader = null;

                foreach (float value in readFloats)
                {
                    databaseData.dbDataFloats.Add(value);
                }
                #endregion

                #region Overall projectiles spawned and hit per type
                sqlQuery = "SELECT projectileType0Spawns, projectileType1Spawns, projectileType2Spawns,"
                    + " projectileType3Spawns, projectileType0Hits, projectileType1Hits, projectileType2Hits,"
                    + " projectileType3Hits FROM PlayerData WHERE shipIndex=1";
                dbcmd.CommandText = sqlQuery;
                reader = dbcmd.ExecuteReader();
                int projectileType0Spawns = 0;
                int projectileType1Spawns = 0;
                int projectileType2Spawns = 0;
                int projectileType3Spawns = 0;
                int projectileType0Hits = 0;
                int projectileType1Hits = 0;
                int projectileType2Hits = 0;
                int projectileType3Hits = 0;

                while (reader.Read())
                {
                    projectileType0Spawns += reader.GetInt32(0);
                    projectileType1Spawns += reader.GetInt32(1);
                    projectileType2Spawns += reader.GetInt32(2);
                    projectileType3Spawns += reader.GetInt32(3);
                    projectileType0Hits += reader.GetInt32(4);
                    projectileType1Hits += reader.GetInt32(5);
                    projectileType2Hits += reader.GetInt32(6);
                    projectileType3Hits += reader.GetInt32(7);
                }
                reader.Close();
                reader = null;

                databaseData.dbDataInts.Add(projectileType0Spawns);
                databaseData.dbDataInts.Add(projectileType1Spawns);
                databaseData.dbDataInts.Add(projectileType2Spawns);
                databaseData.dbDataInts.Add(projectileType3Spawns);
                databaseData.dbDataInts.Add(projectileType0Hits);
                databaseData.dbDataInts.Add(projectileType1Hits);
                databaseData.dbDataInts.Add(projectileType2Hits);
                databaseData.dbDataInts.Add(projectileType3Hits);
                #endregion
            }
            #endregion
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

        em.BroadcastReturnDataFromDatabase(databaseData);
    }

    private void ReadMatchLastMatchDataFromDatabase()
    {
        Debug.Log("ReadMatchLastMatchDataFromDatabase");
        DatabaseData databaseData = new DatabaseData();
        databaseData.dataType = 3;

        string conn = "URI=file:" + Application.dataPath + "/SCRMG_Database.db";

        IDbConnection dbconn;
        dbconn = (IDbConnection)new SqliteConnection(conn);
        IDbCommand dbcmd = dbconn.CreateCommand();
        IDataReader reader;
        string sqlQuery;
        dbconn.Open();

        #region Reading data from MatchData table
        sqlQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='MatchData' ";
        dbcmd.CommandText = sqlQuery;
        reader = dbcmd.ExecuteReader();

        if (!reader.Read())
        {
            reader.Close();
            reader = null;
            databaseData.dataType = -1;
        }
        else
        {
            reader.Close();
            reader = null;

            #region Last match Duration
            sqlQuery = "SELECT matchDuration, humanPlayerWon, powerUpsOverall, powerUpBase1,"
                + " powerUpBase2, powerUpBase3, powerUpBase4 FROM MatchData"
                + " WHERE PK=(SELECT MAX(PK) FROM MatchData)";
            dbcmd.CommandText = sqlQuery;
            reader = dbcmd.ExecuteReader();
            float readFloat = -1;
            List<int> readInts = new List<int>();
            while (reader.Read())
            {
                readFloat = reader.GetFloat(0);
                readInts.Add(reader.GetInt32(1));
                readInts.Add(reader.GetInt32(2));
                readInts.Add(reader.GetInt32(3));
                readInts.Add(reader.GetInt32(4));
                readInts.Add(reader.GetInt32(5));
                readInts.Add(reader.GetInt32(6));
            }
            reader.Close();
            reader = null;

            databaseData.dbDataFloats.Add(readFloat);
            foreach (int value in readInts)
            {
                databaseData.dbDataInts.Add(value);
            }
            #endregion
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

        em.BroadcastReturnDataFromDatabase(databaseData);
    }

    private void ReadMatchOverallDataFromDatabase()
    {
        DatabaseData databaseData = new DatabaseData();
        databaseData.dataType = 4;

        string conn = "URI=file:" + Application.dataPath + "/SCRMG_Database.db";

        IDbConnection dbconn;
        dbconn = (IDbConnection)new SqliteConnection(conn);
        IDbCommand dbcmd = dbconn.CreateCommand();
        IDataReader reader;
        string sqlQuery;
        dbconn.Open();

        #region Reading data from MatchData table
        sqlQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='MatchData' ";
        dbcmd.CommandText = sqlQuery;
        reader = dbcmd.ExecuteReader();

        if (!reader.Read())
        {
            reader.Close();
            reader = null;
            databaseData.dataType = -1;
        }
        else
        {
            reader.Close();
            reader = null;

            #region Match data
            sqlQuery = "SELECT matchDuration, humanPlayerWon, powerUpsOverall, powerUpBase1,"
                + " powerUpBase2, powerUpBase3, powerUpBase4 FROM MatchData";
            dbcmd.CommandText = sqlQuery;
            reader = dbcmd.ExecuteReader();
            float totalTimeSpentInMatches = 0;
            int matchCount = 0;
            int playerVictories = 0;
            int readInt1 = 0;
            int readInt2 = 0;
            int readInt3 = 0;
            int readInt4 = 0;
            int readInt5 = 0;
            while (reader.Read())
            {
                matchCount++;
                   totalTimeSpentInMatches += reader.GetFloat(0);

                if (reader.GetInt32(1) == 1)
                    playerVictories++;

                readInt1 += reader.GetInt32(2);
                readInt2 += reader.GetInt32(3);
                readInt3 += reader.GetInt32(4);
                readInt4 += reader.GetInt32(5);
                readInt5 += reader.GetInt32(6);
            }
            reader.Close();
            reader = null;

            databaseData.dbDataFloats.Add(totalTimeSpentInMatches);
            databaseData.dbDataInts.Add(matchCount);
            databaseData.dbDataInts.Add(playerVictories);
            databaseData.dbDataInts.Add(readInt1);
            databaseData.dbDataInts.Add(readInt2);
            databaseData.dbDataInts.Add(readInt3);
            databaseData.dbDataInts.Add(readInt4);
            databaseData.dbDataInts.Add(readInt5);
            #endregion
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

        em.BroadcastReturnDataFromDatabase(databaseData);
    }
    #endregion

}
