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
        Toolbox toolbox;
        EventManager em;
        GameManager gm;
        GlobalVariableLibrary lib;
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
        static int lobbyJoinedResponse = -1; //-1 = default, 0 = joining denied, 1 = joined successfully, 2 = waiting for response
        static int registrationResponse = -1; //-1 = default, 1 = registration completed, 2 = waiting for response
        int currentGameModeIndex = -1;
        static int readyCountInLobby = -1;
        static int clientsInLobby = -1;
        static List<ShipInfo> newlySpawnedShips = new List<ShipInfo>();

        int gameModeSingleplayerIndex = -1;
        int gameModeNetworkMultiplayerIndex = -1;
        int gameModeLocalMultiplayerIndex = -1;
        bool networkFunctionalityDisabled = false;

        void Awake()
        {
            toolbox = FindObjectOfType<Toolbox>();
            em = toolbox.GetComponent<EventManager>();
            gm = toolbox.GetComponent<GameManager>();
            lib = toolbox.GetComponent<GlobalVariableLibrary>();
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
            inGame = false;
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
            Debug.Log("Requesting lobby enter from server");
            lobbyJoinedResponse = 2;
            Packet p = new Packet(PacketType.LOBBYEVENT, clientID);
            p.GdataInts.Add(0);
            p.GdataInts.Add(1);
            p.GdataStrings.Add(clientName);
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

        private void OnRequestLobbyExit()
        {
            Debug.Log("Exiting lobby");
            Packet p = new Packet(PacketType.LOBBYEVENT, clientID);
            p.GdataInts.Add(0);
            p.GdataInts.Add(0);
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

        private void OnSetGameMode(int newGameModeIndex)
        {
            currentGameModeIndex = newGameModeIndex;
        }

        private void OnNewSceneLoaded(int sceneIndex)
        {
            Debug.Log("NetworkClient, OnNewSceneLoaded, sceneIndex: " + sceneIndex);
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
            Debug.Log("NetworkClient: OnRequestDisconnectFromNetwork");
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
                Debug.Log("Lobby isReady event sent to server");
            }
            else
            {
                p.GdataInts.Add(2);
                p.GdataInts.Add(0);
                Debug.Log("Lobby notReady event sent to server");
            }
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

        private void OnRequestOnlineMatchStart()
        {
            Debug.Log("Sending start match request");
            Packet p = new Packet(PacketType.LOBBYEVENT, clientID);
            p.GdataInts.Add(3);
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

        private string OnRequestMyNetworkID()
        {
            return clientID;
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
                        Debug.Log("NetworkClient: inGame");
                        float timeStamp = Time.time;

                        #region Local client controlled ship's movement data
                        if (gm.shipInfoList.Count > 0)
                        {
                            //TODO: Redo local ship movement data sending
                            //GdataInts[0] is shipIndex
                            //GdataVectors[0] is shipPosition etc

                            //Debug.Log("gm.shipInfoList.Count > 0");
                            //Packet p = new Packet(PacketType.MOVEMENT, clientID);
                            ////string input = "Hello from the client side " +
                            ////DateTime.Now.ToLongTimeString();
                            ////p.GdataStrings.Add(input);
                            //Vector3 shipPosition = gm.shipInfoList[0].shipPosition;
                            //Vector_3 _shipPosition = new Vector_3(shipPosition.x,
                            //    shipPosition.y, shipPosition.z);
                            //p.GdataVectors.Add(_shipPosition);

                            //try
                            //{
                            //    master.Send(p.ToBytes());
                            //    Debug.Log("Bytes sent");
                            //}
                            //catch (SocketException ex)
                            //{
                            //    Debug.Log("SocketException: " + ex);
                            //    Debug.Log("Connection to server lost.");
                            //    Disconnect();
                            //}
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
                    em.BroadcastStartingMatchByServer();
                }

                if(newlySpawnedShips.Count > 0)
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
                            DataManager(packet);
                        }
                        catch
                        {
                            Debug.LogWarning("Unable to create packet from buffer bytes!");
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
            Debug.Log("DataManager called");

            switch (p.packetType)
            {
                case PacketType.REGISTRATION:
                    clientID = p.GdataStrings[0];
                    registrationResponse = 1;
                    Debug.Log("Registration packet managed, my id: " + clientID);

                    break;

                case PacketType.DEBUG:
                    Debug.Log("Debug packet managed");
                    break;

                case PacketType.LOBBYEVENT:
                    int tmp1 = p.GdataInts[0];
                    int tmp2 = p.GdataInts[1];
                    if(tmp1 == 0)
                    {
                        lobbyJoinedResponse = tmp2;
                        Debug.Log("LobbyJoinedResponse: " + lobbyJoinedResponse);
                    }
                    else if(tmp1== 1)
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
                    break;

                case PacketType.GAMESTART:
                    Debug.Log("GameStarted by server");
                    startMatch = true;
                    Debug.Log("GameStart packet managed");
                    break;

                case PacketType.SPAWN:
                    Debug.Log("Starting to manage spawn packet");
                    ShipInfo newShipInfo = new ShipInfo();
                    newShipInfo.shipIndex = p.GdataInts[0];
                    newShipInfo.spawnPointIndex = p.GdataInts[1];
                    newShipInfo.shipColorIndex = p.GdataInts[2];
                    newShipInfo.ownerID = p.GdataStrings[0];
                    newlySpawnedShips.Add(newShipInfo);
                    Debug.Log("Spawn packet managed");
                    break;
            }
        }
        #endregion
    }

}
