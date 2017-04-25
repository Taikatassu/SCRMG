using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using ServerData;
using UnityEngine;

namespace Server
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
     */

    public class NetworkServer : MonoBehaviour
    {
        //TODO: 
        //Implement proper returning to lobby functionality in case a client disconnects
        //Implement proper restarting and exiting match functionality
        //Figure out a way to predict shipInfo to prevent perceivable dis-sync with other clients!

        Toolbox toolbox;
        EventManager em;
        GlobalVariableLibrary lib;
        public static ShipInfoManager shipInfoManager;

        static Socket listenerSocket;
        Thread listenThread;
        public static List<ClientData> _clients = new List<ClientData>();
        public static List<ClientData> _clientsInLobby = new List<ClientData>();

        public static List<ClientData> newlyConnectedClients = new List<ClientData>();
        public static List<ClientData> newlyDisconnectedClients = new List<ClientData>();
        public static List<ClientData> lobbyQueue = new List<ClientData>();
        public static List<ClientData> clientsExitingLobby = new List<ClientData>();
        public static List<ClientData> readyQueue = new List<ClientData>();
        public static List<ClientData> notReadyQueue = new List<ClientData>();
        static List<ProjectileInfo> newlySpawnedProjectiles = new List<ProjectileInfo>();
        static List<ProjectileInfo> newlyHitProjectiles = new List<ProjectileInfo>();
        static List<ProjectileInfo> newlyDestroyedProjectiles = new List<ProjectileInfo>();
        public static bool inGame = false;
        public static bool requestMatchStart = false;
        public static bool alreadyRequestingMatchStart = false;
        public static bool matchBeginTimerStarted = false;
        public static bool requestMatchRestart = false;
        public static bool requestReturnToLobbyFromMatch = false;
        public static int clientsDoneWithInitializingMatch = -1;
        float shipInfoUpdateTimer = -1;
        int numberOfShips = -1;

        float shipInfoUpdatesPerSecond = 5f;
        int maxNumberOfClientsInLobby = -1;


        #region Initialization
        #region Awake & Start
        void Awake()
        {
            toolbox = FindObjectOfType<Toolbox>();
            em = toolbox.GetComponent<EventManager>();
            lib = toolbox.GetComponent<GlobalVariableLibrary>();
            shipInfoManager = toolbox.GetComponent<ShipInfoManager>();
            GetStats();
        }

        void Start()
        {
            Debug.Log("*** [" + DateTime.Now.ToLongTimeString() + "] Starting server on " + Packet.GetIP4Address() + " ***");

            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ResetClientList();
            newlyConnectedClients.Clear();

            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(Packet.GetIP4Address()), 8888);
            listenerSocket.Bind(ipe);

            listenThread = new Thread(ListenThread);
            listenThread.Start();
        }
        #endregion

        #region GetStats
        private void GetStats()
        {
            maxNumberOfClientsInLobby = lib.serverVariables.maxNumberOfClientsInLobby;
        }
        #endregion

        #region OnEnable & OnDisable
        private void OnEnable()
        {
            em.OnShipSpawnByServer += OnShipSpawnByServer;
            em.OnStartingMatchByServer += OnStartingMatchByServer;
            em.OnProjectileSpawned += OnProjectileSpawned;
            em.OnProjectileHitShip += OnProjectileHitShip;
            em.OnProjectileDestroyed += OnProjectileDestroyed;
            em.OnShipDead += OnShipDead;
            em.OnMatchEnded += OnMatchEnded;

            clientsDoneWithInitializingMatch = 0;
            shipInfoUpdateTimer = 0;
        }

        private void OnDisable()
        {
            em.OnShipSpawnByServer -= OnShipSpawnByServer;
            em.OnStartingMatchByServer -= OnStartingMatchByServer;
            em.OnProjectileSpawned -= OnProjectileSpawned;
            em.OnProjectileHitShip -= OnProjectileHitShip;
            em.OnProjectileDestroyed -= OnProjectileDestroyed;
            em.OnShipDead -= OnShipDead;
            em.OnMatchEnded -= OnMatchEnded;

            #region Clear all connections
            if (listenThread != null)
            {
                listenThread.Abort();
                Debug.Log("ListenThread aborted");
            }

            if (listenerSocket != null)
            {
                Debug.Log("ListenerSocket closed");
                listenerSocket.Close();
            }

            ResetClientList();
            #endregion
        }
        #endregion

        #region Subscribers
        private void OnStartingMatchByServer(int newNumberOfShips)
        {
            numberOfShips = newNumberOfShips;

            Debug.Log("Starting match from server!");
            inGame = true;

            Packet p = new Packet(PacketType.GAMESTART, "Server");
            p.GdataInts.Add(0);
            p.GdataInts.Add(numberOfShips);
            for (int i = 0; i < _clientsInLobby.Count; i++)
            {
                _clientsInLobby[i].clientSocket.Send(p.ToBytes());
            }
        }

        private void OnDeniedStartMatchByServer()
        {
            alreadyRequestingMatchStart = false;
        }

        private void OnShipSpawnByServer(int shipIndex, int spawnPointIndex, int shipColorIndex, string ownerID)
        {
            Packet p = new Packet(PacketType.SPAWN, "Server");
            p.GdataInts.Add(shipIndex);
            p.GdataInts.Add(spawnPointIndex);
            p.GdataInts.Add(shipColorIndex);
            p.GdataStrings.Add(ownerID);

            for (int i = 0; i < _clientsInLobby.Count; i++)
            {

                if (_clientsInLobby[i].id == ownerID)
                {
                    _clientsInLobby[i].shipIndex = shipIndex;
                }

                try
                {
                    _clientsInLobby[i].clientSocket.Send(p.ToBytes());
                }
                catch
                {
                    DisconnectClient(_clientsInLobby[i]);
                    i--;
                }
            }

        }

        private void OnProjectileSpawned(int projectileOwnerIndex, int projectileIndex, Vector3 spawnPosition, Vector3 spawnRotation)
        {
            Packet p = new Packet(PacketType.PROJECTILE, "Server");
            p.GdataInts.Add(0);
            p.GdataInts.Add(projectileOwnerIndex);
            p.GdataInts.Add(projectileIndex);
            p.GdataVectors.Add(new Vector_3(spawnPosition));
            p.GdataVectors.Add(new Vector_3(spawnRotation));

            for (int i = 0; i < _clientsInLobby.Count; i++)
            {
                if (_clientsInLobby[i].shipIndex != projectileOwnerIndex)
                {
                    try
                    {
                        _clientsInLobby[i].clientSocket.Send(p.ToBytes());
                    }
                    catch
                    {
                        DisconnectClient(_clientsInLobby[i]);
                        i--;
                    }
                }
            }
        }

        private void OnProjectileHitShip(int projectileOwnerIndex, int projectileIndex,
            int hitShipIndex, float projectileDamage)
        {
            Packet p = new Packet(PacketType.PROJECTILE, "Server");
            p.GdataInts.Add(1);
            p.GdataInts.Add(projectileOwnerIndex);
            p.GdataInts.Add(projectileIndex);
            p.GdataInts.Add(hitShipIndex);
            p.GdataFloats.Add(projectileDamage);

            for (int i = 0; i < _clientsInLobby.Count; i++)
            {
                if (_clientsInLobby[i].shipIndex != projectileOwnerIndex)
                {
                    try
                    {
                        _clientsInLobby[i].clientSocket.Send(p.ToBytes());
                    }
                    catch
                    {
                        DisconnectClient(_clientsInLobby[i]);
                        i--;
                    }
                }
            }
        }

        private void OnProjectileDestroyed(int projectileOwnerIndex, int projectileIndex, Vector3 location)
        {
            Packet p = new Packet(PacketType.PROJECTILE, "Server");
            p.GdataInts.Add(0);
            p.GdataInts.Add(projectileOwnerIndex);
            p.GdataInts.Add(projectileIndex);
            p.GdataVectors.Add(new Vector_3(location));

            for (int i = 0; i < _clientsInLobby.Count; i++)
            {
                if (_clientsInLobby[i].shipIndex != projectileOwnerIndex)
                {
                    try
                    {
                        _clientsInLobby[i].clientSocket.Send(p.ToBytes());
                    }
                    catch
                    {
                        DisconnectClient(_clientsInLobby[i]);
                        i--;
                    }
                }
            }
        }

        private void OnShipDead(int shipIndex, int killerIndex)
        {
            Packet p = new Packet(PacketType.DEATH, "Server");
            p.GdataInts.Add(0);
            p.GdataInts.Add(shipIndex);
            p.GdataInts.Add(killerIndex);

            for (int i = 0; i < _clientsInLobby.Count; i++)
            {
                try
                {
                    _clientsInLobby[i].clientSocket.Send(p.ToBytes());
                }
                catch
                {
                    DisconnectClient(_clientsInLobby[i]);
                    i--;
                }
            }
        }

        private void OnMatchEnded(int winnerIndex)
        {
            Debug.Log("NetworkServer, OnMatchEnded called");
            string winnerName = "AI " + winnerIndex.ToString();

            for (int i = 0; i < _clientsInLobby.Count; i++)
            {
                if (_clientsInLobby[i].shipIndex == winnerIndex)
                {
                    winnerName = _clientsInLobby[i].clientName;
                }
            }

            Packet p = new Packet(PacketType.GAMEEND, "Server");
            p.GdataInts.Add(winnerIndex);
            p.GdataStrings.Add(winnerName);

            foreach (ClientData client in _clientsInLobby)
            {
                try
                {
                    client.clientSocket.Send(p.ToBytes());
                    Debug.Log("NetworkServer, OnMatchEnded packet sent to client");
                }
                catch
                {
                    DisconnectClient(client);
                }
            }
        }
        #endregion
        #endregion

        #region FixedUpdate
        private void FixedUpdate()
        {
            #region Client connecting & disconnecting broadcasts
            if (newlyConnectedClients.Count > 0)
            {
                for (int i = 0; i < newlyConnectedClients.Count; i++)
                {
                    if (newlyConnectedClients[0] != null)
                        em.BroadcastClientConnected(newlyConnectedClients[0]);
                    newlyConnectedClients.RemoveAt(0);
                    i--;
                }
            }

            if (newlyDisconnectedClients.Count > 0)
            {
                for (int i = 0; i < newlyDisconnectedClients.Count; i++)
                {
                    if (newlyDisconnectedClients[0] != null)
                        em.BroadcastClientDisconnected(newlyDisconnectedClients[0]);
                    newlyDisconnectedClients.RemoveAt(0);
                    i--;
                }
            }
            #endregion

            #region Lobby events
            #region Entering and exiting lobby
            #region Entering Lobby
            if (lobbyQueue.Count > 0)
            {
                for (int i = 0; i < lobbyQueue.Count; i++)
                {
                    Packet p = new Packet(PacketType.LOBBYEVENT, "Server");
                    if (RequestLobbyAccess(lobbyQueue[i]))
                    {
                        //_clientsInLobby.Add(lobbyQueue[i]);
                        Debug.Log("Lobby is available, sending positive");
                        p.GdataInts.Add(0);
                        p.GdataInts.Add(1);
                        lobbyQueue[i].clientSocket.Send(p.ToBytes());

                        //Send lobby enter event to all clients
                        Packet p2 = new Packet(PacketType.LOBBYEVENT, "Server");
                        p2.GdataInts.Add(1);
                        p2.GdataInts.Add(_clientsInLobby.Count);
                        for (int j = 0; j < _clients.Count; j++)
                        {
                            try
                            {
                                _clients[j].clientSocket.Send(p2.ToBytes());
                            }
                            catch
                            {
                                DisconnectClient(_clients[j]);
                                j--;
                            }
                        }
                    }
                    else
                    {
                        p.GdataInts.Add(0);
                        p.GdataInts.Add(0);
                        lobbyQueue[i].clientSocket.Send(p.ToBytes());
                        Debug.Log("Lobby is full, sending negative");
                    }
                    lobbyQueue.RemoveAt(i);
                    i--;
                }
            }
            #endregion

            #region Exiting lobby
            if (clientsExitingLobby.Count > 0)
            {
                Debug.Log("clientsExitingLobby.Count > 0");
                for (int i = 0; i < clientsExitingLobby.Count; i++)
                {
                    Debug.Log("Client exiting lobby");
                    em.BroadcastClientExitLobby(clientsExitingLobby[i]);
                    for (int j = 0; j < _clientsInLobby.Count; j++)
                    {
                        if (_clientsInLobby[j].id == clientsExitingLobby[i].id)
                        {
                            _clientsInLobby.RemoveAt(j);
                            foreach (ClientData client in _clients)
                            {
                                if (client.id == clientsExitingLobby[i].id)
                                {
                                    client.inLobby = false;
                                    client.isReady = false;
                                    em.BroadcastClientVote(client.id, 0);
                                }
                            }
                        }
                    }

                    Packet p = new Packet(PacketType.LOBBYEVENT, "Server");
                    p.GdataInts.Add(1);
                    p.GdataInts.Add(_clientsInLobby.Count);
                    foreach (ClientData client in _clients)
                    {
                        if (client.id != clientsExitingLobby[i].id)
                        {
                            client.clientSocket.Send(p.ToBytes());
                        }
                    }
                    clientsExitingLobby.RemoveAt(i);
                    i--;
                }
            }
            #endregion
            #endregion

            #region Lobby ready state
            if (readyQueue.Count > 0)
            {
                for (int i = 0; i < readyQueue.Count; i++)
                {
                    em.BroadcastClientVote(readyQueue[i].id, 1);
                    Packet p = new Packet(PacketType.LOBBYEVENT, "Server");
                    p.GdataInts.Add(2);
                    p.GdataInts.Add(em.BroadcastRequestReadyClientCount());
                    foreach (ClientData client in _clientsInLobby)
                    {
                        if (client.id == readyQueue[i].id)
                        {
                            client.isReady = true;
                        }

                        client.clientSocket.Send(p.ToBytes());
                    }
                    readyQueue.RemoveAt(i);
                    i--;
                }
            }

            if (notReadyQueue.Count > 0)
            {
                for (int i = 0; i < notReadyQueue.Count; i++)
                {
                    em.BroadcastClientVote(notReadyQueue[i].id, 0);
                    Packet p = new Packet(PacketType.LOBBYEVENT, "Server");
                    p.GdataInts.Add(2);
                    p.GdataInts.Add(em.BroadcastRequestReadyClientCount());
                    foreach (ClientData client in _clientsInLobby)
                    {
                        if (client.id == notReadyQueue[i].id)
                        {
                            client.isReady = false;
                        }

                        client.clientSocket.Send(p.ToBytes());
                    }
                    notReadyQueue.RemoveAt(i);
                    i--;
                }
            }
            #endregion

            #region Starting and restarting a match
            if (requestMatchStart)
            {
                requestMatchStart = false;
                em.BroadcastRequestMatchStart();
            }

            if (matchBeginTimerStarted)
            {
                matchBeginTimerStarted = false;
                em.BroadcastMatchStartTimerStart();
            }

            if (requestMatchRestart)
            {
                requestMatchRestart = false;
                em.BroadcastRequestMatchRestart();
                clientsDoneWithInitializingMatch = 0;
                inGame = true;

                Packet p = new Packet(PacketType.GAMESTART, "Server");
                p.GdataInts.Add(0);
                p.GdataInts.Add(numberOfShips);

                for (int i = 0; i < _clientsInLobby.Count; i++)
                {
                    try
                    {
                        _clientsInLobby[i].clientSocket.Send(p.ToBytes());
                    }
                    catch (SocketException ex)
                    {
                        Debug.Log("SocketException: " + ex);
                        DisconnectClient(_clientsInLobby[i]);
                    }
                }

            }
            #endregion

            #region Returning to lobby from match
            if (requestReturnToLobbyFromMatch)
            {
                requestReturnToLobbyFromMatch = false;
                ReturnToLobby();
            }
            #endregion
            #endregion

            #region Ship info and gameplay events
            #region ShipInfo
            if (inGame)
            {
                shipInfoUpdateTimer += Time.fixedDeltaTime;
                if (shipInfoUpdateTimer >= (1 / shipInfoUpdatesPerSecond))
                {
                    shipInfoUpdateTimer = 0;

                    if (shipInfoManager.shipInfoList.Count > 0)
                    {
                        foreach (ShipInfo shipInfo in shipInfoManager.shipInfoList)
                        {
                            //if (!shipInfo.isDead)
                            //{

                            //}
                            Packet p = new Packet(PacketType.SHIPINFO, "Server");
                            p.GdataInts.Add(shipInfo.shipIndex);
                            Debug.Log("Sending ship " + shipInfo.shipIndex + " info to clients " + Time.time);
                            p.GdataInts.Add(shipInfo.spawnPointIndex);
                            p.GdataInts.Add(shipInfo.shipColorIndex);
                            if (shipInfo.isDead)
                            {
                                p.GdataInts.Add(1);
                            }
                            else
                            {
                                p.GdataInts.Add(0);
                            }
                            p.GdataStrings.Add(shipInfo.ownerID);
                            p.GdataFloats.Add(shipInfo.currentHealth);
                            p.GdataVectors.Add(new Vector_3(shipInfo.shipPosition));
                            p.GdataVectors.Add(new Vector_3(shipInfo.hullRotation));
                            p.GdataVectors.Add(new Vector_3(shipInfo.turretRotation));

                            for (int i = 0; i < _clientsInLobby.Count; i++)
                            {
                                if (_clientsInLobby[i].id != shipInfo.ownerID)
                                {
                                    try
                                    {
                                        _clientsInLobby[i].clientSocket.Send(p.ToBytes());
                                    }
                                    catch (SocketException ex)
                                    {
                                        Debug.Log("SocketException: " + ex);
                                        DisconnectClient(_clientsInLobby[i]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Projectile spawning from client
            if (newlySpawnedProjectiles.Count > 0)
            {
                for (int i = 0; i < newlySpawnedProjectiles.Count; i++)
                {
                    em.BroadcastProjectileSpawnedByClient(newlySpawnedProjectiles[i].projectileOwnerIndex,
                        newlySpawnedProjectiles[i].projectileIndex, newlySpawnedProjectiles[i].spawnPosition,
                        newlySpawnedProjectiles[i].spawnRotation);

                    Packet p = new Packet(PacketType.PROJECTILE, "Server");
                    p.GdataInts.Add(0);
                    p.GdataInts.Add(newlySpawnedProjectiles[i].projectileOwnerIndex);
                    p.GdataInts.Add(newlySpawnedProjectiles[i].projectileIndex);
                    p.GdataVectors.Add(new Vector_3(newlySpawnedProjectiles[i].spawnPosition));
                    p.GdataVectors.Add(new Vector_3(newlySpawnedProjectiles[i].spawnRotation));

                    for (int j = 0; j < _clientsInLobby.Count; j++)
                    {
                        if (_clientsInLobby[j].id != newlySpawnedProjectiles[i].infoSenderID)
                        {
                            try
                            {
                                _clientsInLobby[j].clientSocket.Send(p.ToBytes());
                            }
                            catch (SocketException ex)
                            {
                                Debug.Log("SocketException: " + ex);
                                DisconnectClient(_clientsInLobby[j]);
                            }
                        }
                    }


                    newlySpawnedProjectiles.RemoveAt(i);
                    i--;
                }
            }
            #endregion

            #region Projectile hits from client
            if (newlyHitProjectiles.Count > 0)
            {
                for (int i = 0; i < newlyHitProjectiles.Count; i++)
                {
                    em.BroadcastProjectileHitShipByClient(newlyHitProjectiles[i].projectileOwnerIndex,
                        newlyHitProjectiles[i].projectileIndex, newlyHitProjectiles[i].hitShipIndex,
                        newlyHitProjectiles[i].projectileDamage);

                    Packet p = new Packet(PacketType.PROJECTILE, "Server");
                    p.GdataInts.Add(0);
                    p.GdataInts.Add(newlyHitProjectiles[i].projectileOwnerIndex);
                    p.GdataInts.Add(newlyHitProjectiles[i].projectileIndex);
                    p.GdataVectors.Add(new Vector_3(newlyHitProjectiles[i].spawnPosition));
                    p.GdataVectors.Add(new Vector_3(newlyHitProjectiles[i].spawnRotation));

                    for (int j = 0; j < _clientsInLobby.Count; j++)
                    {
                        if (_clientsInLobby[j].id != newlyHitProjectiles[i].infoSenderID)
                        {
                            try
                            {
                                _clientsInLobby[j].clientSocket.Send(p.ToBytes());
                            }
                            catch (SocketException ex)
                            {
                                Debug.Log("SocketException: " + ex);
                                DisconnectClient(_clientsInLobby[j]);
                            }
                        }
                    }

                    newlyHitProjectiles.RemoveAt(i);
                    i--;
                }
            }
            #endregion

            #region Projectile destruction from client
            if (newlyDestroyedProjectiles.Count > 0)
            {
                for (int i = 0; i < newlyDestroyedProjectiles.Count; i++)
                {
                    em.BroadcastProjectileDestroyedByClient(newlyDestroyedProjectiles[i].projectileOwnerIndex,
                        newlyDestroyedProjectiles[i].projectileIndex, newlyDestroyedProjectiles[i].hitLocation);

                    Packet p = new Packet(PacketType.PROJECTILE, "Server");
                    p.GdataInts.Add(0);
                    p.GdataInts.Add(newlyDestroyedProjectiles[i].projectileOwnerIndex);
                    p.GdataInts.Add(newlyDestroyedProjectiles[i].projectileIndex);
                    p.GdataVectors.Add(new Vector_3(newlyDestroyedProjectiles[i].spawnPosition));
                    p.GdataVectors.Add(new Vector_3(newlyDestroyedProjectiles[i].spawnRotation));

                    for (int j = 0; j < _clientsInLobby.Count; j++)
                    {
                        if (_clientsInLobby[j].id != newlyDestroyedProjectiles[i].infoSenderID)
                        {
                            try
                            {
                                _clientsInLobby[j].clientSocket.Send(p.ToBytes());
                            }
                            catch (SocketException ex)
                            {
                                Debug.Log("SocketException: " + ex);
                                DisconnectClient(_clientsInLobby[j]);
                            }
                        }
                    }

                    newlyDestroyedProjectiles.RemoveAt(i);
                    i--;
                }
            }
            #endregion
            #endregion
        }
        #endregion

        #region Return clients back to lobby
        private void ReturnToLobby()
        {
            clientsDoneWithInitializingMatch = 0;
            inGame = false;
            alreadyRequestingMatchStart = false;
            requestMatchStart = false;
            em.BroadcastRequestReturnToLobbyFromMatch();

            Packet p = new Packet(PacketType.LOBBYEVENT, "Server");
            p.GdataInts.Add(3);
            p.GdataInts.Add(_clientsInLobby.Count);
            for (int i = 0; i < _clients.Count; i++)
            {
                try
                {
                    _clients[i].clientSocket.Send(p.ToBytes());
                }
                catch
                {
                    DisconnectClient(_clients[i]);
                    i--;
                }
            }
        }
        #endregion

        #region Lobby access
        private bool RequestLobbyAccess(ClientData newClientData)
        {
            foreach (ClientData client in _clientsInLobby)
            {
                if (client == newClientData)
                {
                    client.inLobby = true;
                    Debug.LogWarning("Client already in lobby!");
                    return true;
                }
            }

            if (maxNumberOfClientsInLobby > _clientsInLobby.Count)
            {
                ClientData tmp = new ClientData();
                foreach (ClientData client in _clients)
                {
                    if (client.id == newClientData.id)
                    {
                        client.inLobby = true;
                        tmp = client;
                    }
                }

                _clientsInLobby.Add(tmp);
                em.BroadcastClientEnterLobby(tmp);
                return true;
            }

            return false;
        }
        #endregion

        #region ListenThread
        static void ListenThread()
        {
            while (true)
            {
                listenerSocket.Listen(0);
                ClientData newClient = new ClientData(listenerSocket.Accept());
                if (!newClient.disconnected)
                {
                    _clients.Add(newClient);
                    Debug.Log("New client connected. _clients.Count: " + _clients.Count);
                }
                else
                {
                    Debug.Log("New client disconnected instantly. _clients.Count: " + _clients.Count);
                }

            }
        }
        #endregion

        #region Incoming Data thread [aka ClientThread]
        public static void Data_IN(object cSocket)
        {
            Socket clientSocket = (Socket)cSocket;
            byte[] Buffer;
            int readBytes;
            bool running = true;

            while (running)
            {
                try
                {
                    Buffer = new byte[clientSocket.SendBufferSize];
                    readBytes = clientSocket.Receive(Buffer);

                    if (readBytes > 0)
                    {
                        //Debug.Log("Before calling DataManager, readBytes: " + readBytes);
                        try
                        {
                            Packet packet = new Packet(Buffer);
                            if (!packet.errorEncountered)
                            {
                                DataManager(packet, clientSocket);
                            }
                            else
                            {
                                Debug.LogWarning("ErrorEncountered when creating packet from buffer, discarding packet");
                            }
                        }
                        catch
                        {
                            //Debug.LogWarning("Unable to create packet from buffer bytes! readBytes: " + readBytes);
                        }
                    }
                }
                catch (SocketException ex)
                {
                    Debug.Log("SocketException: " + ex);
                    running = false;

                    for (int i = 0; i < _clients.Count; i++)
                    {
                        if (_clients[i].clientSocket == clientSocket)
                        {
                            Debug.Log("Disconnectable client found");
                            DisconnectClient(_clients[i]);
                            i--;
                        }
                    }
                }
            }
        }
        #endregion;

        #region Disconnecting clients
        public static void DisconnectClient(ClientData client)
        {
            Debug.LogWarning("DisconnectClient");
            if (client != null)
            {
                newlyDisconnectedClients.Add(client);

                Thread clientThread = client.clientThread;

                client.clientSocket.Close();
                client.isReady = false;
                client.inLobby = false;
                client.disconnected = true;
                //_clientsInLobby.Remove(client);
                clientsExitingLobby.Add(client);
                _clients.Remove(client);
                Debug.Log("Client disconnected. _clients.Count: " + _clients.Count);

                clientThread.Abort();
            }

            if (inGame)
            {
                requestReturnToLobbyFromMatch = true;
            }
        }

        private void ResetClientList()
        {
            for (int i = 0; i < _clients.Count; i++)
            {
                if (_clients[i] != null)
                {
                    newlyDisconnectedClients.Add(_clients[i]);

                    if (_clients[i].clientThread != null)
                        _clients[i].clientThread.Abort();

                    if (_clients[i].clientSocket != null)
                        _clients[i].clientSocket.Close();
                }

                _clients[i].isReady = false;
                _clients[i].inLobby = false;
                _clients[i].disconnected = true;
                _clientsInLobby.Remove(_clients[i]);
                clientsExitingLobby.Add(_clients[i]);
                _clients.Remove(_clients[i]);
                i--;
                Debug.Log("Client removed. _clients.Count: " + _clients.Count);
            }
            _clientsInLobby.Clear();
            _clients.Clear();
        }
        #endregion

        #region DataManager
        static void DataManager(Packet p, Socket clientSocket)
        {
            if (p.packetType == PacketType.GAMESTART)
                Debug.Log("DataManager called, packetType: " + p.packetType);
            #region Finding clientData corresponding to senderID
            if (p.senderID == null)
            {
                Debug.LogWarning("SenderID null, searching through clientList");
                for (int i = 0; i < _clients.Count; i++)
                {
                    if (_clients[i].clientSocket == clientSocket)
                    {
                        p.senderID = _clients[i].id;
                        Debug.Log("SenderID found!!");
                    }
                }
            }
            else
            {
                //Debug.Log("SenderID received with packet data");
            }
            #endregion

            switch (p.packetType)
            {
                case PacketType.REGISTRATION:
                    #region Registration packet handling
                    #endregion
                    break;

                case PacketType.DEBUG:
                    Debug.Log("PacketType Debug: " + p.GdataStrings[0]);
                    break;

                case PacketType.LOBBYEVENT:
                    #region LobbyEvents
                    int listCount = p.GdataInts.Count;
                    int tmp1 = -1;
                    int tmp2 = -1;
                    if (listCount > 0)
                    {
                        tmp1 = p.GdataInts[0];
                    }
                    if (listCount > 1)
                    {
                        tmp2 = p.GdataInts[1];
                    }
                    #region Lobby joining handling
                    if (tmp1 == 0)
                    {
                        if (tmp2 == 1)
                        {
                            foreach (ClientData client in _clients)
                            {
                                if (client.id == p.senderID)
                                {
                                    client.clientName = p.GdataStrings[0];
                                    client.inLobby = true;
                                    client.isReady = false;
                                    lobbyQueue.Add(client);
                                }
                                else
                                {
                                    Debug.LogWarning("No client found with senderID, resending registration packet!");
                                    client.SendRegistrationPacket();
                                }
                            }
                        }
                        else if (tmp2 == 0)
                        {
                            foreach (ClientData client in _clients)
                            {
                                if (client.id == p.senderID)
                                {
                                    client.inLobby = false;
                                    client.isReady = false;
                                    clientsExitingLobby.Add(client);
                                    Debug.Log("Client found, adding client to clientsExitingLobby list");
                                }
                            }
                        }

                        Debug.Log("Lobby join packet managed");
                    }
                    #endregion

                    #region Lobby ready state handling
                    else if (tmp1 == 2)
                    {
                        if (tmp2 == 1)
                        {
                            foreach (ClientData client in _clients)
                            {
                                if (client.id == p.senderID)
                                {
                                    readyQueue.Add(client);
                                }
                                else
                                {
                                    Debug.LogWarning("No client found with senderID, resending registration packet!");
                                    client.SendRegistrationPacket();
                                }
                            }
                        }
                        else if (tmp2 == 0)
                        {
                            foreach (ClientData client in _clients)
                            {
                                if (client.id == p.senderID)
                                {
                                    notReadyQueue.Add(client);
                                    Debug.Log("Client found and added to notReadyQueue");
                                }
                                else
                                {
                                    Debug.LogWarning("No client found with senderID, resending registration packet!");
                                    client.SendRegistrationPacket();
                                }
                            }
                        }

                        Debug.Log("Lobby ready state packet managed");
                    }
                    #endregion

                    #region Lobby start match request handling
                    else if (tmp1 == 3)
                    {
                        if (!inGame && !requestMatchStart && !alreadyRequestingMatchStart)
                        {
                            alreadyRequestingMatchStart = true;
                            requestMatchStart = true;
                        }
                        else
                        {
                            Debug.LogWarning("Match already running!");
                        }
                    }
                    #endregion

                    #region Returning to lobby from match
                    else if (tmp1 == 4)
                    {
                        requestReturnToLobbyFromMatch = true;
                        Debug.Log("Requesting return to lobby from match");
                    }
                    #endregion
                    #endregion
                    break;

                case PacketType.GAMESTART:
                    #region GameStart
                    int tmp = p.GdataInts[0];
                    if (tmp == 1)
                    {
                        clientsDoneWithInitializingMatch++;
                        if (clientsDoneWithInitializingMatch >= _clientsInLobby.Count)
                        {
                            Packet p1 = new Packet(PacketType.GAMESTART, "Server");
                            p1.GdataInts.Add(1);
                            foreach (ClientData client in _clientsInLobby)
                            {
                                client.clientSocket.Send(p1.ToBytes());
                            }
                            matchBeginTimerStarted = true;
                            alreadyRequestingMatchStart = false;
                        }
                    }
                    else if (tmp == 2)
                    {
                        if (!requestMatchRestart && !alreadyRequestingMatchStart)
                        {
                            //Restarting game
                            requestMatchRestart = true;
                            Debug.LogWarning("Match restart request received from a client");
                            alreadyRequestingMatchStart = true;
                        }
                    }
                    #endregion
                    break;

                case PacketType.PROJECTILE:
                    #region Projectile
                    if (p.GdataInts[0] == 0)
                    {
                        ProjectileInfo newProjectileInfo = new ProjectileInfo();
                        newProjectileInfo.infoSenderID = p.senderID;
                        newProjectileInfo.projectileOwnerIndex = p.GdataInts[1];
                        newProjectileInfo.projectileIndex = p.GdataInts[2];
                        newProjectileInfo.spawnPosition = p.GdataVectors[0].ToVector3();
                        newProjectileInfo.spawnRotation = p.GdataVectors[1].ToVector3();
                        newlySpawnedProjectiles.Add(newProjectileInfo);
                    }
                    else if (p.GdataInts[0] == 1)
                    {
                        ProjectileInfo newProjectileInfo = new ProjectileInfo();
                        newProjectileInfo.infoSenderID = p.senderID;
                        newProjectileInfo.projectileOwnerIndex = p.GdataInts[1];
                        newProjectileInfo.projectileIndex = p.GdataInts[2];
                        newProjectileInfo.hitShipIndex = p.GdataInts[3];
                        newProjectileInfo.projectileDamage = p.GdataFloats[0];
                        newlyHitProjectiles.Add(newProjectileInfo);
                    }
                    else if (p.GdataInts[0] == 2)
                    {
                        ProjectileInfo newProjectileInfo = new ProjectileInfo();
                        newProjectileInfo.infoSenderID = p.senderID;
                        newProjectileInfo.projectileOwnerIndex = p.GdataInts[1];
                        newProjectileInfo.projectileIndex = p.GdataInts[2];
                        newProjectileInfo.hitLocation = p.GdataVectors[0].ToVector3();
                        newlyDestroyedProjectiles.Add(newProjectileInfo);
                    }
                    #endregion
                    break;

                case PacketType.SHIPINFO:
                    #region ShipInfo packet handling
                    int shipInfoListElement1 = shipInfoManager.GetMyShipInfoElement(p.GdataInts[0]);
                    if (shipInfoListElement1 != -1)
                    {
                        if(p.GdataInts[1] == 1)
                        {
                            shipInfoManager.shipInfoList[shipInfoListElement1].isDead = true;
                        }
                        shipInfoManager.shipInfoList[shipInfoListElement1].currentHealth = p.GdataFloats[0];
                        shipInfoManager.shipInfoList[shipInfoListElement1].shipPosition = p.GdataVectors[0].ToVector3();
                        shipInfoManager.shipInfoList[shipInfoListElement1].hullRotation = p.GdataVectors[1].ToVector3();
                        shipInfoManager.shipInfoList[shipInfoListElement1].turretRotation = p.GdataVectors[2].ToVector3();
                    }
                    #endregion
                    break;

                case PacketType.DEATH:
                    #region Death
                    int shipInfoListElement2 = shipInfoManager.GetMyShipInfoElement(p.GdataInts[0]);
                    if (shipInfoListElement2 != -1)
                    {
                        shipInfoManager.shipInfoList[shipInfoListElement2].killerIndex = p.GdataInts[1];
                        shipInfoManager.shipInfoList[shipInfoListElement2].isDead = true;
                    }
                    #endregion
                    break;

            }
        }
        #endregion
    }

    #region ClientData
    public class ClientData
    {
        public Socket clientSocket;
        public Thread clientThread;
        public string id;
        public string clientName;
        public bool disconnected = false;
        public bool inLobby = false;
        public bool isReady = false;
        public int shipIndex = -1;

        public ClientData()
        {
            id = "empty";
        }

        public ClientData(Socket clientSocket)
        {
            this.clientSocket = clientSocket;
            id = Guid.NewGuid().ToString();
            clientThread = new Thread(NetworkServer.Data_IN);
            clientThread.Start(clientSocket);
            NetworkServer.newlyConnectedClients.Add(this);
            SendRegistrationPacket();

            //No longer neccessary?
            //Thread.Sleep(1000);
            //try
            //{
            //    SendRegistrationPacket();
            //}
            //catch
            //{
            //    NetworkServer.DisconnectClient(this);
            //}
        }

        public void SendRegistrationPacket()
        {
            Packet p = new Packet(PacketType.REGISTRATION, "Server");
            p.GdataStrings.Add(id);
            clientSocket.Send(p.ToBytes());
        }
    }
    #endregion

}
