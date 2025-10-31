using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static DataDefinitions;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerInputActions inputActions;

    public static bool MoveUp { get; private set; }
    public static bool MoveDown { get; private set; }
    public static bool MoveLeft { get; private set; }
    public static bool MoveRight { get; private set; }

    public static event Action InteractPressedEvent;
    public static event Action SelectPressedEvent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
    }
    private void OnEnable()
    {
        GameManager.ChangeScene += ChangeScene;
        GameManager.OnPlay += OnPlay;
        GameManager.OnReading += OnRead;
    }

    private void OnDisable()
    {
        GameManager.ChangeScene -= ChangeScene;
        GameManager.OnPlay -= OnPlay;
        GameManager.OnReading -= OnRead;
    }
    private void Update()
    {
        MoveUp = inputActions.Player.MoveUp.IsPressed();
        MoveDown = inputActions.Player.MoveDown.IsPressed();
        MoveLeft = inputActions.Player.MoveLeft.IsPressed();
        MoveRight = inputActions.Player.MoveRight.IsPressed();

        if (inputActions.Player.Pause.WasPressedThisFrame())
            GameManager.instance.ChangeState(GameStates.Pause);

        if (inputActions.Player.Interact.WasPressedThisFrame())
            InteractPressedEvent?.Invoke();

        if (inputActions.UI.Select.WasPressedThisFrame())
            SelectPressedEvent?.Invoke();
    }

    void ChangeScene()
    {
        inputActions.Player.Disable();
    }
    void OnPlay()
    {
        inputActions.Player.Enable();
        inputActions.UI.Disable();
    }
    void OnRead()
    {
        inputActions.Player.Disable();
        inputActions.UI.Enable();
    }
}
