using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using ServerData;
using UnityEngine;

namespace Client
{
    /* TODO:
     * - There is a connection button and indicator (separated or combined) in the main menu 
     *      (top left corner)
     * - The game tries to connect to the server automatically when the application starts
     * - If the game is connected to the server, the connection indicator displays succesful 
     *      connection to the server
     * - If the game is not able to connect to the server initially, the user can press the 
     *      connection button to re-try connecting to the server
     * - When succesfully connecting to the sever, a registration packet is sent (from the 
     *      client to the server and then back) from the server to the client with an id to 
     *      identify the clients future packets
     * 
     * - To start a network multiplayer game, the client is required a connection to the 
     *      server
     * - When a client starts a network multiplayer game, they are transferred to the lobby 
     *      to wait for more participants
     * - Lobby displays the number of clients in the lobby, time spent in the lobby as well 
     *      as options to returning to main menu and quitting the game
     * - Once enough players are connected, server sends "ready" notification to all 
     *      participating clients
     * - The clients are then transferred to the match scene
     * 
     * - The server then begins initializing the match
     * - The server spawns ships, gives them indices and sends the info to clients
     * - The server then starts the matchStartTimer
     * - Once the timer runs out, the clients have control of their respective ships
     * - Ship movement, aim, shootEvent, etc info is sent from client to server to other 
     *      clients
     * - Pause menu is available in network multiplayer, though it does not pause the game
     *
     * - When the game ends, server sends notification to the clients
     * - If the clients want to restart the game, they can through unanimous vote in the 
     *      gameEnd menu
     * - If one or more clients decides otherwise, the remaining clients are returned to 
     *      the lobby
     * - (Or if a client disconnects, or exits the match through the pause menu)
     * - When ever a client is returned in the lobby, they are notified for the reason of 
     *      the transfer
     *      
     * - If a client disconnects while in the lobby, they are returned to the game mode view
     * - If a client disconnecst during a match, they are returned to the default main menu 
     *      view
     *      
     *     
     *     
     *      
     * - Find out why reconnecting after disconnecting once without closing the play-mode does not work
     * - Clearing socket and aborting thread not enought to clear connection on client end??
     *  
     */

    public class NetworkClient : MonoBehaviour
    {
        //TODO: Implement ship death to and from server
        //Implement gameEnd menu functionality on networkMultiplayer
        //Figure out a way to predict shipInfo to prevent perceivable dis-sync with other clients!

        //Find out why GAMEEND packet is never received by the client!!

        Toolbox toolbox;
        EventManager em;
        GlobalVariableLibrary lib;
        public static ShipInfoManager shipInfoManager;
        public static Socket master;
        Thread incomingDataThread;
        public static string clientName = "Metsä"; //TODO: Implement client name changing
        public static string clientID;
        static string serverIP;
        bool connected = false;
        bool inGame = false;
        static bool readyCountInLobbyChanged = false;
        static bool clientsInLobbyChanged = false;
        static bool requestingDisconnectFromThread = false;
        static bool startMatch = false;
        static bool allClientsDoneWithMatchInitialization = false;
        static int lobbyJoinedResponse = -1; //-1 = default, 0 = joining denied, 1 = joined successfully, 2 = waiting for response
        static int registrationResponse = -1; //-1 = default, 1 = registration completed, 2 = waiting for response
        static int readyCountInLobby = -1;
        static int clientsInLobby = -1;
        static int numberOfShips = -1;
        static int myShipIndex = -1;
        static int winnerIndex = -1;
        static string winnerName = "empty";
        static bool gameEnded = false;
        int currentGameModeIndex = -1;
        int myShipInfoElement = -1;
        float shipInfoUpdateTimer = -1;
        static List<ShipInfo> newlySpawnedShips = new List<ShipInfo>();
        static List<ProjectileInfo> newlySpawnedProjectiles = new List<ProjectileInfo>();
        static List<ProjectileInfo> newlyHitProjectiles = new List<ProjectileInfo>();
        static List<ProjectileInfo> newlyDestroyedProjectiles = new List<ProjectileInfo>();

