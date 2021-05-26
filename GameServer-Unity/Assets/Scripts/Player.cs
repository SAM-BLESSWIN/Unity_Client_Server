using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;

    public CharacterController charcontrol;
    public float gravity = -9.81f;
    public float movespeed = 5f;
    public float jumpspeed = 5f;
    public float yvelocity = 0f;

    private bool[] inputs;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        movespeed *= Time.fixedDeltaTime;
        jumpspeed *= Time.fixedDeltaTime;
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;

        inputs = new bool[5];
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

        movedirection *= movespeed;

        if(charcontrol.isGrounded)
        {
            yvelocity = 0f;
            if(inputs[4])
            {
                yvelocity = jumpspeed;
            }
        }
        yvelocity += gravity;
        movedirection.y = yvelocity;
        charcontrol.Move(movedirection);

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
