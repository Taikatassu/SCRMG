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
        public static Socket master;
        public static string id;
        bool connected = false;
        bool inGame = false;
        Thread incomingDataThread;

        void Awake()
        {
            toolbox = FindObjectOfType<Core_Toolbox>();
            em = toolbox.GetComponent<Core_EventManager>();
            gm = toolbox.GetComponent<Core_GameManager>();
        }

        void Start()
        {
            string ip = Packet.GetIP4Address();

            master = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), 8888);

            try
            {
                master.Connect(ipe);
                Debug.Log("Connected to host.");
                connected = true;
            }
            catch
            {
                Debug.Log("Could not connect to host.");
                connected = false;
            }

            incomingDataThread = new Thread(Data_IN);
            incomingDataThread.Start();
        }

        void OnEnable()
        {
            em.OnNewSceneLoaded += OnNewSceneLoaded;
            inGame = false;
        }

        void OnDisable()
        {
            em.OnNewSceneLoaded -= OnNewSceneLoaded;

            if (incomingDataThread != null)
            {
                incomingDataThread.Abort();
                Debug.Log("IncomingData thread aborted");
            }
        }

        #region Subscribers
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
        #endregion

        void Update()
        {
            if (connected)
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

                        master.Send(p.ToBytes());
                    }
                }
            }
            else
            {
                Debug.Log("Not connected");
            }
        }

        static void Data_IN()
        {

        }

    }

}
