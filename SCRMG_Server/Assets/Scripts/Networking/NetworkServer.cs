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
        //TODO: Update client ready states to all clients

        //TODO: When a client requests match start, check if all participants are ready
        //and if so, start initializing the match
        //A match requires four ships. If there are less than four participants, the filler
        //ships will receive AI controller on the server, and the server will send their
        //movement etc info to the clients

        Toolbox toolbox;
        EventManager em;
        GlobalVariableLibrary lib;
        
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
        public static bool requestMatchStart = false;
        public static List<ShipInfo> newestShipMovementInfo = new List<ShipInfo>();
        public static bool matchRunning = false;
        
        bool inGame = false; //TODO: Set this to true when starting a match
        int maxNumberOfClientsInLobby = -1;


        #region Initialization
        #region Awake & Start
        void Awake()
        {
            toolbox = FindObjectOfType<Toolbox>();
            em = toolbox.GetComponent<EventManager>();
            lib = toolbox.GetComponent<GlobalVariableLibrary>();
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
        }

        private void OnDisable()
        {
            em.OnShipSpawnByServer -= OnShipSpawnByServer;
            em.OnStartingMatchByServer -= OnStartingMatchByServer;

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
        private void OnStartingMatchByServer()
        {
            Debug.Log("OnStartingMatchByServer");
            matchRunning = true;
            foreach (ClientData client in _clientsInLobby)
            {
                Debug.Log("Sending match start event to a client");
                Packet p = new Packet(PacketType.GAMESTART, "Server");
                client.clientSocket.Send(p.ToBytes());
            }
        }

        private void OnShipSpawnByServer(int shipIndex, int spawnPointIndex, int shipColorIndex, string ownerID)
        {
            Packet p = new Packet(PacketType.SPAWN, "Server");
            p.GdataInts.Add(shipIndex);
            p.GdataInts.Add(spawnPointIndex);
            p.GdataInts.Add(shipColorIndex);
            p.GdataStrings.Add(ownerID);

            foreach (ClientData client in _clientsInLobby)
            {
                client.clientSocket.Send(p.ToBytes());
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
                for(int i = 0; i < newlyConnectedClients.Count; i++)
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
                Debug.Log("LobbyQueue.Count > 0");
                for(int i = 0; i < lobbyQueue.Count; i++)
                {
                    Debug.Log("Inside lobbyQueue for loop");
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
                        foreach (ClientData client in _clients)
                        {
                            client.clientSocket.Send(p2.ToBytes());
                            Debug.Log("Client enter lobby event sent to a client");
                        }
                        Debug.Log("Client enter lobby event sent to all clients");
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
                    Debug.Log("LobbyJoin response sent back to client");
                }
            }
            #endregion

            #region Exiting lobby
            if (clientsExitingLobby.Count > 0)
            {
                Debug.Log("clientsExitingLobby.Count > 0");
                for (int i = 0; i < clientsExitingLobby.Count; i++)
                {
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
                            Debug.Log("Client exiting lobby removed from clientsInLobby list");
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
                            Debug.Log("Client exit lobby event sent to other client");
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
                Debug.Log("ReadyQueue.Count > 0");
                for (int i = 0; i < readyQueue.Count; i++)
                {
                    Debug.Log("Inside readyQueue for-loop");
                    em.BroadcastClientVote(readyQueue[i].id, 1);
                    Packet p = new Packet(PacketType.LOBBYEVENT, "Server");
                    p.GdataInts.Add(2);
                    p.GdataInts.Add(em.BroadcastRequestReadyClientCount());
                    foreach (ClientData client in _clientsInLobby)
                    {
                        if(client.id == readyQueue[i].id)
                        {
                            client.isReady = true;
                        }

                        client.clientSocket.Send(p.ToBytes());
                        Debug.Log("Client isReadyState sent to a client");
                    }
                    readyQueue.RemoveAt(i);
                    i--;
                    Debug.Log("LobbyIsReady state sent to all other clients");
                }
            }

            if (notReadyQueue.Count > 0)
            {
                Debug.Log("NotReadyQueue.Count > 0");
                for (int i = 0; i < notReadyQueue.Count; i++)
                {
                    Debug.Log("Inside notReadyQueue for-loop");
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
                        Debug.Log("Client notReadyState sent to other client");
                    }
                    notReadyQueue.RemoveAt(i);
                    i--;
                    Debug.Log("LobbyNotReady state sent to all other clients");
                }
            }
            #endregion

            #region Requesting match start
            if (requestMatchStart)
            {
                requestMatchStart = false;
                em.BroadcastRequestMatchStart();
            }
            #endregion

            #region ShipInfo
            if (inGame)
            {
                foreach (ShipInfo shipInfo in newestShipMovementInfo)
                {
                    em.BroadcastShipPositionUpdate(shipInfo.shipIndex, shipInfo.shipPosition);
                }
            }
            #endregion
            #endregion
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
                    if(client.id == newClientData.id)
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
                        Debug.Log("Before calling DataManager, readBytes: " + readBytes);
                        Packet p = new Packet(Buffer);
                        DataManager(p, clientSocket);
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
            if (client != null)
            {
                newlyDisconnectedClients.Add(client);

                Thread clientThread = client.clientThread;
                
                client.clientSocket.Close();
                client.isReady = false;
                client.inLobby = false;
                client.disconnected = true;
                _clientsInLobby.Remove(client);
                clientsExitingLobby.Add(client);
                _clients.Remove(client);
                Debug.Log("Client disconnected. _clients.Count: " + _clients.Count);

                clientThread.Abort();
            }
        }

        private void ResetClientList()
        {
            for(int i = 0; i < _clients.Count; i++)
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
            if(p.senderID == null)
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
                Debug.Log("SenderID received with packet data");
            }

            Debug.Log("DataManagr called, packetType: " + p.packetType.ToString());
            switch (p.packetType)
            {
                case PacketType.REGISTRATION:
                    #region Registration packet handling
                    Debug.Log("PacketType Registration: " + p.GdataInts[0]);
                    #endregion
                    break;

                case PacketType.DEBUG:
                    Debug.Log("PacketType Debug: " + p.GdataStrings[0]);
                    break;

                case PacketType.LOBBYEVENT:
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
                                    Debug.Log("Client found and added to lobbyQueue");
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
                                    Debug.Log("Client found and added to readyQueue");
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
                    else if(tmp1 == 3)
                    {
                        if (!matchRunning && !requestMatchStart)
                        {
                            requestMatchStart = true;
                            Debug.Log("Start match request packet managed");
                        }
                        else
                        {
                            Debug.LogWarning("Match already running!");
                        }
                    }
                    #endregion
                    break;

                case PacketType.MOVEMENT:
                    #region Movement packet handling
                    //Vector_3 _shipPosition = p.GdataVectors[0];
                    //Vector3 shipPosition = new Vector3(_shipPosition.x, 
                    //    _shipPosition.y, _shipPosition.z);
                    //Debug.Log("PacketType Movement: " + shipPosition);
                    //Debug.Log("networkServer, shipInfoList.Count: " + shipManager.shipInfoList.Count);
                    //if (shipManager.shipInfoList.Count <= 0)
                    //{
                    //    Debug.Log("ShipInfoList element 0 is null, creating new shipInfo");
                    //    shipManager.shipInfoList.Add(new ShipInfo());
                    //}

                    //Debug.Log("Adding ship position to shipInfoList[0]");
                    //shipManager.shipInfoList[0].shipPosition = shipPosition;
                    int shipIndex = p.GdataInts[0];
                    Vector_3 _shipPosition = p.GdataVectors[0];
                    Vector3 shipPosition;
                    shipPosition.x = _shipPosition.x;
                    shipPosition.y = _shipPosition.y;
                    shipPosition.z = _shipPosition.z;

                    foreach(ShipInfo shipInfo in newestShipMovementInfo)
                    {
                        if(shipInfo.shipIndex == shipIndex)
                        {
                            Debug.Log("NetworkClient: ShipInfo found with shipIndex, updating position info");
                            shipInfo.shipPosition = shipPosition;
                        }
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
        
        public ClientData()
        {
            //This is only used for empty ClientData variable
            Debug.LogWarning("This is only used for empty ClientData variable");
            id = "empty";
            //id = Guid.NewGuid().ToString();
            //clientThread = new Thread(NetworkServer.Data_IN);
            //clientThread.Start(clientSocket);
            //SendRegistrationPacket();
        }

        public ClientData(Socket clientSocket)
        {
            this.clientSocket = clientSocket;
            id = Guid.NewGuid().ToString();
            clientThread = new Thread(NetworkServer.Data_IN);
            clientThread.Start(clientSocket);
            NetworkServer.newlyConnectedClients.Add(this);
            SendRegistrationPacket();
            Thread.Sleep(1000);

            try
            {
                SendRegistrationPacket();
            }
            catch
            {
                NetworkServer.DisconnectClient(this);
            }

            //TODO: Find out why the first packet is split in two parts 
            //(1460 and 169 bytes, instead of the full message's 1629 bytes)
            //Until then, sending two registration packets with a sleep timer
            //in between is necessary

        }

        public void SendRegistrationPacket()
        {
            Debug.Log("SendRegistrationPacket");
            Packet p = new Packet(PacketType.REGISTRATION, "Server");
            p.GdataStrings.Add(id);
            clientSocket.Send(p.ToBytes());
            Debug.Log("Registration packet sent");      
        }
    }
    #endregion

}
