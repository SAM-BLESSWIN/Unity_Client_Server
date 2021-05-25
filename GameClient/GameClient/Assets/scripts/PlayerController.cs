using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //send the INPUT to server,server will calculate the position and send it to all client
    //we dont want client to directly send position to server because hacker easily modify it
    //making server calculate position with input makes server authoitative and incharge

    private void FixedUpdate()
    {
        SendInputToServer();
    }

    private void SendInputToServer()
    {
        bool[] inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.D)
        };

        ClientSend.SendPlayerMovement(inputs);
    }
}
