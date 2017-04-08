using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using ServerData;
using UnityEngine;

namespace ServerData
{
    [Serializable]
    public class Packet
    {
        public float timeStamp;
        public string senderID;
        public PacketType packetType;
        public List<Vector_3> GdataVectors;
        public List<float> GdataFloats;
        public List<string> GdataStrings;

        public Packet(PacketType type, string senderID)
        {
            this.timeStamp = 0;
            this.senderID = senderID;
            this.packetType = type;
            GdataVectors = new List<Vector_3>();
            GdataFloats = new List<float>();
            GdataStrings = new List<string>();
        }

        public Packet(byte[] packetBytes)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(packetBytes);

            Packet p = (Packet)bf.Deserialize(ms);
            ms.Close();
            this.timeStamp = p.timeStamp;
            this.senderID = p.senderID;
            this.packetType = p.packetType;
            this.GdataVectors = p.GdataVectors;
            this.GdataFloats = p.GdataFloats;
            this.GdataStrings = p.GdataStrings;
        }

        public byte[] ToBytes()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();

            bf.Serialize(ms, this);
            byte[] bytes = ms.ToArray();
            ms.Close();
            return bytes;
        }

        public static string GetIP4Address()
        {
            IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (IPAddress i in ips)
            {
                if (i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return i.ToString();
            }

            return "172.31.16.131";
        }
    }

    public enum PacketType
    {
        REGISTRATION,
        DEBUG,
        LOBBYJOIN,
        YES,
        NO,
        GAMESTART,
        SPAWN,
        DEATH,
        MOVEMENT,
        AIM,
        SHOOT,
    }

    [Serializable]
    public class Vector_3
    {
        public Vector_3()
        {
            x = 0;
            y = 0;
            z = 0;
        }

        public Vector_3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float x;
        public float y;
        public float z;
    }
}

