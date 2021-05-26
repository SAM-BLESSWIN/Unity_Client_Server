using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet packet)
    {
        int clientid = packet.ReadInt();
        //Debug.Log("ClientId:" + clientid);
        string msg = packet.ReadString();
        Debug.Log($"SERVER MESSAGE : {msg}");

        Client.instance.myid = clientid;
        ClientSend.WelcomeReceived();
        Client.instance.udp.connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    /*public static void UDPtest(Packet _packet)
    {
        string msg = _packet.ReadString();
        Debug.Log($"RECEIVED PACKAGE VIA UDP FROM SERVER: {msg}");
        ClientSend.UDPtestreceive("UDP message received");
    }*/

    public static void SpawnHandle(Packet _packet)
    {
        int id = _packet.ReadInt();
        string username = _packet.ReadString();
        Vector3 position = _packet.ReadVector3();
        Quaternion rotation = _packet.ReadQuaternion();

        GameManager.Instance.SpawnPlayer(id, username, position, rotation);
    }

    public static void Playerposition(Packet _packet)
    {
        int id = _packet.ReadInt();
        Vector3 position = _packet.ReadVector3();

        GameManager.players[id].transform.position = position;
    }

    public static void Playerrotation(Packet _packet)
    {
        int id = _packet.ReadInt();
        Quaternion rotation = _packet.ReadQuaternion();

        GameManager.players[id].transform.rotation = rotation;
    }

    public static void PlayerDisconnects(Packet _packet)
    {
        int id = _packet.ReadInt();
        Destroy(GameManager.players[id].gameObject);
        GameManager.players.Remove(id);
    }

}
