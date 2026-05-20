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
// Nota: se ha quitado [ExecuteAlways] a propósito. En modo edición, si la
// Game window o la Scene window reportan Screen.width/height = 0 (cosa que
// pasa al cambiar el layout del Editor o al colapsar pestañas), este script
// terminaba escribiendo un cam.rect con NaN/Infinity y eso disparaba
// cientos de warnings tipo "The bounds contain one of the following values:
// NaN, float.PositiveInfinity, float.NegativeInfinity" cada frame. La
// proporción de aspecto solo importa al jugar, así que ejecutamos en Play.
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

        // Sanity reset: si la cámara llega con un rect ya envenenado de
        // antes (NaN/Infinity), Apply() podría hacer early return en este
        // frame (p. ej. si Screen.width aún es 0) y dejarlo así. Ponemos
        // el rect a pantalla completa como suelo seguro antes de calcular.
        if (cam != null)
            cam.rect = new Rect(0f, 0f, 1f, 1f);

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
        // Sanity check del rect actual: si por lo que sea (otro script
        // tocando la cámara, una restauración mala desde una sesión
        // previa, etc.) el cam.rect ha quedado con valores no finitos,
        // lo reseteamos a pantalla completa y forzamos un Apply. Esto
        // sirve de red de seguridad incluso si la causa del veneno no
        // es nuestra.
        if (cam != null && !IsRectSane(cam.rect))
        {
            cam.rect = new Rect(0f, 0f, 1f, 1f);
            Apply();
            return;
        }

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

        // Si la ventana del Editor (o el Game tab) está colapsada y reporta
        // 0px, NO tocamos cam.rect: la división daría Infinity/NaN y luego
        // Unity escupiría miles de warnings tipo
        // "The bounds contain one of the following values: NaN, ...".
        // Esperamos a que el siguiente Update nos traiga un tamaño válido.
        int sw = Screen.width;
        int sh = Screen.height;
        if (sw <= 0 || sh <= 0) return;

        lastWidth = sw;
        lastHeight = sh;
        lastTargetW = targetWidth;
        lastTargetH = targetHeight;

        float targetAspect = targetWidth / targetHeight;
        float windowAspect = (float)sw / sh;
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

        // Validación final: si por lo que sea el rect ha salido con valores
        // no finitos o fuera del rango [0..1], no lo aplicamos. Mejor un
        // frame con el rect anterior que un cam.rect con NaN que envenene
        // todos los cálculos de bounds del resto del frame.
        if (!IsRectSane(rect)) return;

        cam.rect = rect;
    }

    private static bool IsRectSane(Rect r)
    {
        if (!IsFinite(r.x) || !IsFinite(r.y)) return false;
        if (!IsFinite(r.width) || !IsFinite(r.height)) return false;
        if (r.width <= 0f || r.height <= 0f) return false;
        if (r.x < 0f || r.y < 0f) return false;
        if (r.x + r.width > 1.0001f) return false;
        if (r.y + r.height > 1.0001f) return false;
        return true;
    }

    private static bool IsFinite(float v)
    {
        return !float.IsNaN(v) && !float.IsInfinity(v);
    }
}
