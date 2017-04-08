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
     */

    public class NetworkServer : MonoBehaviour
    {
        static ProtoServerShipManager shipManager;
        static Socket listenerSocket;
        public static List<ClientData> _clients;
        static string clientName;
        public static bool clientIsConnected = false;
        Thread listenThread;

        public bool GetConnectedStatus()
        {
            return clientIsConnected;
        }

        void Awake()
        {
            shipManager = FindObjectOfType<ProtoServerShipManager>();
        }

        void Start()
        {
            Debug.Log("*** [" + DateTime.Now.ToLongTimeString() + "] Starting server on " + Packet.GetIP4Address() + " ***");

            listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _clients = new List<ClientData>();

            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(Packet.GetIP4Address()), 8888);
            listenerSocket.Bind(ipe);

            listenThread = new Thread(ListenThread);
            listenThread.Start();
        }

        void OnDisable()
        {
            clientIsConnected = false;
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

        static void ListenThread()
        {
            while (true)
            {
                listenerSocket.Listen(0);
                ClientData newClient = new ClientData(listenerSocket.Accept());
                _clients.Add(newClient);
                Debug.Log("New client connected. _clients.Count: " + _clients.Count);
                clientIsConnected = true;
            }
        }

        public static void Data_IN(object cSocket)
        {
            Socket clientSocket = (Socket)cSocket;

            byte[] Buffer;
            int readBytes;

            while (true)
            {
                try
                {
                    Buffer = new byte[clientSocket.SendBufferSize];
                    readBytes = clientSocket.Receive(Buffer);

                    if (readBytes > 0)
                    {
                        DataManager(new Packet(Buffer));
                    }
                }
                catch (SocketException ex)
                {
                    Debug.Log("SocketException: " + ex);
                    //TODO: Find a way to know if a client disconnects
                    clientIsConnected = false;

                    #region [NOT CURRENTLY IN USE] Disconnection handling
                    //TODO: Find out why the if-statements inside the foreach loop do not work
                    //if (_clients.Count > 0)
                    //{
                    //    Debug.Log("_clients.Count > 0");
                    //    foreach (ClientData clientData in _clients)
                    //    {
                    //        if (clientData.clientThread != null)
                    //        {
                    //            clientData.clientThread.Abort();
                    //            Debug.Log("ClientThread aborted");
                    //        }

                    //        if (clientData.clientSocket != null)
                    //        {
                    //            clientData.clientSocket.Close();
                    //            Debug.Log("ClientSocket closed");
                    //        }
                    //    }
                    //    _clients.Clear();
                    //    Debug.Log("ClientList cleared");
                    //}
                    //Environment.Exit(0);
                    #endregion
                    
                    foreach (ClientData client in _clients)
                    {
                        if (client.clientSocket == clientSocket)
                        {
                            Debug.Log("Disconnectable client found");
                            DisconnectClient(client);
                        }
                    }
                }
            }
        }

        //List<ClientData> clientsToDisconnect = new List<ClientData>();

        private static void DisconnectClient(ClientData client)
        {
            if (client != null)
            {
                Thread clientThread = client.clientThread;
                
                client.clientSocket.Close();
                _clients.Remove(client);
                Debug.Log("Client has disconnected. _clients.Count: " + _clients.Count);

                clientThread.Abort();
            }
        }

        public static void DataManager(Packet p)
        {
            Debug.Log("DataManagr called, packetType: " + p.packetType.ToString());
            switch (p.packetType)
            {
                case PacketType.REGISTRATION:
                    Debug.Log("PacketType Registration: " + p.GdataStrings[0]);
                    break;

                case PacketType.DEBUG:
                    Debug.Log("PacketType Debug: " + p.GdataStrings[0]);
                    break;

                case PacketType.SPAWN:
                    Debug.Log("SpawnEvent packet managed");
                    break;

                case PacketType.DEATH:
                    Debug.Log("DeathEvent packet managed");
                    break;

                case PacketType.MOVEMENT:
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
                    break;

                case PacketType.AIM:
                    Debug.Log("Aim packet managed");
                    break;

                case PacketType.SHOOT:
                    Debug.Log("ShootEvent packet managed");
                    break;

            }
        }
    }

    public class ClientData
    {
        public Socket clientSocket;
        public Thread clientThread;
        public string id;
        public string clientName;

        public ClientData()
        {
            id = Guid.NewGuid().ToString();
            clientThread = new Thread(NetworkServer.Data_IN);
            clientThread.Start(clientSocket);
            Debug.Log("Sending registration packet");
            SendRegistrationPacket();
            Debug.Log("Registration packet sent");
        }

        public ClientData(Socket clientSocket)
        {
            this.clientSocket = clientSocket;
            id = Guid.NewGuid().ToString();
            clientThread = new Thread(NetworkServer.Data_IN);
            clientThread.Start(clientSocket);
            Debug.Log("Sending registration packet");
            SendRegistrationPacket();
            Debug.Log("Registration packet sent");
        }

        public void SendRegistrationPacket()
        {
            Packet p = new Packet(PacketType.REGISTRATION, "Server");
            p.GdataStrings.Add(id);
            clientSocket.Send(p.ToBytes());
        }

    }

}
