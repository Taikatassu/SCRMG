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

        static ProtoServerShipManager shipManager;
        static Socket listenerSocket;
        public static List<ClientData> _clients = new List<ClientData>();
        public static List<ClientData> newlyConnectedClients = new List<ClientData>();
        public static List<string> newlyDisconnectedClients = new List<string>();
        public static List<ClientData> lobbyQueue = new List<ClientData>();
        public static List<string> clientsExitingLobby = new List<string>();
        static string clientName;
        Thread listenThread;
        

        #region Awake & Start
        void Awake()
        {
            toolbox = FindObjectOfType<Toolbox>();
            em = toolbox.GetComponent<EventManager>();

            shipManager = FindObjectOfType<ProtoServerShipManager>();
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

        #region OnDisable
        void OnDisable()
        {
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

            if(_clients.Count > 0)
            {
                foreach(ClientData clientData in _clients)
                {
                    if (clientData.clientThread != null)
                    {
                        clientData.clientThread.Abort();
                        Debug.Log("ClientThread aborted");
                    }

                    if(clientData.clientSocket != null)
                    {
                        clientData.clientSocket.Close();
                        Debug.Log("ClientSocket closed");
                    }
                }
                _clients.Clear();
                Debug.Log("ClientList cleared");
            }
        }
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

            #region Entering and exiting lobby
            if(lobbyQueue.Count > 0)
            {
                Debug.Log("LobbyQueue.Count > 0");
                for(int i = 0; i < lobbyQueue.Count; i++)
                {
                    Debug.Log("Inside lobbyQueue for loop");
                    Packet p = new Packet(PacketType.LOBBYEVENT, "Server");
                    if (em.BroadcastClientRequestLobbyAccess(lobbyQueue[0]))
                    {
                        p.GdataInts.Add(1);
                        Debug.Log("Lobby is available, sending positive");
                    }
                    else
                    {
                        p.GdataInts.Add(0);
                        Debug.Log("Lobby is full, sending negative");
                    }
                    lobbyQueue[0].clientSocket.Send(p.ToBytes());
                    lobbyQueue.RemoveAt(0);
                    i--;
                    Debug.Log("LobbyJoin response sent back to client");
                }
            }

            if(clientsExitingLobby.Count > 0)
            {
                for(int i = 0; i < clientsExitingLobby.Count; i++)
                {
                    em.BroadcastClientExitLobby(clientsExitingLobby[0]);
                    clientsExitingLobby.RemoveAt(0);
                    i--;
                }
            }
            #endregion
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
                newlyDisconnectedClients.Add(client.id);

                Thread clientThread = client.clientThread;
                
                client.clientSocket.Close();
                client.disconnected = true;
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
                    newlyDisconnectedClients.Add(_clients[0].id);

                    if (_clients[i].clientThread != null)
                        _clients[i].clientThread.Abort();

                    if (_clients[i].clientSocket != null)
                        _clients[i].clientSocket.Close();
                }

                _clients[i].disconnected = true;
                _clients.RemoveAt(i);
                i--;
                Debug.Log("Client removed. _clients.Count: " + _clients.Count);
            }
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
                    #region LobbyJoin packet handling
                    if(p.GdataInts[0] == 1)
                    {
                        foreach (ClientData client in _clients)
                        {
                            Debug.Log("client.id: " + client.id);
                            Debug.Log("p.senderID: " + p.senderID);
                            if (client.id == p.senderID)
                            {
                                client.clientName = p.GdataStrings[0];
                                lobbyQueue.Add(client);
                                Debug.Log("Client found and added to lobbyQueue");
                            }
                            else
                            {
                                Debug.LogWarning("No client found with senderID!");
                                client.SendRegistrationPacket();
                            }
                        }
                    }
                    else if (p.GdataInts[0] == 0)
                    {
                        clientsExitingLobby.Add(p.senderID);
                    }

                    Debug.Log("LobbyJoin packet managed");
                    #endregion
                    break;

                case PacketType.MOVEMENT:
                    #region Movement packet handling
                    Vector_3 _shipPosition = p.GdataVectors[0];
                    Vector3 shipPosition = new Vector3(_shipPosition.x, 
                        _shipPosition.y, _shipPosition.z);
                    Debug.Log("PacketType Movement: " + shipPosition);
                    Debug.Log("networkServer, shipInfoList.Count: " + shipManager.shipInfoList.Count);
                    if (shipManager.shipInfoList.Count <= 0)
                    {
                        Debug.Log("ShipInfoList element 0 is null, creating new shipInfo");
                        shipManager.shipInfoList.Add(new ShipInfo());
                    }

                    Debug.Log("Adding ship position to shipInfoList[0]");
                    shipManager.shipInfoList[0].shipPosition = shipPosition;
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
        
        public ClientData()
        {
            id = Guid.NewGuid().ToString();
            clientThread = new Thread(NetworkServer.Data_IN);
            clientThread.Start(clientSocket);
            SendRegistrationPacket();
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
        }
    }
    #endregion

}
