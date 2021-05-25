using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace GameServer
{
    class Player
    {
        public int id;
        public string username;
        public Vector3 position;
        public Quaternion rotation;

        private float movespeed = 5f / Constants.TICKS_PER_SEC;
        private bool[] inputs;

        public Player(int _id, string _username, Vector3 _spawnposition)
        {
            id = _id;
            username = _username;
            position = _spawnposition;
            rotation = Quaternion.Identity;

            inputs = new bool[4];
        }

        public void Update()
        {
            Vector2 inputdirection = Vector2.Zero;      //no jump movemnt so no z axis

            if(inputs[0])
            {
                inputdirection.Y += 1;
            }
            if (inputs[1])
            {
                inputdirection.X -= 1;
            }
            if (inputs[2])
            {
                inputdirection.Y -= 1;
            }
            if (inputs[3])
            {
                inputdirection.X += 1;
            }

            Move(inputdirection);
        }

        private void Move(Vector2 _inputdirection)
        {
            Vector3 forward = Vector3.Transform(new Vector3(0, 0, 1),rotation); //direction player faces

            //vector which point perpendicular to forward vector
            Vector3 right = Vector3.Normalize(Vector3.Cross(new Vector3(0, 1, 0), forward));

            Vector3 movedirection = (right*_inputdirection.X) + (forward * _inputdirection.Y);

            position += movedirection * movespeed;

            //we are sending player position and rotation as seperate packet
            /*we are calculating player position in server based on client input
              and position should  also be send to all the other players connected to server*/
            /*we are not calculating any player rotation in server,client is just sending its rotation value to server
              becaz rotation should also be send to all the other players connected to server except the localplayer itself
              rotaion has full authority by the client itself so no need to send the rotation value to himself*/

            ServerSend.PlayerPosition(this);
            ServerSend.PlayerRotation(this);
        }

        public  void SetInput(bool[] _inputs,Quaternion _rotation)
        {
            inputs = _inputs;
            rotation = _rotation;
        }
    }
}
