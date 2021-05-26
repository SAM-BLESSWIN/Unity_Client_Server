using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;


public class Server 
{
    public static int MAX_player { get; private set; } //maxplayer can be set only in this script //readonly in other scripts
    public static int Port { get; private set; }
    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

    public delegate void PacketHandler(int _clientid, Packet _packet);
    public static Dictionary<int, PacketHandler> packethandler;      //packet id and packet

    //Listens for connections from TCP network clients.
    private static TcpListener tcpListener;

    //Providdes UDP network service
    private static UdpClient udpListener;

    public static void Start(int maxplayers, int port)
    {
        MAX_player = maxplayers;
        Port = port;

        Debug.Log("Starting server...");
        Initializeserverdata();

        tcpListener = new TcpListener(IPAddress.Any, Port);  //listens to connection at particular ip and port

        tcpListener.Start();  //start listening for incoming connection request

        // Accept the connection.
        /*An asynchronous client socket does not suspend the application while waiting for network operations to complete.
         The server is built with an asynchronous socket, so execution of the server application
         is not suspended while it waits for a connection from a client*/

        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPconnectioncallback), null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UDPreceivecallback, null);

        Debug.Log("Listening for client " + DateTime.Now.ToString());
        Debug.Log("Server started on " + Port);
    }

    public static void TCPconnectioncallback(IAsyncResult result)
    {
        Debug.Log("connected to a client " + DateTime.Now.ToString());
        // Debug.Log("TCPconnectioncallback: "+result.ToString());

        // End the operation 
        /* The asynchronous BeginAcceptTcpClient operation must be completed by 
            calling the EndAcceptTcpClient method. */
        //Typically, the method is invoked by the callback delegate.
        //Delegates (AsyncCallback) are used to pass methods as arguments

        TcpClient client = tcpListener.EndAcceptTcpClient(result);

        //once connected we must continue to keep listening for connection (looping)
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPconnectioncallback), null);

        Debug.Log("incoming connection from socket" + client.Client.RemoteEndPoint);

        for (int i = 1; i <= MAX_player; i++) //checks the socket is null if null connect with client
        {
            if (clients[i].tcp.Socket == null)  //checks the socket in tcp class in client class in client script
            {
                //calls the connect method in tcp class in client class in client script
                clients[i].tcp.Connect(client);
                //Debug.Log("clientid: "+i+" "+"client value:"+clients[i]);
                return;
            }
        }

        Debug.Log("Server Full");
    }

    public static void UDPreceivecallback(IAsyncResult _result)
    {
        try
        {
            IPEndPoint _endPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpListener.EndReceive(_result, ref _endPoint); //the ref keyword indicates that an argument is passed by reference, not by value. 
            udpListener.BeginReceive(UDPreceivecallback, null);

            if (data.Length < 4)
            {
                return;
            }

            using (Packet packet = new Packet(data))
            {
                int clientid = packet.ReadInt();
                //Debug.Log($"UDP-clientid:{clientid}");

                if (clientid == 0) //not possible but if it happens server crashes on executing next statemnet
                {
                    return;
                }

                if (clients[clientid].udp.endpoint == null) //1st packet send from client, received is just an empty packet, sent to open the port 
                {
                    clients[clientid].udp.connect(_endPoint);
                    return;
                }

                //check endpoint stored for the client matches the endpoint where packet came from
                //becaz theortically hacker can a send a packet altering clientid without their own id
                //string conversion must before comparing
                if (clients[clientid].udp.endpoint.ToString() == _endPoint.ToString())
                {
                    clients[clientid].udp.HandleData(packet);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Error in UDP receivecallback: {ex}");
        }
    }

    public static void SendUDPData(IPEndPoint _clientendpoint, Packet _packet)
    {
        try
        {
            if (_clientendpoint != null)
            {
                udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientendpoint, null, null);
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Error UDP-Sending data : {ex}");
        }
    }

    private static void Initializeserverdata()
    {
        Debug.Log("Server Data Initializing...");
        for (int i = 1; i <= MAX_player; i++)
        {
            //creates a client for all players and add client id and client to dictionary
            //calls the client class constructor in client script
            clients.Add(i, new Client(i));
        }

        packethandler = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.welcomeReceived,ServerHandle.ReceiveData},
               //{(int)ClientPackets.udptestreceive,Serverhandle.UDPtestreceive}
               {(int)ClientPackets.playermovement,ServerHandle.ReceivePlayerMovement},
            };
        Debug.Log("Initialized Packets");
    }

    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }
}
