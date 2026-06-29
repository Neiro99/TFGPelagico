using System.Collections;
using UnityEngine;

/// <summary>
/// Orquesta la cinemática que se dispara la primera vez que el jugador
/// interactúa con el barco (o intenta interactuar con la puerta cerrada):
///
///   1. Bloquea el control del jugador (estado Cinemática).
///   2. Activa los animators de sprite de Syn y Munin (idle → walk) y, si
///      se ha configurado, el animator de movimiento que mueve los dos
///      GameObjects juntos por la escena.
///   3. Espera <see cref="walkDuration"/> segundos a que lleguen.
///   4. Lanza el diálogo configurado (por defecto "BoatFirstTime") con la
///      acción de fin "UnlockDoor", que desbloqueará la puerta cuando termine.
///
/// Las referencias a los animators son INSTANCIAS PROPIAS del escenario del
/// barco (un Syn y un Munin colocados aparte), distintas del Syn original
/// que tiene su propia animación de "irse" tras hablar con Aster.
/// </summary>
public class BoatCinematicAction : MonoBehaviour
{
    public static BoatCinematicAction Instance;

    public ObjectForeground tableScript;

    [Header("Animator de Syn (sprite: idle ↔ walk)")]
    [Tooltip("Animator del Syn que aparece cerca del barco para esta cinemática.")]
    public Animator synAnimator;
    [Tooltip("Nombre del parámetro bool del Animator de Syn que arranca su animación de caminar.")]
    public string synWalkBoolName = "SynWalk";

    [Header("Animator de Munin (sprite: idle ↔ walk)")]
    [Tooltip("Animator del Munin que aparece cerca del barco para esta cinemática.")]
    public Animator muninAnimator;
    [Tooltip("Nombre del parámetro bool del Animator de Munin que arranca su animación de caminar.")]
    public string muninWalkBoolName = "muninWalk";

    [Header("Animator de movimiento (opcional)")]
    [Tooltip("Animator extra que mueve los dos sprites de sitio (por ejemplo, un Animator " +
             "en un GameObject padre que contiene a Syn y Munin). Déjalo vacío si no lo usas.")]
    public Animator movementAnimator;
    [Tooltip("Si está activado, el componente Animator de movimiento se mantendrá DESACTIVADO " +
             "hasta que arranque la cinemática. Útil cuando el Animator no tiene parámetros " +
             "y su estado por defecto ya es la animación de desplazamiento (se reproduce " +
             "automáticamente al habilitarlo).")]
    public bool enableMovementAnimatorOnStart = true;
    [Tooltip("Nombre del parámetro bool del Animator de movimiento. Solo se usa si el Animator " +
             "tiene un parámetro con ese nombre; si está vacío, no se hace nada con bools.")]
    public string movementWalkBoolName = "";

    [Header("GameObjects de Syn y Munin del barco")]
    [Tooltip("GameObject del Syn del barco. Si está desactivado al inicio, se activará al lanzar la cinemática.")]
    public GameObject synGameObject;
    [Tooltip("GameObject del Munin del barco. Si está desactivado al inicio, se activará al lanzar la cinemática.")]
    public GameObject muninGameObject;

    [Header("Pre-cinemática: caminar a Aster hasta un punto")]
    [Tooltip("Si se asigna, ANTES de empezar la cinemática Aster caminará automáticamente hasta " +
             "este Transform (usando su movimiento normal, como si pulsara WASD). " +
             "Sirve para reposicionarla en cámara antes de que aparezcan Syn y Munin.")]
    public Transform asterMoveTarget;
    [Tooltip("Distancia a la que se considera que Aster ha llegado al destino.")]
    public float asterArrivalDistance = 0.5f;
    [Tooltip("Tiempo máximo de seguridad esperando a Aster (por si se queda atascada).")]
    public float asterMoveTimeout = 5f;
    [Tooltip("Referencia opcional al PlayerMove de Aster. Si se deja vacío, se busca con PlayerMove.Instance.")]
    public PlayerMove playerMove;

    [Header("Tiempos")]
    [Tooltip("Segundos que tardan Syn y Munin en llegar al barco antes de empezar el diálogo.")]
    public float walkDuration = 2.5f;
    [Tooltip("Pequeña pausa extra antes de mostrar el diálogo, una vez ya han llegado.")]
    public float postWalkPause = 0.2f;

