using System;
using Unity.VisualScripting;
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
    public static event Action MoveDownPressedEvent;
    public static event Action MoveUpPressedEvent;
    
    public bool canMove;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        canMove = true;
        Instance = this;
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
    }
    private void OnEnable()
    {
        GameManager.ChangeScene += ChangeScene;
        GameManager.OnPlay += OnPlay;
        GameManager.OnReading += OnUI;
        GameManager.OnMainMenu += OnUI;
        GameManager.OnPause += OnUI;
        GameManager.OnCinematic += OnCinematic;
    }

    private void OnDisable()
    {
        GameManager.ChangeScene -= ChangeScene;
        GameManager.OnPlay -= OnPlay;
        GameManager.OnReading -= OnUI;
        GameManager.OnMainMenu -= OnUI;
        GameManager.OnPause -= OnUI;
        GameManager.OnCinematic -= OnCinematic;
    }
    private void Update()
    {
        if (canMove) 
        {
            MoveUp = inputActions.Player.MoveUp.IsPressed();
            MoveDown = inputActions.Player.MoveDown.IsPressed();
            MoveLeft = inputActions.Player.MoveLeft.IsPressed();
            MoveRight = inputActions.Player.MoveRight.IsPressed();
        }

        if (inputActions.Player.Pause.WasPressedThisFrame() || inputActions.UI.Pause.WasPressedThisFrame())
            GameManager.instance.Pause();

        if (inputActions.Player.Interact.WasPressedThisFrame())
            InteractPressedEvent?.Invoke();

        if (inputActions.UI.Select.WasPressedThisFrame())
            SelectPressedEvent?.Invoke();

        if (inputActions.UI.MoveDown.WasPressedThisFrame())
            MoveDownPressedEvent?.Invoke();

        if (inputActions.UI.MoveUp.WasPressedThisFrame())
            MoveUpPressedEvent?.Invoke();
    }

    void ChangeScene()
    {
        inputActions.Player.Disable();
        inputActions.UI.Disable();
    }
    void OnPlay()
    {
        inputActions.Player.Enable();
        inputActions.UI.Disable();
    }
    void OnUI()
    {
        inputActions.Player.Disable();
        inputActions.UI.Enable();
    }

    private void OnCinematic()
    {
        inputActions.Player.Disable();
        inputActions.UI.Disable();
    }
}