        float shipInfoUpdatesPerSecond = 5f;
        int gameModeSingleplayerIndex = -1;
        int gameModeNetworkMultiplayerIndex = -1;
        int gameModeLocalMultiplayerIndex = -1;
        bool networkFunctionalityDisabled = false;

        void Awake()
        {
            toolbox = FindObjectOfType<Toolbox>();
            em = toolbox.GetComponent<EventManager>();
            lib = toolbox.GetComponent<GlobalVariableLibrary>();
            shipInfoManager = toolbox.GetComponent<ShipInfoManager>();
            GetStats();

            if (networkFunctionalityDisabled)
            {
                this.enabled = false;
            }
        }

        private void GetStats()
        {
            gameModeSingleplayerIndex = lib.gameSettingVariables.gameModeSingleplayerIndex;
            gameModeNetworkMultiplayerIndex = lib.gameSettingVariables.gameModeNetworkMultiplayerIndex;
            gameModeLocalMultiplayerIndex = lib.gameSettingVariables.gameModeLocalMultiplayerIndex;
            networkFunctionalityDisabled = lib.networkingVariables.networkFunctionalityDisabled;

            readyCountInLobby = 0;
            clientsInLobby = 0;
        }

        void Start()
        {
            string ip = Packet.GetIP4Address();
            master = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            TryConnectingToHost(ip);
        }

        #region OnEnable & OnDisable
        void OnEnable()
        {
            em.OnNewSceneLoaded += OnNewSceneLoaded;
            em.OnSetGameMode += OnSetGameMode;
            em.OnRequestConnectToNetwork += OnRequestConnectToNetwork;
            em.OnRequestServerIPAddress += Packet.GetIP4Address;
            em.OnRequestDisconnectFromNetwork += OnRequestDisconnectFromNetwork;
            em.OnRequestLobbyEnter += OnRequestLobbyEnter;
            em.OnRequestLobbyExit += OnRequestLobbyExit;
            em.OnLobbyReadyStateChange += OnLobbyReadyStateChange;
            em.OnRequestOnlineMatchStart += OnRequestOnlineMatchStart;
            em.OnRequestMyNetworkID += OnRequestMyNetworkID;
            em.OnNetworkMultiplayerMatchInitialized += OnNetworkMultiplayerMatchInitialized;
            em.OnProjectileHitShip += OnProjectileHitShip;
            em.OnProjectileSpawned += OnProjectileSpawned;
            em.OnProjectileDestroyed += OnProjectileDestroyed;
            em.OnShipDead += OnShipDead;

            inGame = false;
            shipInfoUpdateTimer = 0;
        }

        void OnDisable()
        {
            em.OnNewSceneLoaded -= OnNewSceneLoaded;
            em.OnSetGameMode -= OnSetGameMode;
            em.OnRequestConnectToNetwork -= OnRequestConnectToNetwork;
            em.OnRequestServerIPAddress -= Packet.GetIP4Address;
            em.OnRequestDisconnectFromNetwork -= OnRequestDisconnectFromNetwork;
            em.OnRequestLobbyEnter -= OnRequestLobbyEnter;
            em.OnRequestLobbyExit -= OnRequestLobbyExit;
            em.OnLobbyReadyStateChange -= OnLobbyReadyStateChange;
            em.OnRequestOnlineMatchStart -= OnRequestOnlineMatchStart;
            em.OnRequestMyNetworkID -= OnRequestMyNetworkID;
            em.OnNetworkMultiplayerMatchInitialized -= OnNetworkMultiplayerMatchInitialized;
            em.OnProjectileHitShip -= OnProjectileHitShip;
            em.OnProjectileSpawned -= OnProjectileSpawned;
            em.OnProjectileDestroyed -= OnProjectileDestroyed;
            em.OnShipDead -= OnShipDead;

            if (incomingDataThread != null)
            {
                incomingDataThread.Abort();
                incomingDataThread = null;
                Debug.Log("IncomingData thread aborted");
            }

            if (master != null)
            {
                master.Close();
                master = null;
                Debug.Log("MasterSocket closed");
            }
        }
        #endregion