    [Header("Diálogo posterior")]
    [Tooltip("Nombre del CSV en Resources/ (sin extensión) con el diálogo de Syn y Munin.")]
    public string cinematicDialogueCSV = "BoatFirstTime";
    [Tooltip("Acción de diálogo que se lanzará al terminar el diálogo. Por defecto desbloquea la puerta.")]
    public string postDialogueAction = "UnlockDoor";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Si el animator de movimiento debe arrancar la cinemática nada más
        // habilitarse, nos aseguramos de que esté deshabilitado al cargar la escena
        // para que la animación no se reproduzca antes de tiempo.
        if (enableMovementAnimatorOnStart && movementAnimator != null)
            movementAnimator.enabled = false;
    }

    /// <summary>
    /// Punto de entrada llamado por DialogueUIManager.EndDialogue cuando la
    /// acción "BoatFirstInteract" se cierra (es decir, justo después del
    /// diálogo inicial de Boat.csv o de Door.csv).
    /// </summary>
    public void StartCinematic()
    {
        StartCoroutine(RunCinematic());
    }

    private IEnumerator RunCinematic()
    {
        // 1. Bloqueamos el control del jugador.
        GameManager.instance.ChangeState(DataDefinitions.GameStates.Cinematic);

        // 1.b Si hay punto definido, movemos a Aster hasta allí antes de seguir.
        if (asterMoveTarget != null)
        {
            PlayerMove pm = playerMove != null ? playerMove : PlayerMove.Instance;
            if (pm != null)
            {
                bool arrived = false;
                pm.WalkTo(asterMoveTarget.position, asterArrivalDistance, () => arrived = true, asterMoveTimeout);

                // Esperamos a que el callback se dispare (llegada o timeout).
                float waited = 0f;
                while (!arrived && waited <= asterMoveTimeout + 0.5f)
                {
                    waited += Time.deltaTime;
                    yield return null;
                }

                // Por si acaso, paramos cualquier walk scriptado que siga vivo.
                pm.StopScriptedWalk();
            }
        }

        // 2. Aseguramos que Syn y Munin del barco estén visibles.
        if (synGameObject != null && !synGameObject.activeSelf) synGameObject.SetActive(true);
        if (muninGameObject != null && !muninGameObject.activeSelf) muninGameObject.SetActive(true);

        // 3. Activamos el caminar: sprite de Syn, sprite de Munin y movimiento de conjunto.
        SetBoolSafe(synAnimator, synWalkBoolName, true);
        SetBoolSafe(muninAnimator, muninWalkBoolName, true);

        tableScript.textToShow1 = "TableTorpere1";
        tableScript.textToShow2 = "TableTorpere1";

        // Reseteamos el firstInteract de la mesa: aunque el jugador ya
        // hubiera interactuado antes (con los textos viejos, sin que se
        // considerara "leer los papeles"), la próxima interacción
        // mostrará "TableTorpere1" y volverá a disparar "FindPapers".
        // El propio FindPapers ya solo activa PapersFound si DoorUnlocked
        // es true, lo cual ocurre al terminar este diálogo.
        if (tableScript != null)
            tableScript.ResetFirstInteract();

        // El animator de movimiento puede activarse por bool y/o habilitando el componente.
        if (movementAnimator != null && enableMovementAnimatorOnStart)
            movementAnimator.enabled = true;
        SetBoolSafe(movementAnimator, movementWalkBoolName, true);

        // 4. Esperamos a que lleguen al barco.
        yield return new WaitForSeconds(walkDuration);

        // 5. Paramos las animaciones de sprite (vuelven a idle).
        // El animator de movimiento se deja como esté: si la animación es one-shot
        // y no hace loop, se quedará en el último frame con Syn y Munin junto al barco.
        SetBoolSafe(synAnimator, synWalkBoolName, false);
        SetBoolSafe(muninAnimator, muninWalkBoolName, false);
        SetBoolSafe(movementAnimator, movementWalkBoolName, false);

        if (postWalkPause > 0f)
            yield return new WaitForSeconds(postWalkPause);

        // 6. Lanzamos el diálogo de Syn y Munin pidiéndole a Aster que abra la puerta.
        GameManager.instance.currentDialogueCSV = cinematicDialogueCSV;
        GameManager.instance.currentDialogueAction = postDialogueAction;

        UIManager.instance.ActivateUI("background", true);
        UIManager.instance.ActivateUI("dialogue", true);
        UIManager.instance.ActivateUI("characters", true);

        GameManager.instance.ChangeState(DataDefinitions.GameStates.Reading);
    }

    /// <summary>
    /// SetBool defensivo: ignora animators o nombres no asignados, evitando
    /// excepciones si alguna referencia queda vacía en el Inspector.
    /// </summary>
    private static void SetBoolSafe(Animator animator, string boolName, bool value)
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(boolName)) return;
        animator.SetBool(boolName, value);
    }
}
