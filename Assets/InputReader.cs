using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputReader : MonoBehaviour
{
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public bool AimHeld { get; private set; }
    public bool SprintHeld { get; private set; }

    public event Action InteractPressed;
    public event Action ReloadPressed;

    PlayerInput pi;
    InputAction move, look, aim, sprint, interact, reload;

    void Awake()
    {
        pi = GetComponent<PlayerInput>();
        var a = pi.actions;
        move = a.FindAction("Move", true);
        look = a.FindAction("Look", true);
        aim = a.FindAction("Aim", true);
        sprint = a.FindAction("Sprint", true);
        interact = a.FindAction("Interact", true);
        //reload   = a.FindAction("Reload",   true);
    }

    void OnEnable()
    {
        move.performed += ctx => Move = ctx.ReadValue<Vector2>();
        move.canceled += _ => Move = Vector2.zero;

        look.performed += ctx => Look = ctx.ReadValue<Vector2>();
        look.canceled += _ => Look = Vector2.zero;

        aim.started += _ => AimHeld = true;
        aim.canceled += _ => AimHeld = false;

        sprint.started += _ => SprintHeld = true;
        sprint.canceled += _ => SprintHeld = false;

        interact.performed += _ => InteractPressed?.Invoke();
        // reload.performed   += _ => ReloadPressed?.Invoke();
    }

    void OnDisable()
    {
        move.performed -= ctx => Move = ctx.ReadValue<Vector2>();
        move.canceled -= _ => Move = Vector2.zero;
        look.performed -= ctx => Look = ctx.ReadValue<Vector2>();
        look.canceled -= _ => Look = Vector2.zero;
        aim.started -= _ => AimHeld = true;
        aim.canceled -= _ => AimHeld = false;
        sprint.started -= _ => SprintHeld = true;
        sprint.canceled -= _ => SprintHeld = false;
        interact.performed -= _ => InteractPressed?.Invoke();
        //  reload.performed   -= _ => ReloadPressed?.Invoke();
    }
}