        #region Subscribers
        private void OnRequestLobbyEnter()
        {
            lobbyJoinedResponse = 2;
            Packet p = new Packet(PacketType.LOBBYEVENT, clientID);
            p.GdataInts.Add(0);
            p.GdataInts.Add(1);
            p.GdataStrings.Add(clientName);
            try
            {
                master.Send(p.ToBytes());
            }
            catch (SocketException ex)
            {
                Debug.Log("SocketException: " + ex);
                Debug.Log("Connection to server lost.");
                Disconnect();
            }
        }

        private void OnRequestLobbyExit()
        {
            Packet p = new Packet(PacketType.LOBBYEVENT, clientID);
            p.GdataInts.Add(0);
            p.GdataInts.Add(0);
            try
            {
                master.Send(p.ToBytes());
            }
            catch (SocketException ex)
            {
                Debug.Log("SocketException: " + ex);
                Debug.Log("Connection to server lost.");
                Disconnect();
            }
        }

        private void OnSetGameMode(int newGameModeIndex)
        {
            currentGameModeIndex = newGameModeIndex;
        }

        private void OnNewSceneLoaded(int sceneIndex)
        {
            if (sceneIndex == 0)
            {
                inGame = false;
            }
            else if (sceneIndex == 1)
            {
                inGame = true;
            }
        }

        private void OnRequestConnectToNetwork(string ip)
        {
            if (!connected)
            {
                TryConnectingToHost(ip);
            }
            else
            {
                Debug.LogError("NetworkClient, OnRequestConnectToNetwork: Already connected!!");
            }
        }

        private void OnRequestDisconnectFromNetwork()
        {
            if (connected)
            {
                Disconnect();
            }
            else
            {
                Debug.LogError("NetworkClient, OnRequestDisconnectFromNetwork: Not connected!!");
            }
        }

        private bool OnRequestNetworkConnectionStatus()
        {
            return connected;
        }

        private void OnLobbyReadyStateChange(bool state)
        {
            Debug.Log("Sending lobby ready state");
            Packet p = new Packet(PacketType.LOBBYEVENT, clientID);
            if (state)
            {
                p.GdataInts.Add(2);
                p.GdataInts.Add(1);
            }
            else
            {
                p.GdataInts.Add(2);
                p.GdataInts.Add(0);
            }
            try
            {
                master.Send(p.ToBytes());
            }
            catch (SocketException ex)
            {
                Debug.Log("SocketException: " + ex);
                Debug.Log("Connection to server lost.");
                Disconnect();
            }
        }

        private void OnRequestOnlineMatchStart()
        {
            Debug.Log("Sending start match request");
            Packet p = new Packet(PacketType.LOBBYEVENT, clientID);
            p.GdataInts.Add(3);
            try
            {
                master.Send(p.ToBytes());
            }
            catch (SocketException ex)
            {
                Debug.Log("SocketException: " + ex);
                Debug.Log("Connection to server lost.");
                Disconnect();
            }
        }

        private string OnRequestMyNetworkID()
        {
            return clientID;
        }

        private void OnNetworkMultiplayerMatchInitialized()
        {
            Debug.Log("Sending match initialization completed check to server");
            Packet p = new Packet(PacketType.GAMESTART, clientID);
            p.GdataInts.Add(1);
            try
            {
                master.Send(p.ToBytes());
            }
            catch (SocketException ex)
            {
                Debug.Log("SocketException: " + ex);
                Debug.Log("Connection to server lost.");
                Disconnect();
            }
        }

        private void OnProjectileSpawned(int projectileOwnerIndex, int projectileIndex,
            Vector3 spawnPosition, Vector3 spawnRotation, bool isControlledByServer)
        {
            if (!isControlledByServer)
            {
                Packet p = new Packet(PacketType.PROJECTILE, clientID);
                p.GdataInts.Add(0);
                p.GdataInts.Add(projectileOwnerIndex);
                p.GdataInts.Add(projectileIndex);
                p.GdataVectors.Add(new Vector_3(spawnPosition));
                p.GdataVectors.Add(new Vector_3(spawnRotation));

                try
                {
                    master.Send(p.ToBytes());
                }
                catch (SocketException ex)
                {
                    Debug.Log("SocketException: " + ex);
                    Debug.Log("Connection to server lost.");
                    Disconnect();
                }
            }
        }

