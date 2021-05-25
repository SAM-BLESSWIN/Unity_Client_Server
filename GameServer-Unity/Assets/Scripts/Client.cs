using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class Client
{
    public static int databuffersize = 4096;
    public int Id; //store client id
    public TCP tcp;
    public UDP udp;
    public Player player;

    public Client(int clientid)
    {
        Id = clientid;
        tcp = new TCP(Id);
        udp = new UDP(Id);
    }

    #region TCP
    public class TCP
    {
        //Provides client connections for TCP network services.
        public TcpClient Socket = null;
        private Packet receivedata;

        private readonly int Id;

        //The NetworkStream class provides methods for sending and receiving data over Stream sockets 
        //To create a NetworkStream, you must provide a connected Socket
        private NetworkStream stream;
        private byte[] receivebuffer;

        public TCP(int id)
        {
            Id = id;
        }

        public void Connect(TcpClient socket)
        {
            Socket = socket;
            socket.ReceiveBufferSize = databuffersize;
            socket.SendBufferSize = databuffersize;

            stream = socket.GetStream(); //Returns the NetworkStream used to send and receive data.

            receivedata = new Packet();

            receivebuffer = new byte[databuffersize];

            //to read data asynchronously from the network stream.
            stream.BeginRead(receivebuffer, 0, databuffersize, Receivecallback, null);
            //Debug.Log(Id);
            ServerSend.welcome(Id, " Welcome to Game Server!!");
        }

        public void SendData(Packet packet)
        {
            try
            {
                if (Socket != null)
                {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"Error Sending packet to client {Id} via TCP:{ex}");
            }
        }

        private void Receivecallback(IAsyncResult result)
        {
            //Debug.Log("Receivecallback: " + result);
            try
            {
                int bytelength = stream.EndRead(result);
                if (bytelength <= 0)
                {
                    Server.clients[Id].Disconnect();
                    return;
                }
                byte[] data = new byte[bytelength];
                Array.Copy(receivebuffer, data, bytelength); //receivebuffer receives the packet send from client and copy to data array

                receivedata.Reset(Handledata(data));

                stream.BeginRead(receivebuffer, 0, databuffersize, Receivecallback, null);
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
                Server.clients[Id].Disconnect();
            }
        }

        private bool Handledata(byte[] data)
        {
            int packetlength = 0;
            receivedata.SetBytes(data);
            if (receivedata.UnreadLength() >= 4)
            {
                packetlength = receivedata.ReadInt();
                if (packetlength <= 0)
                {
                    return true;
                }
            }
            while ((packetlength > 0 && packetlength <= receivedata.UnreadLength()))
            {
                byte[] packetbytes = receivedata.ReadBytes(packetlength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetbytes))
                    {
                        int packetid = packet.ReadInt();
                        Server.packethandler[packetid](Id, packet); //Recepective dictionary delegate will be called
                    }
                });
                packetlength = 0;
                if (receivedata.UnreadLength() >= 4)
                {
                    packetlength = receivedata.ReadInt();
                    if (packetlength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (packetlength <= 1)
            {
                return true;
            }
            return false;
        }

        public void Disconnect()
        {
            Socket.Close();
            stream = null;
            receivebuffer = null;
            receivedata = null;
            Socket = null;
        }
    }
    #endregion

    #region UDP
    public class UDP
    {
        public IPEndPoint endpoint;
        private int myid;

        public UDP(int id)
        {
            myid = id;
        }

        public void connect(IPEndPoint _endpoint)
        {
            endpoint = _endpoint;
            //Debug.Log($"endpoint:{endpoint}");
            //ServerSend.UDPtest(myid, "Test packet for UDP connecttion");
        }

        public void SendData(Packet _packet)
        {
            Server.SendUDPData(endpoint, _packet);
        }

        public void HandleData(Packet _packet)
        {
            int packetlength = _packet.ReadInt();
            //Debug.Log("packetLength : " + packetlength);
            byte[] data = _packet.ReadBytes(packetlength);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet packet = new Packet(data))
                {
                    int packetid = packet.ReadInt();
                    //Debug.Log("packetid : "+packetid);
                    Server.packethandler[packetid](myid, packet);
                }
            });
        }

        public void Disconnect()
        {
            endpoint = null;
        }
    }
    #endregion

    public void SendToGame(string _playername)
    {
        //create a player for the client
        player = NetworkManager.instance.InstantiatePlayer();
        player.Initialize(Id, _playername);

        foreach (Client client in Server.clients.Values) //getting each client connected with server
        {
            //send all the other player info to to the new player
            //localplayer(mine) will not be spawned ,other player will be spawned on localplayer screen
            if (client.player != null)
            {
                if (client.Id != Id)
                {
                    ServerSend.SpawnPlayer(Id, client.player);  //new player id, all other player detail
                }
            }
        }

        foreach (Client client in Server.clients.Values) //getting each client connected with server
        {
            //send new player info to him and other players
            //local player willl be spawned on all player screen including its own
            if (client.player != null)
            {
                ServerSend.SpawnPlayer(client.Id, player);  //all players id , new player detail
            }
        }
    }

    public void Disconnect()
    {
        Debug.Log($"{tcp.Socket.Client.RemoteEndPoint} has diconnected");
        player = null;
        UnityEngine.Object.Destroy(player.gameObject);
        tcp.Disconnect();
        udp.Disconnect();
    }
}

