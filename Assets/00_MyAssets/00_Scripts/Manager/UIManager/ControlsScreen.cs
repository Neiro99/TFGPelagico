using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Componente que rellena un <see cref="TMP_Text"/> con la lista de controles
/// del juego usando un formato bonito en dos columnas (acción · tecla) y
/// con una "ventana" deslizante: solo se muestran a la vez
/// <see cref="windowSize"/> entradas. El jugador navega con las mismas
/// teclas que el menú principal (W/S/flechas arriba-abajo), igual que se
/// recorre el Main Menu, y la ventana se va desplazando para mostrar
/// las entradas siguientes/anteriores.
///
/// Si añades o quitas un control en el InputManager, basta con tocar
/// <see cref="entries"/> (o <see cref="DefaultEntries"/> si está vacía) y
/// el carrusel se ajusta solo, sin tener que cambiar nada en la escena.
///
/// Nota: este script NO usa <c>[ExecuteAlways]</c> a propósito. En modo
/// edición TextMeshPro puede no tener su mesh inicializado cuando algo
/// fuerza un <c>ForceMeshUpdate</c>, y eso provoca warnings tipo
/// "The bounds contain one of the following values: NaN, float.PositiveInfinity,
/// float.NegativeInfinity". Solo ejecutamos lógica en Play.
/// </summary>
public class ControlsScreen : MonoBehaviour
{
    /// <summary>
    /// Una entrada de la lista de controles: acción mostrada al jugador y la
    /// tecla (o teclas) que la disparan.
    /// </summary>
    [System.Serializable]
    public class ControlEntry
    {
        [Tooltip("Texto visible para esta acción (p. ej. \"Moverse\").")]
        public string action;

        [Tooltip("Tecla o combinación de teclas asociada (p. ej. \"WASD / Flechas\").")]
        public string key;
    }

    [Header("Referencia UI")]
    [Tooltip("TextMeshPro donde se va a pintar la lista de controles. Si no se " +
             "asigna manualmente, se busca uno en este mismo GameObject o en sus " +
             "hijos al despertar.")]
    public TMP_Text targetLabel;

    [Header("Opcional: título por encima de la lista")]
    [Tooltip("Si se rellena, se muestra como cabecera en negrita por encima de " +
             "los controles. Déjalo vacío si tu panel ya tiene su propio título.")]
    public string title = "";

    [Header("Estilo")]
    [Tooltip("Tamaño relativo del título respecto al texto base (en %).")]
    [Range(80, 200)] public int titleSizePercent = 130;

    [Tooltip("Color de las teclas. Por defecto se usa el mismo que el del texto.")]
    public bool tintKeys = true;
    public Color keysColor = new Color(0.95f, 0.78f, 0.32f, 1f); // dorado suave

    [Tooltip("Separación vertical entre filas (en líneas en blanco).")]
    [Range(0, 3)] public int rowSpacing = 1;

    [Header("Carrusel")]
    [Tooltip("Cuántas entradas se muestran a la vez en pantalla. El resto " +
             "queda oculto y se va revelando al navegar.")]
    [Range(1, 12)] public int windowSize = 5;

    [Tooltip("Si está activo, mostramos pequeños indicadores arriba/abajo " +
             "(▲ / ▼) cuando hay más entradas fuera de la ventana visible.")]
    public bool showScrollIndicators = true;

    [Tooltip("Color de los indicadores ▲ / ▼. Si lo dejas a (0,0,0,0) se usa " +
             "el color del texto base.")]
    // #3B4A53 — gris azulado oscuro de la paleta del juego.
    public Color indicatorColor = new Color(0.2313725f, 0.2901961f, 0.3254902f, 1f);

    [Header("Contenido")]
    [Tooltip("Lista de controles a mostrar. Si la dejas vacía se usa la lista " +
             "por defecto (la que coincide con el InputManager actual).")]
    public List<ControlEntry> entries = new List<ControlEntry>();

    // Índice de la primera entrada visible. La ventana visible es siempre
    // [topIndex, topIndex + windowSize).
    private int topIndex;

    private void Reset()
    {
        // Cuando arrastras el componente nuevo al GameObject en el Editor,
        // prerellenamos la lista con los controles reales del juego para que
        // se vea ya algo en pantalla sin tener que rellenarlos a mano.
        entries = DefaultEntries();
        title = "Controles";
    }