        private void OnProjectileHitShip(int projectileOwnerIndex, int projectileIndex,
            int hitShipIndex, float projectileDamage)
        {
            if (projectileOwnerIndex == myShipIndex)
            {
                Packet p = new Packet(PacketType.PROJECTILE, clientID);
                p.GdataInts.Add(1);
                p.GdataInts.Add(projectileOwnerIndex);
                p.GdataInts.Add(projectileIndex);
                p.GdataInts.Add(hitShipIndex);
                p.GdataFloats.Add(projectileDamage);
                try
                {
                    master.Send(p.ToBytes());
                }
                catch (SocketException ex)
                {
                    Debug.Log("SocketException: " + ex);
                    Debug.Log("Connection to server lost.");
                    Disconnect();
                }
            }
        }

        private void OnProjectileDestroyed(int projectileOwnerIndex, int projectileIndex, Vector3 location)
        {
            if (projectileOwnerIndex == myShipIndex)
            {
                Packet p = new Packet(PacketType.PROJECTILE, clientID);
                p.GdataInts.Add(0);
                p.GdataInts.Add(projectileOwnerIndex);
                p.GdataInts.Add(projectileIndex);
                p.GdataVectors.Add(new Vector_3(location));

                try
                {
                    master.Send(p.ToBytes());
                    Debug.Log("Bytes sent");
                }
                catch (SocketException ex)
                {
                    Debug.Log("SocketException: " + ex);
                    Debug.Log("Connection to server lost.");
                    Disconnect();
                }
            }
        }

        private void OnShipDead(int shipIndex, int killerIndex)
        {
            Packet p = new Packet(PacketType.DEATH, clientID);
            p.GdataInts.Add(0);
            p.GdataInts.Add(shipIndex);
            p.GdataInts.Add(killerIndex);

            try
            {
                master.Send(p.ToBytes());
            }
            catch (SocketException ex)
            {
                Debug.Log("SocketException: " + ex);
                Debug.Log("Connection to server lost.");
                Disconnect();
            }
        }
        #endregion

        #region Connecting and disconnecting
        private void TryConnectingToHost(string ip)
        {
            em.BroadcastRequestLoadingIconOn();
            try
            {
                serverIP = ip;
                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(serverIP), 8888);
                Debug.Log("Trying to connect to IP address: " + serverIP);
                master.Connect(ipe); //TODO: Find out why this no longer works (until restarting play mode) after disconnecting once

                incomingDataThread = new Thread(DataIN);
                incomingDataThread.Start();

                Debug.Log("Connected to host.");
                connected = true;
                registrationResponse = 2;
            }
            catch
            {
                Debug.Log("Could not connect to host.");
                connected = false;
                em.BroadcastConnectingToNetworkFailed(serverIP);
                em.BroadcastRequestLoadingIconOff();
            }
        }

        private void Disconnect()
        {
            if (connected)
            {
                Debug.Log("NetworkClient: Disconnect");
                if (incomingDataThread != null)
                {
                    incomingDataThread.Abort();
                    incomingDataThread = null;
                }

                if (master != null)
                {
                    master.Close();
                    master = null;
                }

                connected = false;
                Debug.Log("Disconnected");
                em.BroadcastConnectionToNetworkLost();
                em.BroadcastRequestUINotification(lib.networkingVariables.unintentionalDisconnectUINotificationContent);
            }
        }
        #endregion

