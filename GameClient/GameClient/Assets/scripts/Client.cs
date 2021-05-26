using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using TMPro;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int databuffersize = 4096;

    public string ipaddress;
    public int port = 26950;
    public int myid = 0;
    public TCP tcp;
    public UDP udp;

    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> packethandler;

    private bool isconnected = false;

    public TMP_InputField IPaddress;


    private void Awake()
    {
        if(instance==null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(this);
        }
    }

    public void SetIP()
    {
        ipaddress = IPaddress.text;
    }

    private void Start()
    {
        tcp = new TCP();
        udp = new UDP(); 
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void connecttoServer()
    {
        // Debug.Log("called");
        InitializeclientData();
        tcp.connect();
        isconnected = true;
    }

    #region TCP
    //TCP (Transmission Control Protocol) 
    //TCP is a connection oriented protocol.
    //includes error-checking, guarantees the delivery and preserves the order of the data packets.
    // particular sequence -packets arrive in-order at the receiver.
    //TCP is slower 
    //Retransmission of data packets is possible in TCP in case packet get lost or need to resend.
    public class TCP
    {
        public TcpClient socket;
        public NetworkStream stream;
        public byte[] receivebuffer;

        private Packet receivedata;

        /*similar to
        public TcpClient Socket;
        public void Connect(TcpClient socket)
        {
            Socket = socket;
            socket.ReceiveBufferSize = databuffer;
            socket.SendBufferSize = databuffer;
        }*/

        public void connect()
        {
            socket = new TcpClient                            
            {
                ReceiveBufferSize = databuffersize,
                SendBufferSize = databuffersize
            };
            receivebuffer = new byte[databuffersize];
            
            //Begins an asynchronous request for a remote host connection.
            Debug.Log("connecting server..." + DateTime.Now.ToString());
            socket.BeginConnect(instance.ipaddress, instance.port, connectcallback, null);
        }   
        
        public void Senddata(Packet _packet)
        {
            try
            {
                if(socket!=null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(),null,null);
                }
            }
            catch(Exception ex)
            {
                Debug.Log($"Error while sending data to server {ex}");
            }
        }

        private void connectcallback(IAsyncResult result)
        {
           // Debug.Log("connectcallback: "+result);
            socket.EndConnect(result); //Ends a pending asynchronous connection attempt.
            if (!socket.Connected)  //check its connected
            {
                Debug.Log("Socket Not connected "+ DateTime.Now.ToString());
                return;
            }
            stream = socket.GetStream();

            receivedata = new Packet(); //object to access packet
            Debug.Log("connected to server..." + DateTime.Now.ToString());
            stream.BeginRead(receivebuffer, 0, databuffersize, Receivecallback, null);
        }

        private void Receivecallback(IAsyncResult result)
        {
            //Debug.Log("Receivecallback: "+result);
            try
            {
                int bytelength = stream.EndRead(result);
                //Debug.Log("bytelength: " + bytelength);
                if (bytelength <= 0)
                {
                    instance.Disconnect(); //calls client class disconnect
                    return;
                }
                byte[] data = new byte[bytelength];
                Array.Copy(receivebuffer, data, bytelength); //receivebuffer receives the packet send from server and copy to data array

                /*Our client and server are connected thorugh TCP
                  tcp is stream based - it sends a continous stream of data
                    ensures all packets send are delivered and in correct order
                  Data are gauranted to deliver but not in one piece
                  packet we send is added to larger list of bytes when size is maxed they send as single big delivery
                  Tcp leaves us to handle whther the package is split as sepearte deliveries,
                      so we dont reset receivedata everytime becaues 
                      a piece of packet would remain without handling because the rest of it haven't arrived
                      if we reset everyime we would result in lose of data
                  we reset based on the return value of handledata*/

                receivedata.Reset(Handledata(data)); 
                stream.BeginRead(receivebuffer, 0, databuffersize, Receivecallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Disconnect();  //calls tcp class disconnect
            }
        }

        private bool Handledata(byte[] data)
        {
            int packetlength = 0;
            receivedata.SetBytes(data);
            //Debug.Log("TotalLength:"+receivedata.UnreadLength());
            if(receivedata.UnreadLength()>=4)
            {
                packetlength = receivedata.ReadInt();
                //Debug.Log("PacketLength:"+packetlength);
                if (packetlength<=0)
                {
                    return true;
                }
            }
            while((packetlength>0 && packetlength<=receivedata.UnreadLength()))
            {
                byte[] packetbytes = receivedata.ReadBytes(packetlength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using(Packet packet=new Packet(packetbytes))
                    {
                        int packetid = packet.ReadInt();
                        //Debug.Log(packetid);
                        packethandler[packetid](packet); //Recepective dictionary delegate will be called
                    }
                });
                packetlength = 0;
                //Debug.Log("ur"+receivedata.UnreadLength());
                if (receivedata.UnreadLength() >= 4)
                {
                    packetlength = receivedata.ReadInt();
                    //Debug.Log("r" + packetlength);
                    if (packetlength <= 0)
                    {
                        return true;
                    }
                }
            }

            if(packetlength<=1)
            {
                return true;
            }
            return false;
        }

        private void Disconnect()
        {
            instance.Disconnect();
            stream = null;
            receivebuffer = null;
            receivedata = null;
            socket = null;
        }
    }
    #endregion

    #region UDP
    // UDP (User Datagram Protocol) 
    //UDP is a connection less protocol.
    // the data will be sent continuously, irrespective of the issues in the receiving end.
    //no sequencing of data in UDP - implement ordering it has to be managed by the application 
    //UDP is faster
    //etransmission of packets is not possible in UDP.

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endpoint;

        public UDP()
        {
            endpoint = new IPEndPoint(IPAddress.Parse(instance.ipaddress),instance.port);
        }

        public void connect(int _localport) //this localport is different from server port(26950)
        {
            Debug.Log($"Connected with localport : {_localport}");

            socket = new UdpClient(_localport);
            socket.Connect(endpoint);
            socket.BeginReceive(receivecallback, null);

            /*creating a packet and send immediate to server main purpose to 
              establish connection to server and open the local port so client can receive messages*/

            using (Packet packet = new Packet()) 
            {
                senddata(packet);
            }
        }

        public void senddata(Packet _packet)
        {
            try
            {
                //TCP, server can handle multiple TCP client so whenver a client connects id is allocated sequential
                //UDP, server can handle only one UDP client so client id is sent along with packet to server to know who sent it

                _packet.InsertInt(instance.myid); //insert clientid to packet to send to server

                if (socket != null)
                {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }
            }
            catch(Exception ex)
            {
                Debug.Log($"Error in UDP while sendingdata: {ex}");
            }
        }

        public void receivecallback(IAsyncResult _result)
        {
            try
            {
                byte[] data = socket.EndReceive(_result, ref endpoint);
                socket.BeginReceive(receivecallback, null);

                if(data.Length<4)
                {
                    instance.Disconnect();
                    return;
                }

                Handledata(data);
            }
            catch(Exception ex)
            {
                Disconnect();
                Debug.Log($"Error in UDP receivecallback: {ex}");  
            }
        }

        public void Handledata(byte[] _data)
        {
            using (Packet packet = new Packet(_data))
            {
                int packetlength = packet.ReadInt();
                _data = packet.ReadBytes(packetlength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using(Packet packet=new Packet(_data))
                {
                    int packetid = packet.ReadInt();
                    packethandler[packetid](packet);
                }
            });
        }

        private void Disconnect()
        {
            instance.Disconnect();
            endpoint = null;
            socket = null;
        }
    }
    #endregion

    private void InitializeclientData()
    {
        packethandler = new Dictionary<int, PacketHandler>()
        {
                {(int)ServerPackets.welcome,ClientHandle.Welcome },  //delegate will not be callled while initializing, called while accessing it
                //{(int)ServerPackets.udptest,ClientHandle.UDPtest },
                {(int)ServerPackets.spawnplayer,ClientHandle.SpawnHandle },
                {(int)ServerPackets.playerposition,ClientHandle.Playerposition},
                {(int)ServerPackets.playerrotation,ClientHandle.Playerrotation},
                { (int)ServerPackets.playerdisconnected,ClientHandle.PlayerDisconnects}
        };
        Debug.Log("Data Initialized");
    }

    private void Disconnect()
    {
        if(isconnected)
        {
            isconnected = false;
            tcp.socket.Close();
            udp.socket.Close();
            Debug.Log("Disconnected from Server...");
        }
    }
}
