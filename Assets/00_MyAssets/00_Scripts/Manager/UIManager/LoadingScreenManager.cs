using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Entrada reutilizable de pantalla de carga: una clave que identifica la
/// pantalla, el sprite que se muestra (controles, mapa, lo que sea) y la
/// duración mínima que el jugador la verá en pantalla. Se configura desde el
/// Inspector del <see cref="LoadingScreenManager"/>.
/// </summary>
[System.Serializable]
public class LoadingScreenEntry
{
    [Tooltip("Identificador único que usarán los scripts para pedir esta pantalla " +
             "(p. ej. \"Controls\", \"Outer->Inner\", \"Inner->Outer2\").")]
    public string key;

    [Tooltip("Imagen a tamaño completo que se muestra durante la carga.")]
    public Sprite contentSprite;

    [Tooltip("Segundos que se mantiene la pantalla (después del fade in y antes " +
             "del fade out). Debe dar tiempo a leer el contenido sin aburrir.")]
    public float duration = 10f;

    [Tooltip("Tipo de fade que se pasará al ChangeSceneManager para los dos " +
             "ciclos (entrada y salida). Si se deja vacío se usa el fade por " +
             "defecto del manager.")]
    public string overrideFade = "";
}

/// <summary>
/// Gestiona pantallas de carga "falsas" reutilizables, encadenando DOS ciclos
/// de fade del <see cref="ChangeSceneManager"/>:
///
///   Ciclo 1 (entrada):
///     escena actual visible → fade OUT a negro → (en negro: activar Loading)
///     → fade IN revelando la pantalla de carga.
///
///   Espera (rellenado de la salpa de izquierda a derecha durante
///   <see cref="LoadingScreenEntry.duration"/> segundos).
///
///   Ciclo 2 (salida):
///     pantalla de carga visible → fade OUT a negro → (en negro: desactivar
///     Loading + cargar la escena destino) → fade IN revelando la escena nueva.
///
/// El mismo manager sirve para todas las pantallas de carga del juego; solo
/// hay que añadir una entrada por cada caso en <see cref="entries"/>.
/// </summary>
public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager instance;

    [Header("Referencias UI")]
    [Tooltip("Image a tamaño completo que muestra el contenido de la pantalla " +
             "(controles, mapa, etc.). Su sprite se cambia por entrada.")]
    public Image contentImage;

    [Tooltip("Image de la salpa que se va rellenando como barra de carga. Debe " +
             "estar configurada como Image Type = Filled, Fill Method = Horizontal, " +
             "Fill Origin = Left, Fill Amount = 0.")]
    public Image salpaFill;

    [Header("Configuración global")]
    [Tooltip("Curva opcional para el relleno de la salpa. X = tiempo normalizado, " +
             "Y = fillAmount. Por defecto lineal.")]
    public AnimationCurve fillCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Tooltip("Pausa al final del rellenado (con la salpa ya llena) antes de " +
             "empezar el fade out hacia la escena nueva. Sirve para que no se " +
             "perciba como un corte brusco.")]
    public float postFillPause = 0.4f;

    [Tooltip("Fade por defecto que se pasa al ChangeSceneManager si la entrada " +
             "no define uno propio.")]
    public string defaultFade = "StandarFade";

    [Header("Entradas reutilizables")]
    [Tooltip("Lista de pantallas de carga disponibles. Cada script que quiera " +
             "lanzar una pantalla pasa la 'key' correspondiente.")]
    public LoadingScreenEntry[] entries;

    private Dictionary<string, LoadingScreenEntry> entryDict;
    private Coroutine fillRoutine;
    private int pendingNextSceneIndex;
    private string pendingFade;
    private float currentDuration;
    private bool loadingActive;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        BuildDictionary();
    }

    private void BuildDictionary()
    {
        entryDict = new Dictionary<string, LoadingScreenEntry>();
        if (entries == null) return;

        foreach (var e in entries)
        {
            if (e == null || string.IsNullOrEmpty(e.key)) continue;
            entryDict[e.key] = e;
        }
    }

    // -----------------------------------------------------------------------
    // API pública
    // -----------------------------------------------------------------------

    /// <summary>
    /// Lanza una pantalla de carga identificada por <paramref name="key"/> y,
    /// al terminar, cambia a la escena <paramref name="nextSceneIndex"/>.
    /// El flujo completo (fade OUT → loading → fade OUT → escena nueva) lo
    /// gestiona internamente este manager.
    ///
    /// Ejemplo de uso:
    ///   LoadingScreenManager.instance.StartLoading("Controls", 2);
    ///   LoadingScreenManager.instance.StartLoading("Outer->Inner", 3);
    /// </summary>
    public void StartLoading(string key, int nextSceneIndex)
    {
        if (loadingActive)
        {
            Debug.LogWarning("[LoadingScreenManager] Ya hay una pantalla de carga en marcha, " +
                             "ignoro la nueva petición.");
  
        }

        if (entryDict == null) BuildDictionary();

        if (!entryDict.TryGetValue(key, out LoadingScreenEntry entry))
        {
            Debug.LogError($"[LoadingScreenManager] No existe entrada con key '{key}'. " +
                           $"Salto a la escena {nextSceneIndex} sin pantalla de carga.");
            FallbackDirectChangeScene(nextSceneIndex);
            return;
        }

        // Pre-configuración: dejamos la imagen y la salpa listas, pero todavía
        // ocultas (UIManager solo activa la capa Loading al cambiar el estado).
        if (contentImage != null) contentImage.sprite = entry.contentSprite;
        if (salpaFill != null) salpaFill.fillAmount = 0f;

        pendingNextSceneIndex = nextSceneIndex;
        pendingFade = string.IsNullOrEmpty(entry.overrideFade) ? defaultFade : entry.overrideFade;
        currentDuration = entry.duration;
        loadingActive = true;

        // Bloqueamos input desde ya, para que durante el primer fade out el
        // jugador no pueda pulsar más botones del menú. Usamos Cinematic
        // porque ya está pensado para "todo bloqueado, no se ve UI".
        GameManager.instance.ChangeState(DataDefinitions.GameStates.Cinematic);

        // CICLO 1: fade out de la escena actual → activar Loading en negro →
        // fade in revelando la pantalla de carga.
        ChangeSceneManager.instance.PlayFadeWithCallback(
            pendingFade,
            onBlack: OnPhase1Black,
            onComplete: OnPhase1Complete
        );
    }

    // -----------------------------------------------------------------------
    // Ciclo 1
    // -----------------------------------------------------------------------

    private void OnPhase1Black()
    {
        // Estamos a negro total: cambiamos el estado a Loading para que el
        // UIManager active la capa (ResetUI + Activate loading) y el
        // InputManager mantenga el input bloqueado. Como pasamos al estado en
        // este frame, cuando el fade in arranque ya estará la capa Loading
        // bajo el negro y aparecerá suavemente.
        GameManager.instance.ChangeState(DataDefinitions.GameStates.Loading);
    }

    private void OnPhase1Complete()
    {
        // Fade in terminado: la pantalla de carga está totalmente visible.
        // Arrancamos el relleno de la salpa.
        if (fillRoutine != null) StopCoroutine(fillRoutine);
        fillRoutine = StartCoroutine(FillThenExit());
    }

    // -----------------------------------------------------------------------
    // Espera + relleno de la salpa
    // -----------------------------------------------------------------------

    private IEnumerator FillThenExit()
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, currentDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            if (salpaFill != null)
                salpaFill.fillAmount = Mathf.Clamp01(fillCurve.Evaluate(t));

            yield return null;
        }

        if (salpaFill != null)
            salpaFill.fillAmount = 1f;

        if (postFillPause > 0f)
            yield return new WaitForSeconds(postFillPause);

        StartPhase2();
    }

    // -----------------------------------------------------------------------
    // Ciclo 2
    // -----------------------------------------------------------------------

    private void StartPhase2()
    {
        // CICLO 2: fade out cubriendo la pantalla de carga → en negro
        // desactivamos la capa Loading y cargamos la escena destino → fade in
        // revelando ya la escena nueva.
        ChangeSceneManager.instance.PlayFadeWithCallback(
            pendingFade,
            onBlack: OnPhase2Black,
            onComplete: OnPhase2Complete
        );
    }

    private void OnPhase2Black()
    {
        // Estamos a negro total: ocultamos la capa Loading (manualmente, no
        // queremos depender de un cambio de estado que dispararía el evento
        // ChangeScene) y cargamos la escena destino. Todo esto pasa mientras
        // la pantalla está completamente negra, así que no se ve el cambio.
        if (UIManager.instance != null)
            UIManager.instance.ActivateUI("loading", false);

        SceneManager.LoadScene(pendingNextSceneIndex);
    }

    private void OnPhase2Complete()
    {
        // Fade in terminado en la escena nueva: pasamos a Play, lo que activa
        // el input del jugador, deja la UI limpia y libera el estado para que
        // el resto del juego funcione con normalidad.
        loadingActive = false;
        GameManager.instance.ChangeState(DataDefinitions.GameStates.Play);
    }

    // -----------------------------------------------------------------------
    // Utilidades
    // -----------------------------------------------------------------------

    private void FallbackDirectChangeScene(int sceneIndex)
    {
        loadingActive = false;
        if (ChangeSceneManager.instance != null && GameManager.instance != null)
        {
            ChangeSceneManager.instance.nextSceneInsdex = sceneIndex;
            ChangeSceneManager.instance.typeOfFade = defaultFade;
            GameManager.instance.ChangeState(DataDefinitions.GameStates.ChangeScene);
        }
    }
}