    private void OnEnable()
    {
        topIndex = 0;

        // En modo edición no tocamos nada: TextMeshPro puede no estar
        // listo y provocar warnings de bounds NaN. Solo ejecutamos
        // lógica cuando el juego está en Play.
        if (!Application.isPlaying) return;

        Refresh();

        // El primer Refresh() se ejecuta en el mismo frame en que el
        // GameObject se acaba de activar; algunos layouts UI (sobre todo
        // si hay un ContentSizeFitter / VerticalLayoutGroup en la jerarquía)
        // no han calculado todavía su tamaño y el texto puede aparecer
        // mal colocado hasta que algo lo "menea". Forzamos un rebuild
        // inmediato y otro un frame después para garantizar que se ve
        // bien posicionado desde el primer instante.
        ForceLayoutRebuild();
        StartCoroutine(DelayedRefresh());

        InputManager.MoveUpPressedEvent += ScrollUp;
        InputManager.MoveDownPressedEvent += ScrollDown;
    }

    private IEnumerator DelayedRefresh()
    {
        // Esperamos un frame para que Unity termine de procesar la
        // activación del GameObject y la jerarquía de Canvas, y volvemos
        // a refrescar y a forzar el layout.
        yield return null;
        if (!Application.isPlaying) yield break;
        Refresh();
        ForceLayoutRebuild();
    }

    private void OnDisable()
    {
        if (!Application.isPlaying) return;

        InputManager.MoveUpPressedEvent -= ScrollUp;
        InputManager.MoveDownPressedEvent -= ScrollDown;
    }

    // OnValidate eliminado a propósito: refrescar el TMP_Text en modo
    // edición es justo lo que provocaba los warnings de bounds NaN cuando
    // el mesh del label aún no estaba calculado. Si necesitas previsualizar
    // los cambios de la lista en el Editor, basta con entrar y salir del
    // GameObject (toggle del check) o usar el ContextMenu "Refresh Preview"
    // expuesto más abajo.
    [ContextMenu("Refresh Preview")]
    private void ContextMenuRefresh()
    {
        if (!Application.isPlaying)
        {
            // En edición protegemos extra: solo intentamos refrescar si el
            // GameObject está activo y tiene un label asignado.
            if (!gameObject.activeInHierarchy || targetLabel == null) return;
        }
        Refresh();
    }

    // -----------------------------------------------------------------------
    // Navegación del carrusel
    // -----------------------------------------------------------------------

    /// <summary>
    /// Mueve la ventana una posición hacia arriba (entrada anterior pasa a
    /// ser la primera visible). Si ya estamos en lo alto, ignora la pulsación
    /// (sin wrap): se queda en el tope.
    /// </summary>
    public void ScrollUp()
    {
        var list = ActiveList();
        if (list == null || list.Count == 0) return;

        if (topIndex <= 0) return; // límite superior, no hacemos nada
        topIndex--;
        Refresh();
    }

    /// <summary>
    /// Mueve la ventana una posición hacia abajo. Si ya estamos en lo bajo,
    /// ignora la pulsación (sin wrap): se queda en el fondo.
    /// </summary>
    public void ScrollDown()
    {
        var list = ActiveList();
        if (list == null || list.Count == 0) return;

        int maxTop = Mathf.Max(0, list.Count - windowSize);
        if (topIndex >= maxTop) return; // límite inferior, no hacemos nada
        topIndex++;
        Refresh();
    }

    // -----------------------------------------------------------------------
    // Pintado del texto
    // -----------------------------------------------------------------------

    /// <summary>
    /// Reconstruye el texto del label con la lista de entradas actuales y la
    /// ventana visible. Llámalo manualmente si modificas la lista por código
    /// en runtime.
    /// </summary>
    public void Refresh()
    {
        if (targetLabel == null)
        {
            targetLabel = GetComponent<TMP_Text>();
            if (targetLabel == null)
                targetLabel = GetComponentInChildren<TMP_Text>(true);
        }
        if (targetLabel == null) return;

        var list = ActiveList();
        int total = list.Count;

        // Saneamos topIndex por si han cambiado el número de entradas o el
        // windowSize desde el Inspector mientras estaba activo el panel.
        int maxTop = Mathf.Max(0, total - windowSize);
        topIndex = Mathf.Clamp(topIndex, 0, maxTop);

        int visible = Mathf.Min(windowSize, total);
        bool hasMoreAbove = topIndex > 0;
        bool hasMoreBelow = (topIndex + visible) < total;

        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(title))
        {
            sb.Append("<b><size=").Append(titleSizePercent).Append("%>")
              .Append(title)
              .Append("</size></b>");
            sb.AppendLine();
            for (int i = 0; i < rowSpacing; i++) sb.AppendLine();
        }

        // Indicador superior: "▲" si hay entradas por encima de la ventana.
        if (showScrollIndicators)
            sb.Append(BuildIndicator("▲", hasMoreAbove)).AppendLine();

        // Tags de color para resaltar las teclas.
        string keyOpen = tintKeys ? $"<color=#{ColorUtility.ToHtmlStringRGB(keysColor)}>" : "";
        string keyClose = tintKeys ? "</color>" : "";