        #region FixedUpdate
        void FixedUpdate()
        {
            if (connected)
            {
                if (currentGameModeIndex == gameModeNetworkMultiplayerIndex)
                {
                    if (inGame)
                    {
                        #region Local client controlled ship's info
                        shipInfoUpdateTimer += Time.fixedDeltaTime;
                        if (shipInfoUpdateTimer >= (1 / shipInfoUpdatesPerSecond))
                        {
                            shipInfoUpdateTimer = 0;

                            if (shipInfoManager.shipInfoList.Count > 0)
                            {
                                if (myShipInfoElement == -1)
                                {
                                    myShipInfoElement = shipInfoManager.GetMyShipInfoElement(myShipIndex);
                                }

                                if (myShipInfoElement != -1)
                                {
                                    Packet p = new Packet(PacketType.SHIPINFO, clientID);
                                    p.GdataInts.Add(shipInfoManager.shipInfoList[myShipInfoElement].shipIndex);
                                    p.GdataStrings.Add(shipInfoManager.shipInfoList[myShipInfoElement].ownerID);
                                    p.GdataVectors.Add(new Vector_3(shipInfoManager.shipInfoList[myShipInfoElement].shipPosition));
                                    p.GdataVectors.Add(new Vector_3(shipInfoManager.shipInfoList[myShipInfoElement].hullRotation));
                                    p.GdataVectors.Add(new Vector_3(shipInfoManager.shipInfoList[myShipInfoElement].turretRotation));

                                    try
                                    {
                                        master.Send(p.ToBytes());
                                    }
                                    catch (SocketException ex)
                                    {
                                        Debug.Log("Error while sending local shipInfo to server. SocketException: " + ex);
                                        Debug.Log("Connection to server lost.");
                                        Disconnect();
                                    }
                                }
                            }
                        }
                        #endregion

                        #region Projectile events from server
                        if (newlySpawnedProjectiles.Count > 0)
                        {
                            for (int i = 0; i < newlySpawnedProjectiles.Count; i++)
                            {
                                em.BroadcastProjectileSpawnedByServer(newlySpawnedProjectiles[i].projectileOwnerIndex,
                                    newlySpawnedProjectiles[i].projectileIndex, newlySpawnedProjectiles[i].spawnPosition,
                                    newlySpawnedProjectiles[i].spawnRotation);

                                newlySpawnedProjectiles.RemoveAt(i);
                                i--;
                            }
                        }
                        if (newlyHitProjectiles.Count > 0)
                        {
                            for (int i = 0; i < newlyHitProjectiles.Count; i++)
                            {
                                em.BroadcastProjectileHitShipByServer(newlyHitProjectiles[i].projectileOwnerIndex,
                                    newlyHitProjectiles[i].projectileIndex, newlyHitProjectiles[i].hitShipIndex,
                                    newlyHitProjectiles[i].projectileDamage);

                                newlyHitProjectiles.RemoveAt(i);
                                i--;
                            }
                        }
                        if (newlyDestroyedProjectiles.Count > 0)
                        {
                            for (int i = 0; i < newlyDestroyedProjectiles.Count; i++)
                            {
                                em.BroadcastProjectileDestroyedByServer(newlyDestroyedProjectiles[i].projectileOwnerIndex,
                                    newlyDestroyedProjectiles[i].projectileIndex, newlyHitProjectiles[i].hitLocation);

                                newlyDestroyedProjectiles.RemoveAt(i);
                                i--;
                            }
                        }
                        #endregion
                    }
                }

                #region Registration & lobby events
                if (lobbyJoinedResponse != -1)
                {
                    if (lobbyJoinedResponse == 1)
                    {
                        em.BroadcastLobbyEnterSuccessful();
                        lobbyJoinedResponse = -1;
                    }
                    else if (lobbyJoinedResponse == 0)
                    {
                        em.BroadcastLobbyEnterDenied();
                        lobbyJoinedResponse = -1;
                    }
                }

                if (registrationResponse != -1)
                {
                    if (registrationResponse == 1)
                    {
                        em.BroadcastConnectingToNetworkSucceeded(serverIP);
                        registrationResponse = -1;
                        em.BroadcastRequestLoadingIconOff();
                    }
                }

                if (clientsInLobbyChanged)
                {
                    em.BroadcastClientCountInLobbyChange(clientsInLobby);
                    clientsInLobbyChanged = false;
                }

                if (readyCountInLobbyChanged)
                {
                    em.BroadcastReadyCountInLobbyChange(readyCountInLobby);
                    readyCountInLobbyChanged = false;
                }
                #endregion

                #region Match initialization events
                if (startMatch)
                {
                    startMatch = false;
                    em.BroadcastStartingMatchByServer(numberOfShips);
                }

                if (newlySpawnedShips.Count > 0)
                {
                    for (int i = 0; i < newlySpawnedShips.Count; i++)
                    {
                        em.BroadcastShipSpawnByServer(newlySpawnedShips[i].shipIndex,
                            newlySpawnedShips[i].spawnPointIndex, newlySpawnedShips[i].
                            shipColorIndex, newlySpawnedShips[i].ownerID);

                        newlySpawnedShips.RemoveAt(i);
                        i--;
                    }
                }

                if (allClientsDoneWithMatchInitialization)
                {
                    em.BroadcastNetworkMultiplayerStartMatchStartTimer();
                    allClientsDoneWithMatchInitialization = false;
                }
                #endregion

                #region Match ending
                if (gameEnded)
                {
                    Debug.LogWarning("Game has ended, broadcasting info!");
                    gameEnded = false;
                    bool iWin = false;
                    if (winnerIndex == myShipIndex)
                        iWin = true;

                    em.BroadcastMatchEndedByServer(winnerName, iWin);
                    gameEnded = false;
                }
                #endregion
            }

            #region Disconnect requests from a thread
            if (requestingDisconnectFromThread)
            {
                requestingDisconnectFromThread = false;
                Disconnect();
            }
            #endregion
        }
        #endregion

