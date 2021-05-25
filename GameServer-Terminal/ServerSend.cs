using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class ServerSend
    {
        #region TCP
        //prepares the packet to send for  particular client
        private static void SendTCPdata(int clientid,Packet packet)
        {
            packet.WriteLength(); //welcome packet (int packet (4+4+4+25=37) size which is 4 byte) 
            //Console.WriteLine($"Packet length{packet.Length()}"); //Total packet size =41 byte
            Server.clients[clientid].tcp.SendData(packet);
        }

        //prepares the packet to send for  all client
        private static void SendTCPdatatoall(Packet packet)
        {
            packet.WriteLength();
            for(int i=1;i<=Server.MAX_player;i++)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }

        //prepares the packet to send for  all client
        private static void SendTCPdataexcept(int clientid,Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MAX_player; i++)
            {
                if(clientid!=i)
                {
                    Server.clients[i].tcp.SendData(packet); ;
                }
            }
        }
        #endregion

        #region UDP
        //prepares the packet to send for  particular client
        private static void SendUDPData(int _clientid,Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_clientid].udp.SendData(_packet);
        }

        //prepares the packet to send for  all client
        private static void SendUDPdatatoall(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MAX_player; i++)
            {
                Server.clients[i].udp.SendData(packet);
            }
        }

        //prepares the packet to send for  all client
        private static void SendUDPdataexcept(int clientid, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MAX_player; i++)
            {
                if (clientid != i)
                {
                    Server.clients[i].udp.SendData(packet); ;
                }
            }
        }
        #endregion

        public static void welcome(int clientid,string msg)  //welcome packet
        {
            //constuctor(int) packet called(int welcome 4 byte)
            using (Packet packet = new Packet((int)ServerPackets.welcome)) 
            {
                packet.Write(clientid); //(int clientid 4 byte)
                packet.Write(msg);      //(int msg length 4 byte and msg" Welcome to Game Server!!" 25 byte) 29 byte

                SendTCPdata(clientid, packet);
            };
        }

        /*public static void UDPtest(int clientid,string msg)
        {
            using (Packet packet = new Packet((int)ServerPackets.udptest))
            {
                packet.Write(msg);
                SendUDPData(clientid,packet);
            };
        }*/

        public static void SpawnPlayer(int _id,Player _player)
        {
            using(Packet packet=new Packet((int)ServerPackets.spawnplayer))
            {
                packet.Write(_player.id);      //_player.id which player info(id) is to be send
                packet.Write(_player.username);
                packet.Write(_player.position);
                packet.Write(_player.rotation);

                SendTCPdata(_id, packet); //_id says to which client packet to be send
            };
        }

        public static void PlayerPosition(Player _player)
        {
            using (Packet packet = new Packet((int)ServerPackets.playerposition))
            {
                packet.Write(_player.id);
                packet.Write(_player.position);

                SendUDPdatatoall(packet);
            };
        }

        public static void PlayerRotation(Player _player)
        {
            using (Packet packet = new Packet((int)ServerPackets.playerrotation))
            {
                packet.Write(_player.id);
                packet.Write(_player.rotation);

                SendUDPdataexcept(_player.id, packet);
            };
        }
    }
}
