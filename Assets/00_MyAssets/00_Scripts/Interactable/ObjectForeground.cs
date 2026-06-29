using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ObjectForeground : MonoBehaviour, Interactable
{
    bool firstInteract;
    public bool showImage;

    public string textToShow1;
    public string textToShow2;

    public int imageType;
    public string spriteKey;

    public bool caninteract;

    [Header("Acción opcional en la primera interacción")]
    [Tooltip("Si se rellena, esta cadena se asigna a GameManager.currentDialogueAction la " +
             "primera vez que se interactúa con el objeto, para encadenar una cinemática " +
             "o un cambio de estado después del diálogo inicial. " +
             "Ejemplos: \"BoatFirstInteract\" (barco), \"FindPapers\" (mesa de Torpere). " +
             "Cada acción comprueba sus propias condiciones en DialogueUIManager.EndDialogue.")]
    public string firstInteractAction;

    [Header("Previsualización en grande (opcional)")]
    [Tooltip("Si está activo, al interactuar con este objeto primero se muestra " +
             "una imagen a tamaño grande (sin diálogo) y al pulsar Confirmar " +
             "(Intro/Espacio) se pasa al diálogo normal con la imagen pequeña. " +
             "El GameObject que se enciende es el que esté asignado en el campo " +
             "'Large Preview' del UIManager — eso evita arrastrar referencias " +
             "entre escenas con el preview que vive en DontDestroyOnLoad.")]
    public bool useLargePreview;

    [Tooltip("Override opcional: si se asigna aquí un GameObject local, se usará " +
             "ese en lugar del 'Large Preview' del UIManager. Útil cuando la mesa " +
             "y el preview están en la misma escena y quieres una referencia " +
             "directa. Si lo dejas vacío y 'Use Large Preview' está activo, se " +
             "recoge del UIManager persistente.")]
    public GameObject largePreviewOverride;

    // Mientras esté en true estamos esperando a que el jugador pulse Confirmar
    // para cerrar la previa. Evita doble-entrada y nos sirve de guard del handler.
    private bool inLargePreview;

    public void Start()
    {
        caninteract = true;
        firstInteract = true;
        inLargePreview = false;

        // Por si la escena se guarda con el preview activo por accidente,
        // arrancamos siempre con él apagado.
        GameObject preview = ResolveLargePreview();
        if (preview != null && preview.activeSelf)
            preview.SetActive(false);
    }

    public void ItsInteracting()
    {
        if (!caninteract) return;
        if (inLargePreview) return; // ya esperando confirmación

        if (useLargePreview && ResolveLargePreview() != null)
        {
            EnterLargePreview();
            return;
        }

        EnterDialoguePhase();
    }

    /// <summary>
    /// Devuelve el GameObject del preview a usar. Prioriza el override
    /// local (si se asignó desde el Inspector). Si no, devuelve el del
    /// <see cref="UIManager"/> persistente.
    /// </summary>
    private GameObject ResolveLargePreview()
    {
        if (largePreviewOverride != null) return largePreviewOverride;
        if (UIManager.instance != null) return UIManager.instance.largePreview;
        return null;
    }

    /// <summary>
    /// Fase 1: activamos el GameObject del dibujo grande sin abrir el
    /// diálogo. Pasamos a estado Reading para que el InputManager active
    /// el mapa UI y podamos escuchar Confirmar.
    /// </summary>
    private void EnterLargePreview()
    {
        inLargePreview = true;

        GameObject preview = ResolveLargePreview();
        if (preview != null) preview.SetActive(true);

        GameManager.instance.ChangeState(DataDefinitions.GameStates.Reading);

        // Apagamos el indicador de interacción del objeto (la flechita).
        if (transform.childCount > 0)
            transform.GetChild(0).gameObject.SetActive(false);

        InputManager.SelectPressedEvent += OnSelectAdvanceFromPreview;
    }

    private void OnSelectAdvanceFromPreview()
    {
        if (!inLargePreview) return;

        InputManager.SelectPressedEvent -= OnSelectAdvanceFromPreview;
        inLargePreview = false;

        GameObject preview = ResolveLargePreview();
        if (preview != null) preview.SetActive(false);

        EnterDialoguePhase();
    }

    /// <summary>
    /// Fase 2 (o única si no hay preview): activar diálogo con la imagen
    /// pequeña y avanzar al estado Reading.
    /// </summary>
    private void EnterDialoguePhase()
    {
        if (firstInteract)
        {
            GameManager.instance.currentDialogueCSV = textToShow1;

            // Si hay acción configurada se la asignamos. La acción se procesa
            // en DialogueUIManager.EndDialogue y es allí donde cada caso
            // comprueba si todavía toca ejecutarse (por ejemplo
            // BoatFirstInteract solo dispara la cinemática si la puerta sigue
            // bloqueada).
            if (!string.IsNullOrEmpty(firstInteractAction))
                GameManager.instance.currentDialogueAction = firstInteractAction;

            firstInteract = false;
        }
        else
        {
            GameManager.instance.currentDialogueCSV = textToShow2;
        }

        UIManager.instance.ActivateUI("dialogue", true);

        if (showImage)
        {
            UIManager.instance.ActivateUI("background", true);
            UIManager.instance.ActivateUI("objectView", true);
            UIImageManager.instance.ShowObjectImage(imageType, spriteKey);
        }

        GameManager.instance.ChangeState(DataDefinitions.GameStates.Reading);

        if (transform.childCount > 0)
            transform.GetChild(0).gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // Sanity: si el objeto se desactiva mientras estábamos en preview
        // (cambio de escena, p. ej.), desuscribimos para no dejar el
        // handler colgado y dejamos el GameObject del preview apagado.
        if (inLargePreview)
        {
            InputManager.SelectPressedEvent -= OnSelectAdvanceFromPreview;
            inLargePreview = false;
            GameObject preview = ResolveLargePreview();
            if (preview != null) preview.SetActive(false);
        }
    }

    /// <summary>
    /// Vuelve a marcar este objeto como "no interactuado todavía". Útil
    /// cuando un evento de la historia cambia el contenido del objeto
    /// (textos, sprite, etc.) y queremos que la próxima interacción
    /// vuelva a mostrar el textToShow1 y a disparar el firstInteractAction.
    /// </summary>
    public void ResetFirstInteract()
    {
        firstInteract = true;
    }
}