        #region Incoming Data thread
        static void DataIN()
        {
            byte[] Buffer;
            int readBytes;

            //TODO: Find out why the first packet is split in two parts 
            //(1460 and 169 bytes, instead of the full message's 1629 bytes)
            //Added two dummy receive calls to clear first split message from buffer
            //Buffer = new byte[master.SendBufferSize];
            //master.Receive(Buffer);
            //master.Receive(Buffer);
            bool running = true;

            while (running)
            {
                try
                {
                    Buffer = new byte[master.SendBufferSize];
                    readBytes = master.Receive(Buffer);

                    if (readBytes > 0)
                    {
                        Debug.Log("Before calling DataManager, readBytes: " + readBytes);
                        //DataManager(new Packet(Buffer));
                        //Packet p = new Packet(Buffer);

                        try
                        {
                            Packet packet = new Packet(Buffer);
                            if (!packet.errorEncountered)
                            {
                                DataManager(packet);
                            }
                            else
                            {
                                Debug.LogWarning("ErrorEncountered when creating packet from buffer, discarding packet");
                            }
                        }
                        catch
                        {
                            Debug.LogWarning("Unable to create packet from buffer bytes! readBytes: " + readBytes);
                        }
                    }
                }
                catch (SocketException ex)
                {
                    Debug.LogWarning("SocketException: " + ex);
                    running = false;
                    requestingDisconnectFromThread = true;
                    Thread.CurrentThread.Abort();
                }
            }
        }
        #endregion