        for (int i = 0; i < visible; i++)
        {
            int idx = topIndex + i;
            if (idx >= total) break;
            var e = list[idx];
            if (e == null || string.IsNullOrEmpty(e.action)) continue;

            sb.Append(e.action)
              .Append("   ·   ")
              .Append(keyOpen).Append(e.key).Append(keyClose);

            if (i < visible - 1)
            {
                sb.AppendLine();
                for (int j = 0; j < rowSpacing; j++) sb.AppendLine();
            }
        }

        // Indicador inferior: "▼" si hay entradas por debajo de la ventana.
        if (showScrollIndicators)
        {
            sb.AppendLine();
            sb.Append(BuildIndicator("▼", hasMoreBelow));
        }

        targetLabel.richText = true;
        targetLabel.text = sb.ToString();

        // Forzamos a TextMeshPro a recalcular su mesh ya mismo, en vez de
        // esperar al final del frame. Sin esto, la primera vez que se
        // activa el panel el texto puede aparecer descolocado hasta que
        // algo (p. ej. un scroll) provoque un repintado.
        //
        // OJO: solo en runtime. En modo edición el mesh del TMP puede no
        // estar inicializado y ForceMeshUpdate genera bounds NaN /
        // PositiveInfinity, que es exactamente el warning que estaríamos
        // intentando evitar.
        if (Application.isPlaying)
            targetLabel.ForceMeshUpdate();
    }

    /// <summary>
    /// Fuerza un rebuild inmediato del layout en la jerarquía del label.
    /// Es la artillería pesada contra el efecto "el texto aparece mal
    /// colocado el primer frame y se autocoloca al pulsar una tecla".
    /// Solo se llama en runtime para evitar tocar la UI en edición.
    /// </summary>
    private void ForceLayoutRebuild()
    {
        if (!Application.isPlaying) return;
        if (targetLabel == null) return;

        var rt = targetLabel.rectTransform;
        if (rt == null) return;

        // Subimos al RectTransform raíz del propio panel para rebuildear
        // toda la jerarquía visible, no solo el label.
        RectTransform root = rt;
        if (transform is RectTransform myRt) root = myRt;

        LayoutRebuilder.ForceRebuildLayoutImmediate(root);
        Canvas.ForceUpdateCanvases();
    }

    /// <summary>
    /// Construye la línea del indicador (▲ o ▼). Si <paramref name="visible"/>
    /// es false, mantenemos un espacio en blanco del mismo tamaño para que
    /// las filas no "salten" verticalmente al cambiar de página.
    /// </summary>
    private string BuildIndicator(string glyph, bool visible)
    {
        if (!visible)
        {
            // Caracter invisible con el mismo tamaño para conservar la altura.
            return "<alpha=#00>" + glyph + "<alpha=#FF>";
        }

        if (indicatorColor.a <= 0f)
            return glyph;

        return $"<color=#{ColorUtility.ToHtmlStringRGBA(indicatorColor)}>{glyph}</color>";
    }

    /// <summary>
    /// Devuelve la lista activa: la del Inspector si tiene entradas, o la
    /// lista por defecto si está vacía.
    /// </summary>
    private List<ControlEntry> ActiveList()
    {
        return (entries != null && entries.Count > 0) ? entries : DefaultEntries();
    }

    /// <summary>
    /// Lista por defecto, ajustada al InputManager / PlayerInputActions actuales:
    ///   - Moverse:    WASD / Flechas       (InputActions: MoveUp/Down/Left/Right)
    ///   - Interactuar: E                   (InputActions: Player.Interact)
    ///   - Confirmar:   Intro / Espacio     (InputActions: UI.Select)
    ///   - Pausar:      Esc                 (InputActions: Pause)
    ///   - Diario:      Q                   (InputManager.cs, KeyDown Q)
    ///   - Coger / Soltar: Espacio          (InputManager.cs, KeyDown Space — puzzle)
    ///   - Girar pieza: G                   (InputManager.cs, KeyDown G — puzzle)
    /// Si añades un control nuevo en el InputManager, súmalo aquí.
    /// </summary>
    public static List<ControlEntry> DefaultEntries()
    {
        return new List<ControlEntry>
        {
            new ControlEntry { action = "Moverse",         key = "WASD / Flechas" },
            new ControlEntry { action = "Interactuar",     key = "E" },
            new ControlEntry { action = "Confirmar",       key = "Intro / Espacio" },
            new ControlEntry { action = "Pausar",          key = "Esc" },
            new ControlEntry { action = "Abrir diario",    key = "Q" },
            new ControlEntry { action = "Coger / Soltar",  key = "Espacio" },
            new ControlEntry { action = "Girar pieza",     key = "G" },
        };
    }
}
