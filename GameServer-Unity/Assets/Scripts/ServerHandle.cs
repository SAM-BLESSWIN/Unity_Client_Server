using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    public static void ReceiveData(int _clientid, Packet _packet)
    {
        int clientid = _packet.ReadInt();
        string username = _packet.ReadString();

        Debug.Log($"{Server.clients[clientid].tcp.Socket.Client.RemoteEndPoint} connected successfully and is now player {clientid} and name {username}.");

        if (clientid != _clientid)
        {
            Debug.Log($"Player \"{username}\" (ID: {_clientid}) has assumed the wrong client ID ({clientid})!");
        }
        Server.clients[_clientid].SendToGame(username);
    }

    /*public static void UDPtestreceive(int _clientid,Packet _packet)
    {
        string msg = _packet.ReadString();
        Debug.Log($"RECEIVED PACKAGE VIA UDP FROM CLIENT : {msg}");
    }*/

    public static void ReceivePlayerMovement(int _id, Packet _packet)
    {
        bool[] inputs = new bool[_packet.ReadInt()];

        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = _packet.ReadBool();
        }

        Quaternion rotation = _packet.ReadQuaternion();

        Server.clients[_id].player.SetInput(inputs, rotation);
    }
}
