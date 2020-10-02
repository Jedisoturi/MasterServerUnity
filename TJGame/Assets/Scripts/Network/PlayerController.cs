using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform _camTransform;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ClientSend.PlayerShoot(_camTransform.forward);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            ClientSend.PlayerThrowItem(_camTransform.forward);
        }
    }

    private void FixedUpdate()
    {
        SendInputToServer();
    }

    private void SendInputToServer()
    {
        var inputs = new bool[]
        {
            Input.GetKey(KeyCode.Comma),
            Input.GetKey(KeyCode.O),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.E),
            Input.GetKey(KeyCode.Space)
        };


        ClientSend.PlayerMovement(inputs);
    }
}
