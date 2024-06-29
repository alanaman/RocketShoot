using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    PlayerInputActions playerInputActions;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    public Vector2 getDirectionVector()
    {
        //Debug.Log(playerInputActions.Player.direction.ReadValue<Vector2>());
        return playerInputActions.Player.direction.ReadValue<Vector2>();
    }

    public float getAccelaration()
    {

        return playerInputActions.Player.accelerate.ReadValue<float>();
    }
}
