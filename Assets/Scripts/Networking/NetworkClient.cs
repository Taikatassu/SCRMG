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

        #region Update
        void Update()
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
        }
    }

}