        #region DataManager
        static void DataManager(Packet p)
        {
            Debug.Log("DataManager called, packetType: " + p.packetType);
            int shipIndex = -1;

            switch (p.packetType)
            {
                case PacketType.REGISTRATION:
                    #region Registration packet handling
                    clientID = p.GdataStrings[0];
                    registrationResponse = 1;
                    #endregion
                    break;

                case PacketType.DEBUG:
                    Debug.Log("PacketType Debug: " + p.GdataStrings[0]);
                    break;

                case PacketType.LOBBYEVENT:
                    #region LobbyEvent packet handling
                    int tmp1 = p.GdataInts[0];
                    int tmp2 = p.GdataInts[1];
                    if (tmp1 == 0)
                    {
                        lobbyJoinedResponse = tmp2;
                    }
                    else if (tmp1 == 1)
                    {
                        clientsInLobby = tmp2;
                        clientsInLobbyChanged = true;
                    }
                    else if (tmp1 == 2)
                    {
                        readyCountInLobby = tmp2;
                        readyCountInLobbyChanged = true;
                    }
                    Debug.Log("LobbyEvent packet managed");
                    #endregion
                    break;

                case PacketType.GAMESTART:
                    #region GameStart packet handling
                    int tmp = p.GdataInts[0];
                    if (tmp == 0)
                    {
                        numberOfShips = p.GdataInts[1];
                        startMatch = true;
                    }
                    else if (tmp == 1)
                    {
                        allClientsDoneWithMatchInitialization = true;
                    }
                    #endregion
                    break;

                case PacketType.SPAWN:
                    #region Spawn packet handling
                    ShipInfo newShipInfo = new ShipInfo();
                    shipIndex = p.GdataInts[0];
                    string ownerID = p.GdataStrings[0];
                    if (ownerID == clientID)
                    {
                        myShipIndex = shipIndex;
                    }
                    newShipInfo.shipIndex = shipIndex;
                    newShipInfo.ownerID = ownerID;
                    newShipInfo.spawnPointIndex = p.GdataInts[1];
                    newShipInfo.shipColorIndex = p.GdataInts[2];
                    newlySpawnedShips.Add(newShipInfo);
                    #endregion
                    break;

                case PacketType.PROJECTILE:
                    #region Projectile packet handling
                    if (p.GdataInts[0] == 0)
                    {
                        ProjectileInfo newProjectileInfo = new ProjectileInfo();
                        newProjectileInfo.projectileOwnerIndex = p.GdataInts[1];
                        newProjectileInfo.projectileIndex = p.GdataInts[2];
                        newProjectileInfo.spawnPosition = p.GdataVectors[0].ToVector3();
                        newProjectileInfo.spawnRotation = p.GdataVectors[1].ToVector3();
                        newlySpawnedProjectiles.Add(newProjectileInfo);
                    }
                    else if (p.GdataInts[0] == 1)
                    {
                        ProjectileInfo newProjectileInfo = new ProjectileInfo();
                        newProjectileInfo.projectileOwnerIndex = p.GdataInts[1];
                        newProjectileInfo.projectileIndex = p.GdataInts[2];
                        newProjectileInfo.hitShipIndex = p.GdataInts[3];
                        newProjectileInfo.projectileDamage = p.GdataFloats[0];
                        newlyHitProjectiles.Add(newProjectileInfo);
                    }
                    else if (p.GdataInts[0] == 2)
                    {
                        ProjectileInfo newProjectileInfo = new ProjectileInfo();
                        newProjectileInfo.projectileOwnerIndex = p.GdataInts[1];
                        newProjectileInfo.projectileIndex = p.GdataInts[2];
                        newProjectileInfo.hitLocation = p.GdataVectors[0].ToVector3();
                        newlyDestroyedProjectiles.Add(newProjectileInfo);
                    }
                    #endregion
                    break;

                case PacketType.SHIPINFO:
                    #region ShipInfo packet handling
                    int shipInfoListElement = shipInfoManager.GetMyShipInfoElement(p.GdataInts[0]);
                    if (shipInfoListElement != -1)
                    {
                        shipInfoManager.shipInfoList[shipInfoListElement].shipPosition = p.GdataVectors[0].ToVector3();
                        shipInfoManager.shipInfoList[shipInfoListElement].hullRotation = p.GdataVectors[1].ToVector3();
                        shipInfoManager.shipInfoList[shipInfoListElement].turretRotation = p.GdataVectors[2].ToVector3();
                    }
                    #endregion
                    break;

                case PacketType.DEATH:
                    #region Death packet handling
                    int shipInfoListElement2 = shipInfoManager.GetMyShipInfoElement(p.GdataInts[0]);
                    if (shipInfoListElement2 != -1)
                    {
                        shipInfoManager.shipInfoList[shipInfoListElement2].killerIndex = p.GdataInts[1];
                        shipInfoManager.shipInfoList[shipInfoListElement2].isDead = true;
                    }
                    #endregion
                    break;

                case PacketType.GAMEEND:
                    winnerIndex = p.GdataInts[0];
                    winnerName = p.GdataStrings[0];
                    gameEnded = true;
                    Debug.LogWarning("GameEndPacket managed");
                    break;

            }
        }
        #endregion
    }

}
