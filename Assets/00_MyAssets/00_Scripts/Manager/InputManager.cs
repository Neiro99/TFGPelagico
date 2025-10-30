using UnityEngine;

/// <summary>
/// DESCRIPCIÓN:
/// Fecha:
/// Autor:
/// </summary>
public class InputManager : MonoBehaviour
{
    //*************************************************************************************************************
    #region 1 Definicion de variables
    public static Vector2 Movement { get; private set; }
    public static bool JumpPressed { get; private set; }
    public static bool AttackPressed { get; private set; }
    public static bool PausePressed { get; private set; }
    public static bool InteractueObjects { get; private set; }

    #endregion 1
    //*************************************************************************************************************
    #region 2 Funciones de Unity

    void Update()
    {
        // Movimiento en plano XZ
        Movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Entrada de acciones
        JumpPressed = Input.GetKeyDown(KeyCode.Space);
        AttackPressed = Input.GetMouseButtonDown(0);
        PausePressed = Input.GetKeyDown(KeyCode.Escape);
        InteractueObjects = Input.GetKey(KeyCode.E);




    }
    #endregion 2
    //*************************************************************************************************************
    #region 3 Mis funciones

    #endregion 3
    //*************************************************************************************************************
}
