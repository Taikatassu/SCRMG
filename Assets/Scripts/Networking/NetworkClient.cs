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
     */

    public class NetworkClient : MonoBehaviour
    {
        Core_Toolbox toolbox;
        Core_EventManager em;
        Core_GameManager gm;
        Core_GlobalVariableLibrary lib;
        public static Socket master;
        Thread incomingDataThread;
        public static string id;
        bool connected = false;
        bool inGame = false;
        int currentGameModeIndex = -1;

        int gameModeSingleplayerIndex = -1;
        int gameModeNetworkMultiplayerIndex = -1;
        int gameModeLocalMultiplayerIndex = -1;
        bool networkFunctionalityDisabled = false;

        void Awake()
        {
            toolbox = FindObjectOfType<Core_Toolbox>();
            em = toolbox.GetComponent<Core_EventManager>();
            gm = toolbox.GetComponent<Core_GameManager>();
            lib = toolbox.GetComponent<Core_GlobalVariableLibrary>();
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
            em.OnTryConnectToNetwork += OnTryConnectToNetwork;
            inGame = false;
        }

        void OnDisable()
        {
            em.OnNewSceneLoaded -= OnNewSceneLoaded;
            em.OnSetGameMode -= OnSetGameMode;
            em.OnTryConnectToNetwork -= OnTryConnectToNetwork;

            if (incomingDataThread != null)
            {
                incomingDataThread.Abort();
                Debug.Log("IncomingData thread aborted");
            }

            if (master != null)
            {
                master.Close();
                Debug.Log("MasterSocket closed");
            }
        }
        #endregion

        #region Subscribers
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
            else if(sceneIndex == 1)
            {
                inGame = true;
            }
        }

        private void OnTryConnectToNetwork(string ip)
        {
            if (!connected)
            {
                TryConnectingToHost(ip);
            }
            else
            {
                Debug.LogError("NetworkClient, OnTryConnectToNetwork: Already connected!!");
            }
        }
        #endregion

        #region Connecting and disconnecting
        private void TryConnectingToHost(string ip)
        {
            try
            {
                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), 8888);
                Debug.Log("Trying to connect to ip: " + ip);
                master.Connect(ipe);

                incomingDataThread = new Thread(Data_IN);
                incomingDataThread.Start();

                Debug.Log("Connected to host.");
                connected = true;
                em.BroadcastConnectingToNetworkSucceeded(ip);
            }
            catch
            {
                Debug.Log("Could not connect to host.");
                connected = false;
                em.BroadcastConnectingToNetworkFailed(ip);
            }
        }

        private void Disconnect()
        {
            if (incomingDataThread != null)
            {
                incomingDataThread.Abort();
                Debug.Log("IncomingData thread aborted");
            }

            if (master != null)
            {
                master.Close();
                Debug.Log("MasterSocket closed");
            }

            connected = false;
            Debug.Log("Disconnected");
        }
        #endregion

        #region FixedUpdate
        void FixedUpdate()
        {
            if (connected)
            {
                if(currentGameModeIndex == gameModeNetworkMultiplayerIndex)
                {
                    if (inGame)
                    {
                        Debug.Log("NetworkClient: inGame");
                        float timeStamp = Time.time;

                        if (gm.shipInfoList.Count > 0)
                        {
                            Debug.Log("gm.shipInfoList.Count > 0");
                            Packet p = new Packet(PacketType.MOVEMENT, id);
                            //string input = "Hello from the client side " +
                            //DateTime.Now.ToLongTimeString();
                            //p.GdataStrings.Add(input);
                            Vector3 shipPosition = gm.shipInfoList[0].shipPosition;
                            Vector_3 _shipPosition = new Vector_3(shipPosition.x,
                                shipPosition.y, shipPosition.z);
                            p.GdataVectors.Add(_shipPosition);

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
                }          
            }
            //else
            //{
            //    Debug.Log("Not connected");
            //}
        }
        #endregion

        static void Data_IN()
        {
            byte[] Buffer;
            int readBytes;

            while (true)
            {
                try
                {
                    Buffer = new byte[master.SendBufferSize];
                    readBytes = master.Receive(Buffer);

                    if(readBytes > 0)
                    {
                        Debug.Log("NetworkClient, Data_IN: readBytes > 0");
                        DataManager(new Packet(Buffer));
                    }
                }
                catch(SocketException ex)
                {
                    Debug.Log("SocketException: " + ex);
                }
            }
        }

        static void DataManager(Packet p)
        {
            Debug.Log("DataManager called");

            switch (p.packetType)
            {
                case PacketType.REGISTRATION:
                    Debug.Log("Registration packet managed");
                    break;

                case PacketType.DEBUG:
                    Debug.Log("Debug packet managed");
                    break;

                case PacketType.SPAWNEVENT:
                    Debug.Log("SpawnEvent packet managed");
                    break;

                case PacketType.DEATHEVENT:
                    Debug.Log("DeathEvent packet managed");
                    break;

                case PacketType.MOVEMENT:
                    Debug.Log("Movement packet managed");
                    break;

                case PacketType.AIM:
                    Debug.Log("Aim packet managed");
                    break;

                case PacketType.SHOOTEVENT:
                    Debug.Log("ShootEvent packet managed");
                    break;

            }

        }
    }

}
