using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput I { get; private set; }
    PlayerInputActions playerInputActions;

    public event Action OnAbility1Used = delegate { };

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(this);
            return;
        }

        I = this;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        playerInputActions.Player.X.started += ctx => ActivateAbilityPieMenu();
        playerInputActions.Player.X.canceled += ctx => DeactivateAbilityPieMenu();

        playerInputActions.Player.Y.performed += ctx => OnAbility1Used.Invoke();

    }
    public Vector2 GetDirectionVector()
    {
        //Debug.Log(playerInputActions.Player.direction.ReadValue<Vector2>());
        return playerInputActions.Player.direction.ReadValue<Vector2>();
    }

    public float GetAccelaration()
    {
        return playerInputActions.Player.accelerate.ReadValue<float>();
    }

    void ActivateAbilityPieMenu()
    {
        Debug.Log("ActivateAbilityPieMenu");
        playerInputActions.PieMenu.Enable();
        GameManager.I.ActivatePieMenu();
        playerInputActions.Player.direction.Disable();
    }

    void DeactivateAbilityPieMenu()
    {
        Debug.Log("DeactivateAbilityPieMenu");
        playerInputActions.Player.direction.Enable();
        GameManager.I.DeactivatePieMenu();
        playerInputActions.PieMenu.Disable();
    }

    public Vector2 GetPieDirectionVector()
    {
        return playerInputActions.PieMenu.select.ReadValue<Vector2>();
    }
}
