using UnityEngine;

/// <summary>
/// Mantiene una proporción de aspecto fija (por defecto 16:9 = 1920x1080)
/// en la cámara, añadiendo bandas negras a los lados (pantallas ultrawide)
/// o arriba y abajo (pantallas verticales o cuadradas) según haga falta.
///
/// Cómo usarlo:
///   1. Añade este componente a la cámara principal del juego.
///   2. En esa misma cámara, pon Clear Flags = Solid Color y Background = negro.
///   3. Para que las bandas negras se vean siempre negras (y no como un "ghost"
///      del frame anterior), añade en la escena una segunda cámara con:
///         - Clear Flags: Solid Color, Background: negro
///         - Culling Mask: Nothing
///         - Depth: menor que la cámara principal (por ejemplo -10)
///         - Viewport Rect: (0, 0, 1, 1)
///      Esa cámara "limpia" el fondo completo a negro antes de que la cámara
///      principal pinte su viewport letterboxed encima.
/// </summary>
[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class AspectRatioEnforcer : MonoBehaviour
{
    [Header("Proporción objetivo")]
    [Tooltip("Ancho del aspecto objetivo (1920 para Full HD).")]
    public float targetWidth = 1920f;
    [Tooltip("Alto del aspecto objetivo (1080 para Full HD).")]
    public float targetHeight = 1080f;

    private Camera cam;
    private int lastWidth;
    private int lastHeight;
    private float lastTargetW;
    private float lastTargetH;

    private void OnEnable()
    {
        cam = GetComponent<Camera>();
        Apply();
    }

    private void OnDisable()
    {
        // Al desactivar, devolvemos la cámara a pantalla completa para no
        // dejar el viewport recortado si quitamos el script en tiempo de juego.
        if (cam != null)
            cam.rect = new Rect(0f, 0f, 1f, 1f);
    }

    private void Update()
    {
        // Solo recalculamos si cambia el tamaño de la ventana o los valores
        // objetivo. Así no toqueteamos el Rect cada frame innecesariamente.
        if (Screen.width != lastWidth
            || Screen.height != lastHeight
            || !Mathf.Approximately(targetWidth, lastTargetW)
            || !Mathf.Approximately(targetHeight, lastTargetH))
        {
            Apply();
        }
    }

    private void Apply()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) return;
        if (targetWidth <= 0f || targetHeight <= 0f) return;

        lastWidth = Screen.width;
        lastHeight = Screen.height;
        lastTargetW = targetWidth;
        lastTargetH = targetHeight;

        float targetAspect = targetWidth / targetHeight;
        float windowAspect = (float)Screen.width / Screen.height;
        float scale = windowAspect / targetAspect;

        Rect rect = new Rect(0f, 0f, 1f, 1f);

        if (scale < 1f)
        {
            // La ventana es más alta (proporcionalmente) que el objetivo
            // → recortamos en altura y dejamos bandas arriba y abajo.
            rect.width  = 1f;
            rect.height = scale;
            rect.x = 0f;
            rect.y = (1f - scale) / 2f;
        }
        else
        {
            // La ventana es más ancha que el objetivo
            // → recortamos en anchura y dejamos bandas a izquierda y derecha.
            float inv = 1f / scale;
            rect.width  = inv;
            rect.height = 1f;
            rect.x = (1f - inv) / 2f;
            rect.y = 0f;
        }

        cam.rect = rect;
    }
}
