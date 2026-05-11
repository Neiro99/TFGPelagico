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

    // Cuando esto es true, el Update() de aquí deja de sobreescribir los bools
    // de movimiento; los gestiona quien haya llamado a SetScriptedMovement (por
    // ejemplo PlayerMove.WalkTo durante una cinemática). Así IdleLogic y demás
    // siguen funcionando exactamente igual y la animación de caminar se reproduce.
    private static bool scriptedMove;

    public static event Action InteractPressedEvent;
    public static event Action SelectPressedEvent;
    public static event Action MoveDownPressedEvent;
    public static event Action MoveUpPressedEvent;
    public static event Action MoveLeftPressedEvent;
    public static event Action MoveRightPressedEvent;
    
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
        GameManager.OnDiary += OnUI;
        GameManager.OnCinematic += OnCinematic;
    }

    private void OnDisable()
    {
        GameManager.ChangeScene -= ChangeScene;
        GameManager.OnPlay -= OnPlay;
        GameManager.OnReading -= OnUI;
        GameManager.OnMainMenu -= OnUI;
        GameManager.OnPause -= OnUI;
        GameManager.OnDiary -= OnUI;
        GameManager.OnCinematic -= OnCinematic;
    }
    private void Update()
    {
        if (canMove && !scriptedMove)
        {
            MoveUp = inputActions.Player.MoveUp.IsPressed();
            MoveDown = inputActions.Player.MoveDown.IsPressed();
            MoveLeft = inputActions.Player.MoveLeft.IsPressed();
            MoveRight = inputActions.Player.MoveRight.IsPressed();
        }

        if (inputActions.Player.Pause.WasPressedThisFrame() || inputActions.UI.Pause.WasPressedThisFrame())
            GameManager.instance.Pause();

        if (Input.GetKeyDown(KeyCode.Q))
            GameManager.instance.ToggleDiary();

        if (inputActions.Player.Interact.WasPressedThisFrame())
            InteractPressedEvent?.Invoke();

        if (inputActions.UI.Select.WasPressedThisFrame())
            SelectPressedEvent?.Invoke();

        if (inputActions.UI.MoveDown.WasPressedThisFrame())
            MoveDownPressedEvent?.Invoke();

        if (inputActions.UI.MoveUp.WasPressedThisFrame())
            MoveUpPressedEvent?.Invoke();

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            MoveLeftPressedEvent?.Invoke();

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            MoveRightPressedEvent?.Invoke();
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

    public void AutopressE()
    {
        InteractPressedEvent?.Invoke();
    }

    // -----------------------------------------------------------------------
    // Hooks para movimiento scriptado (cinemáticas, tutoriales, etc.)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Fuerza el estado de los bools de movimiento de InputManager. Mientras
    /// esté activo este modo, el Update() no los sobreescribe con WASD real.
    /// Sirve para que sistemas como IdleLogic, que leen estos bools para
    /// pintar la animación, "vean" un movimiento sintético.
    /// </summary>
    public static void SetScriptedMovement(bool up, bool down, bool left, bool right)
    {
        scriptedMove = true;
        MoveUp = up;
        MoveDown = down;
        MoveLeft = left;
        MoveRight = right;
    }

    /// <summary>
    /// Cierra el modo scriptado y vuelve a leer WASD en el siguiente Update().
    /// </summary>
    public static void ClearScriptedMovement()
    {
        scriptedMove = false;
        MoveUp = false;
        MoveDown = false;
        MoveLeft = false;
        MoveRight = false;
    }
}
