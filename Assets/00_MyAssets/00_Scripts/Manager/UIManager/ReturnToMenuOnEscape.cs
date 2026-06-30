using UnityEngine;

/// <summary>
/// Pensado para la escena final (05_Fin). Al pulsar cualquier tecla común
/// (Escape, Intro, Intro del teclado numérico o "E"):
///   1. Resetea WorldState para que la próxima partida empiece limpia.
///   2. Lanza un fade out y carga la escena del Main Menu.
///
/// Las tres teclas las leemos directamente con Input.GetKeyDown, en vez de
/// usar los eventos del InputManager, porque cada una vive en un mapa de
/// inputs distinto (Esc en UI.Pause, Intro en UI.Select, E en
/// Player.Interact) y dependiendo del estado del juego no todos están
/// enabled a la vez. Leer el teclado legacy nos quita esa complicación.
///
/// Cómo usarlo:
///   - Añade este componente a cualquier GameObject de la escena 05_Fin
///     (basta con que esté activo al cargar la escena).
///   - El texto "Pulsa Escape/Enter/E para volver al menú" lo pones tú
///     en un TextMeshProUGUI aparte; este script no toca ningún texto.
/// </summary>
public class ReturnToMenuOnEscape : MonoBehaviour
{
    [Header("Comportamiento del cambio de escena")]
    [Tooltip("BuildIndex del Main Menu. Tras eliminar 00_Introduccion: 0.")]
    public int mainMenuBuildIndex = 0;

    [Tooltip("Tipo de fade que se pasa al ChangeSceneManager para la transición.")]
    public string fadeType = "StandarFade";

    // Evita que pulsar la tecla varias veces durante el fade dispare la
    // transición más de una vez.
    private bool triggered;

    private void OnEnable()
    {
        triggered = false;

        // Forzamos el estado MainMenu en el GameManager por dos motivos:
        //   - Que Escape no dispare GameManager.Pause() (en estado Play sí
        //     lo haría y entraríamos en el menú de pausa por accidente).
        //   - Que los inputs Player de Aster queden deshabilitados durante
        //     la escena de Fin (no esperamos movimiento aquí).
        if (GameManager.instance != null)
            GameManager.instance.ChangeState(DataDefinitions.GameStates.MainMenu);
    }

    private void Update()
    {
        if (triggered) return;

        bool keyPressed =
            Input.GetKeyDown(KeyCode.Escape)      ||
            Input.GetKeyDown(KeyCode.Return)      ||
            Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.E);

        if (!keyPressed) return;

        TriggerReturn();
    }

    private void TriggerReturn()
    {
        if (ChangeSceneManager.instance == null) return;
        if (GameManager.
instance == null) return;

        triggered = true;

        // Limpiamos los flags de la partida para que al volver al menú y
        // empezar otra no se arrastren (DoorUnlocked, PapersFound,
        // FlowersInteracted, Page4Seen, etc.).
        WorldState.ResetAll();

        ChangeSceneManager.instance.nextSceneInsdex = mainMenuBuildIndex;
        ChangeSceneManager.instance.typeOfFade = fadeType;
        GameManager.instance.ChangeState(DataDefinitions.GameStates.ChangeScene);
    }
}
