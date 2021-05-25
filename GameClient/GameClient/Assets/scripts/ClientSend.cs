using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.Senddata(_packet);
    }

    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.senddata(_packet);
    }

    public static void WelcomeReceived()
    {
        using(Packet _packet=new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(Client.instance.myid);
            _packet.Write(UIManager.instance.username.text);
            SendTCPData(_packet);
        };
    }

    /*public static void UDPtestreceive(string msg)
    {
        using (Packet packet = new Packet((int)ClientPackets.udptestreceive))
        {
            packet.Write(msg);
            SendUDPData(packet);
        };
    }*/

    public static void SendPlayerMovement(bool[] _inputs)
    {
        using(Packet packet = new Packet((int)ClientPackets.playermovement))
        {
            packet.Write(_inputs.Length);
            foreach(bool input in _inputs)
            {
                packet.Write(input);
            }
            packet.Write(GameManager.players[Client.instance.myid].transform.rotation);
            SendUDPData(packet);
        };
    }
}
