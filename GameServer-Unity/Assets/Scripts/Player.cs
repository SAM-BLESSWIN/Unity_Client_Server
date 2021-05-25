using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;

    private float movespeed = 5f / Constants.TICKS_PER_SEC;
    private bool[] inputs;

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;

        inputs = new bool[4];
    }

    private void FixedUpdate()
    {
        Vector2 inputdirection = Vector2.zero;      //no jump movemnt so no z axis

        if (inputs[0])
        {
            inputdirection.y += 1;
        }
        if (inputs[1])
        {
            inputdirection.x -= 1;
        }
        if (inputs[2])
        {
            inputdirection.y -= 1;
        }
        if (inputs[3])
        {
            inputdirection.x += 1;
        }

        Move(inputdirection);
    }

    private void Move(Vector2 _inputdirection)
    {
        Vector3 movedirection = (transform.right * _inputdirection.x) + (transform.forward * _inputdirection.y);

        transform.position += movedirection * movespeed;

        //we are sending player position and rotation as seperate packet
        /*we are calculating player position in server based on client input
          and position should  also be send to all the other players connected to server*/
        /*we are not calculating any player rotation in server,client is just sending its rotation value to server
          becaz rotation should also be send to all the other players connected to server except the localplayer itself
          rotaion has full authority by the client itself so no need to send the rotation value to himself*/

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }
}
