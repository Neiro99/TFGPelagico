using System.Collections;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Cinemática que se reproduce a pantalla completa después del diálogo
/// posterior al puzzle (la conversación entre Syn y Aster). Cuando termina,
/// dispara el cambio de escena hacia la escena destino (por defecto la nueva
/// escena 4) con el fade configurado.
///
/// Soporta tres formas de detectar el final de la cinemática:
///   1. Si hay un VideoPlayer asignado: se suscribe a su loopPointReached.
///   2. Si hay un Animator asignado: añade en tu animación un AnimationEvent
///      al final que llame al método público <see cref="OnCinematicEnd"/>.
///   3. Si no hay ninguno (o como red de seguridad): se usa <see cref="maxDuration"/>.
/// </summary>
public class PostPuzzleCinematic : MonoBehaviour
{
    public static PostPuzzleCinematic Instance;

    [Header("GameObject raíz de la cinemática")]
    [Tooltip("GameObject que contiene la UI / VideoPlayer / Animator de la cinemática. " +
             "Se mantiene desactivado hasta que se llame PlayCinematic, y se desactiva al terminar.")]
    public GameObject cinematicRoot;

    [Header("Reproducción por VideoPlayer (opcional)")]
    [Tooltip("Si la cinemática es un vídeo, asígnalo aquí. Si lo dejas vacío se ignora.")]
    public VideoPlayer videoPlayer;

    [Header("Reproducción por Animator (opcional)")]
    [Tooltip("Si la cinemática es una animación, asígnalo aquí. Para detectar el final " +
             "añade un AnimationEvent al último frame de la animación que llame al " +
             "método OnCinematicEnd() de este script.")]
    public Animator cinematicAnimator;
    [Tooltip("Nombre del bool del Animator que arranca la animación. Déjalo vacío si " +
             "tu Animator se reproduce sólo al activarse el GameObject.")]
    public string playBoolName = "";

    [Header("Tiempos")]
    [Tooltip("Duración máxima de seguridad. Si después de este tiempo la cinemática no " +
             "ha avisado de su final, se fuerza el cambio de escena igualmente.")]
    public float maxDuration = 30f;

    [Header("Cambio de escena al terminar")]
    [Tooltip("Índice de la escena destino (Build Settings).")]
    public int nextSceneIndex = 4;
    [Tooltip("Tipo de fade del ChangeSceneManager. Por defecto el mismo que se usa al " +
             "pasar de la escena 02 a la 03 (\"SwichFade\").")]
    public string sceneTransitionFade = "SwichFade";

    private bool finished;
    private Coroutine watchdogRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Aseguramos que la cinemática NO se reproduce nada más cargar la escena.
        if (cinematicRoot != null) cinematicRoot.SetActive(false);
    }

    /// <summary>
    /// Punto de entrada llamado por DialogueUIManager.EndDialogue al terminar
    /// el diálogo posterior al puzzle (acción "EndPuzzleSequence").
    /// </summary>
    public void PlayCinematic()
    {
        finished = false;

        // Pasamos a estado Cinemática para que el InputManager bloquee al jugador
        // y se desactiven sus mapas de input.
        GameManager.instance.ChangeState(DataDefinitions.GameStates.Cinematic);

        // Activamos el GameObject de la cinemática (Canvas + vídeo / animación).
        if (cinematicRoot != null && !cinematicRoot.activeSelf)
            cinematicRoot.SetActive(true);

        // Suscripción al final del vídeo (si lo hay).
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= HandleVideoEnd;
            videoPlayer.loopPointReached += HandleVideoEnd;
            videoPlayer.Play();
        }

        // Lanzamos animación (si la hay).
        if (cinematicAnimator != null && !string.IsNullOrEmpty(playBoolName))
            cinematicAnimator.SetBool(playBoolName, true);

        // Watchdog: si tras maxDuration nadie ha llamado a OnCinematicEnd,
        // forzamos el cambio de escena para no dejar la partida colgada.
        if (watchdogRoutine != null) StopCoroutine(watchdogRoutine);
        watchdogRoutine = StartCoroutine(Watchdog());
    }

    private IEnumerator Watchdog()
    {
        yield return new WaitForSeconds(maxDuration);
        if (!finished) OnCinematicEnd();
    }

    private void HandleVideoEnd(VideoPlayer source)
    {
        OnCinematicEnd();
    }

    /// <summary>
    /// Llamar a este método cuando la cinemática haya terminado. Se puede
    /// invocar desde un AnimationEvent, desde un script de timeline, o desde
    /// el VideoPlayer (este script lo conecta automáticamente). También se
    /// llama desde el watchdog si pasa demasiado tiempo sin aviso.
    /// </summary>
    public void OnCinematicEnd()
    {
        if (finished) return;
        finished = true;

        if (watchdogRoutine != null)
        {
            StopCoroutine(watchdogRoutine);
            watchdogRoutine = null;
        }

        if (videoPlayer != null)
            videoPlayer.loopPointReached -= HandleVideoEnd;

        if (cinematicAnimator != null && !string.IsNullOrEmpty(playBoolName))
            cinematicAnimator.SetBool(playBoolName, false);

        if (cinematicRoot != null) cinematicRoot.SetActive(false);

        // Marcamos el flag global para desbloquear el contenido posterior
        // (por ejemplo, la 4ª página del diario).
        WorldState.PostPuzzleCinematicSeen = true;

        // Disparamos el cambio de escena con el fade configurado.
        ChangeSceneManager.instance.nextSceneInsdex = nextSceneIndex;
        ChangeSceneManager.instance.typeOfFade = sceneTransitionFade;
        GameManager.instance.ChangeState(DataDefinitions.GameStates.ChangeScene);
    }
}
