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
        //public float timeStamp;
        public string senderID;
        public PacketType packetType;
        public List<Vector_3> GdataVectors;
        public List<float> GdataFloats;
        public List<string> GdataStrings;
        public List<int> GdataInts;
        public bool errorEncountered = false;

        char nextValueDelimiter = '*';
        char nextTypeDelimiter = '#';
        char skipMarker = 'x';
        char vectorElementDelimiter = ',';

        public Packet(PacketType type, string senderID)
        {
            //this.timeStamp = 0;
            this.senderID = senderID;
            this.packetType = type;
            GdataVectors = new List<Vector_3>();
            GdataFloats = new List<float>();
            GdataStrings = new List<string>();
            GdataInts = new List<int>();
        }

        public Packet(byte[] packetBytes)
        {
            #region Proto: Creating a packet from string
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(packetBytes);

            string packetString = (string)bf.Deserialize(ms);
            ms.Close();
            
            string[] types = packetString.Split(nextTypeDelimiter);

            #region Finding packetType
            int packetTypeIndex;
            if (int.TryParse(types[0], out packetTypeIndex))
            {
                //Debug.Log("packetTypeIndex parsed successfully");
            }
            else
            {
                Debug.LogWarning("packetTypeIndex parsing failed!");
                errorEncountered = true;
                packetTypeIndex = -1;
            }

            switch (packetTypeIndex)
            {
                case -1:
                    packetType = PacketType.DEBUG;
                    Debug.LogWarning("PacketTyper could not be parsed, so type was set to DEBUG");
                    break;
                case 0:
                    packetType = PacketType.REGISTRATION;
                    break;
                case 1:
                    packetType = PacketType.DEBUG;
                    break;
                case 2:
                    packetType = PacketType.LOBBYEVENT;
                    break;
                case 3:
                    packetType = PacketType.GAMESTART;
                    break;
                case 4:
                    packetType = PacketType.SPAWN;
                    break;
                case 5:
                    packetType = PacketType.PROJECTILE;
                    break;
                case 6:
                    packetType = PacketType.SHIPINFO;
                    break;
                case 7:
                    packetType = PacketType.SHOOT;
                    break;
                case 8:
                    packetType = PacketType.DEATH;
                    break;
                case 9:
                    packetType = PacketType.GAMEEND;
                    break;
            }
            #endregion

            senderID = types[1];

            #region Finding ints
            GdataInts = new List<int>();
            string intsString = types[2];
            if (intsString != skipMarker.ToString())
            {
                string[] ints = intsString.Split(nextValueDelimiter);

                foreach (string str in ints)
                {
                    int value;
                    if (int.TryParse(str, out value))
                    {
                        //Debug.Log("GdataInts value parsed successfully, value: " + value);
                    }
                    else
                    {
                        Debug.LogWarning("GdataInts value parsing failed! str: " + str);
                        errorEncountered = true;
                        value = -1;
                    }

                    if (value != -1)
                    {
                        GdataInts.Add(value);
                    }
                }
            }
            #endregion

            #region Finding floats
            GdataFloats = new List<float>();
            string floatsString = types[3];
            if (floatsString != skipMarker.ToString())
            {
                string[] floats = floatsString.Split(nextValueDelimiter);

                foreach (string str in floats)
                {
                    float value;
                    if (float.TryParse(str, out value))
                    {
                        //Debug.Log("GdataFloats value parsed successfully, value: " + value);
                    }
                    else
                    {
                        Debug.LogWarning("GdataFloats value parsing failed! str: " + str);
                        errorEncountered = true;
                        value = -1;
                    }

                    if (value != -1)
                    {
                        GdataFloats.Add(value);
                    }
                }
            }
            #endregion

            #region Finding strings
            GdataStrings = new List<string>();
            string stringsString = types[4];
            if (stringsString != skipMarker.ToString())
            {
                string[] strings = stringsString.Split(nextValueDelimiter);

                foreach (string str in strings)
                {
                    //Debug.Log("GdataStrings value parsed successfully, str: " + str);
                    GdataStrings.Add(str);
                }
            }
            #endregion

            #region Finding vectors
            GdataVectors = new List<Vector_3>();
            string vectorsString = types[5];
            if (vectorsString != skipMarker.ToString())
            {
                string[] vectors = vectorsString.Split(nextValueDelimiter);

                foreach (string str in vectors)
                {
                    string[] singleVectorElements = str.Split(vectorElementDelimiter);

                    if (singleVectorElements.Length == 3)
                    {
                        //Debug.Log("singleVectorElements.Length == 3");

                        float xValue;
                        float yValue;
                        float zValue;

                        if (float.TryParse(singleVectorElements[0], out xValue))
                        {
                            //Debug.Log("singleVectorElements[0] parsed successfully");
                        }
                        else
                        {
                            Debug.LogWarning("singleVectorElements[0] parsing failed, invalid vector element");
                            errorEncountered = true;
                        }

                        if (float.TryParse(singleVectorElements[1], out yValue))
                        {
                            //Debug.Log("singleVectorElements[1] parsed successfully");
                        }
                        else
                        {
                            Debug.LogWarning("singleVectorElements[1] parsing failed, invalid vector element");
                            errorEncountered = true;
                        }

                        if (float.TryParse(singleVectorElements[2], out zValue))
                        {
                            //Debug.Log("singleVectorElements[2] parsed successfully");
                        }
                        else
                        {
                            Debug.LogWarning("singleVectorElements[2] parsing failed, invalid vector element");
                            errorEncountered = true;
                        }

                        GdataVectors.Add(new Vector_3(xValue, yValue, zValue));
                    }
                    else
                    {
                        Debug.LogWarning("singleVectorElements.Length != 3, invalid vector");
                        for (int i = 0; i < singleVectorElements.Length; i++)
                        {
                            Debug.LogWarning("Invalid vector: singleVectorElement[" + i + "]: " + singleVectorElements[i] + ", vectorsString: " + vectorsString);
                        }
                        errorEncountered = true;
                    }
                }
            }
            #endregion
            #endregion

            #region Original
            //BinaryFormatter bf = new BinaryFormatter();
            //MemoryStream ms = new MemoryStream(packetBytes);

            //Packet p = (Packet)bf.Deserialize(ms);
            //ms.Close();
            //this.timeStamp = p.timeStamp;
            //this.senderID = p.senderID;
            //this.packetType = p.packetType;
            //this.GdataVectors = p.GdataVectors;
            //this.GdataFloats = p.GdataFloats;
            //this.GdataStrings = p.GdataStrings;
            //this.GdataInts = p.GdataInts;
            #endregion
        }

        public byte[] ToBytes()
        {
            #region Proto: Turning the packet to string
            //Debug.Log("Sending packetType: " + packetType);
            int packetTypeIndex = -1;
            switch (packetType)
            {
                case PacketType.REGISTRATION:
                    packetTypeIndex = 0;
                    break;
                case PacketType.DEBUG:
                    packetTypeIndex = 1;
                    break;
                case PacketType.LOBBYEVENT:
                    packetTypeIndex = 2;
                    break;
                case PacketType.GAMESTART:
                    packetTypeIndex = 3;
                    break;
                case PacketType.SPAWN:
                    packetTypeIndex = 4;
                    break;
                case PacketType.PROJECTILE:
                    packetTypeIndex = 5;
                    break;
                case PacketType.SHIPINFO:
                    packetTypeIndex = 6;
                    break;
                case PacketType.SHOOT:
                    packetTypeIndex = 7;
                    break;
                case PacketType.DEATH:
                    packetTypeIndex = 8;
                    break;
                case PacketType.GAMEEND:
                    packetTypeIndex = 9;
                    break;
            }

            string packetString = packetTypeIndex.ToString() + nextTypeDelimiter;

            packetString += senderID + nextTypeDelimiter.ToString();

            if (GdataInts.Count > 0)
            {
                bool first = true;
                foreach (int integer in GdataInts)
                {
                    if (first)
                    {
                        first = false;
                        packetString += integer;
                    }
                    else
                    {
                        packetString += nextValueDelimiter.ToString() + integer;
                    }
                }

                packetString += nextTypeDelimiter.ToString();
            }
            else
            {
                packetString += skipMarker.ToString() + nextTypeDelimiter.ToString();
            }

            if (GdataFloats.Count > 0)
            {
                bool first = true;
                foreach (float floatingPoint in GdataFloats)
                {
                    if (first)
                    {
                        first = false;
                        packetString += floatingPoint;
                    }
                    else
                    {
                        packetString += nextValueDelimiter.ToString() + floatingPoint;
                    }
                }

                packetString += nextTypeDelimiter.ToString();
            }
            else
            {
                packetString += skipMarker.ToString() + nextTypeDelimiter.ToString();
            }

            if (GdataStrings.Count > 0)
            {
                bool first = true;
                foreach (string str in GdataStrings)
                {
                    if (first)
                    {
                        first = false;
                        packetString += str;
                    }
                    else
                    {
                        packetString += nextValueDelimiter.ToString() + str;
                    }
                }

                packetString += nextTypeDelimiter.ToString();
            }
            else
            {
                packetString += skipMarker.ToString() + nextTypeDelimiter.ToString();
            }

            if (GdataVectors.Count > 0)
            {
                bool first = true;
                foreach (Vector_3 vec_3 in GdataVectors)
                {
                    if (first)
                    {
                        first = false;
                        packetString += vec_3.x + vectorElementDelimiter.ToString() + vec_3.y + vectorElementDelimiter.ToString() + vec_3.z;
                    }
                    else
                    {
                        packetString += nextValueDelimiter.ToString() + vec_3.x + vectorElementDelimiter.ToString() + vec_3.y + vectorElementDelimiter.ToString() + vec_3.z;
                    }
                }

                packetString += nextTypeDelimiter.ToString();
            }
            else
            {
                packetString += skipMarker.ToString() + nextTypeDelimiter.ToString();
            }

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();

            bf.Serialize(ms, packetString);
            byte[] bytes = ms.ToArray();
            ms.Close();
            return bytes;
            #endregion

            #region Original
            //BinaryFormatter bf = new BinaryFormatter();
            //MemoryStream ms = new MemoryStream();

            //bf.Serialize(ms, this);
            //byte[] bytes = ms.ToArray();
            //ms.Close();
            //return bytes;
            #endregion
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
        LOBBYEVENT,
        GAMESTART,
        SPAWN,
        PROJECTILE,
        SHIPINFO,
        SHOOT,
        DEATH,
        GAMEEND,
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

        public Vector_3(Vector3 vec3)
        {
            x = vec3.x;
            y = vec3.y;
            z = vec3.z;
        }

        public Vector_3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3 ToVector3()
        {
            Vector3 r;
            r.x = x;
            r.y = y;
            r.z = z;
            return r;
        }

        public float x;
        public float y;
        public float z;
    }
}

