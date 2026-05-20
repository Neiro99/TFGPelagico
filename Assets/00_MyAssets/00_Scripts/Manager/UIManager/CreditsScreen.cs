using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Versión "Créditos" del carrusel deslizante de <see cref="ControlsScreen"/>.
/// Funciona exactamente igual: rellena un <see cref="TMP_Text"/> con la lista
/// de entradas (rol / persona) y permite navegar con W/S/flechas si hay más
/// elementos de los que caben en pantalla (<see cref="windowSize"/>). Los
/// extremos no hacen wrap: si estás arriba del todo no sube más, si estás
/// abajo del todo no baja más.
///
/// Por simplicidad mantenemos los dos componentes separados (en vez de
/// generalizar uno solo) para no tocar el ControlsScreen que ya está en
/// escena y funcionando. Si en el futuro hace falta otro panel con scroll,
/// se puede refactorizar a una clase base común.
///
/// Nota: este script NO usa <c>[ExecuteAlways]</c> a propósito. En modo
/// edición TextMeshPro puede no tener su mesh inicializado cuando algo
/// fuerza un <c>ForceMeshUpdate</c>, y eso provoca warnings tipo
/// "The bounds contain one of the following values: NaN, ...".
/// Solo ejecutamos lógica en Play.
/// </summary>
public class CreditsScreen : MonoBehaviour
{
    /// <summary>
    /// Una entrada de los créditos: el rol/categoría y la persona o personas
    /// que lo cubren.
    /// </summary>
    [System.Serializable]
    public class CreditEntry
    {
        [Tooltip("Rol o categoría (p. ej. \"Programación\").")]
        public string role;

        [Tooltip("Persona o personas a las que se atribuye ese rol.")]
        public string name;
    }

    [Header("Referencia UI")]
    [Tooltip("TextMeshPro donde se va a pintar la lista de créditos. Si no se " +
             "asigna manualmente, se busca uno en este mismo GameObject o en sus " +
             "hijos al despertar.")]
    public TMP_Text targetLabel;

    [Header("Opcional: título por encima de la lista")]
    [Tooltip("Si se rellena, se muestra como cabecera en negrita por encima de " +
             "los créditos. Déjalo vacío si tu panel ya tiene su propio título.")]
    public string title = "";

    [Header("Estilo")]
    [Tooltip("Tamaño relativo del título respecto al texto base (en %).")]
    [Range(80, 200)] public int titleSizePercent = 130;

    [Tooltip("Si está activo, los nombres se pintan con un color distinto al " +
             "rol para que sea más fácil leer dos columnas.")]
    public bool tintNames = true;
    public Color namesColor = new Color(0.95f, 0.78f, 0.32f, 1f); // dorado suave

    [Tooltip("Separación vertical entre filas (en líneas en blanco).")]
    [Range(0, 3)] public int rowSpacing = 1;

    [Header("Carrusel")]
    [Tooltip("Cuántas entradas se muestran a la vez en pantalla. El resto " +
             "queda oculto y se va revelando al navegar con W/S/flechas.")]
    [Range(1, 12)] public int windowSize = 4;

    [Tooltip("Si está activo, mostramos pequeños indicadores arriba/abajo " +
             "(▲ / ▼) cuando hay más entradas fuera de la ventana visible.")]
    public bool showScrollIndicators = true;

    [Tooltip("Color de los indicadores ▲ / ▼. Si lo dejas a (0,0,0,0) se usa " +
             "el color del texto base.")]
    // #3B4A53 — gris azulado oscuro de la paleta del juego (coherente con
    // los indicadores de la pantalla de Controles).
    public Color indicatorColor = new Color(0.2313725f, 0.2901961f, 0.3254902f, 1f);

    [Header("Contenido")]
    [Tooltip("Lista de créditos a mostrar. Si la dejas vacía se usa la lista " +
             "por defecto.")]
    public List<CreditEntry> entries = new List<CreditEntry>();

    // Índice de la primera entrada visible. La ventana es siempre
    // [topIndex, topIndex + windowSize).
    private int topIndex;

    private void Reset()
    {
        // Cuando arrastras el componente nuevo al GameObject en el Editor,
        // prerellenamos la lista con los créditos para que se vea ya algo
        // en pantalla sin tener que rellenarlos a mano.
        entries = DefaultEntries();
        title = "Créditos";
    }

    private void OnEnable()
    {
        topIndex = 0;

        // En modo edición no tocamos nada: TextMeshPro puede no estar listo
        // y provocar warnings de bounds NaN. Solo en Play.
        if (!Application.isPlaying) return;

        Refresh();

        // Misma artillería que ControlsScreen para evitar que el texto
        // aparezca descolocado el primer frame al abrir el panel.
        ForceLayoutRebuild();
        StartCoroutine(DelayedRefresh());

        InputManager.MoveUpPressedEvent += ScrollUp;
        InputManager.MoveDownPressedEvent += ScrollDown;
    }

    private IEnumerator DelayedRefresh()
    {
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

    [ContextMenu("Refresh Preview")]
    private void ContextMenuRefresh()
    {
        if (!Application.isPlaying)
        {
            if (!gameObject.activeInHierarchy || targetLabel == null) return;
        }
        Refresh();
    }

    // -----------------------------------------------------------------------
    // Navegación del carrusel
    // -----------------------------------------------------------------------

    /// <summary>
    /// Sube una posición. Si estamos en lo alto, no hace nada (sin wrap).
    /// </summary>
    public void ScrollUp()
    {
        var list = ActiveList();
        if (list == null || list.Count == 0) return;

        if (topIndex <= 0) return;
        topIndex--;
        Refresh();
    }

    /// <summary>
    /// Baja una posición. Si estamos en el fondo, no hace nada (sin wrap).
    /// </summary>
    public void ScrollDown()
    {
        var list = ActiveList();
        if (list == null || list.Count == 0) return;

        int maxTop = Mathf.Max(0, list.Count - windowSize);
        if (topIndex >= maxTop) return;
        topIndex++;
        Refresh();
    }

    // -----------------------------------------------------------------------
    // Pintado del texto
    // -----------------------------------------------------------------------

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

        if (showScrollIndicators)
            sb.Append(BuildIndicator("▲", hasMoreAbove)).AppendLine();

        string nameOpen = tintNames ? $"<color=#{ColorUtility.ToHtmlStringRGB(namesColor)}>" : "";
        string nameClose = tintNames ? "</color>" : "";

        for (int i = 0; i < visible; i++)
        {
            int idx = topIndex + i;
            if (idx >= total) break;
            var e = list[idx];
            if (e == null || string.IsNullOrEmpty(e.role)) continue;

            // Patrón "Rol · Nombre", coherente visualmente con la pantalla
            // de Controles. Si los nombres son muy largos, ajusta el
            // windowSize o el RectTransform del label.
            sb.Append(e.role)
              .Append("   ·   ")
              .Append(nameOpen).Append(e.name).Append(nameClose);

            if (i < visible - 1)
            {
                sb.AppendLine();
                for (int j = 0; j < rowSpacing; j++) sb.AppendLine();
            }
        }

        if (showScrollIndicators)
        {
            sb.AppendLine();
            sb.Append(BuildIndicator("▼", hasMoreBelow));
        }

        targetLabel.richText = true;
        targetLabel.text = sb.ToString();

        if (Application.isPlaying)
            targetLabel.ForceMeshUpdate();
    }

    private void ForceLayoutRebuild()
    {
        if (!Application.isPlaying) return;
        if (targetLabel == null) return;

        var rt = targetLabel.rectTransform;
        if (rt == null) return;

        RectTransform root = rt;
        if (transform is RectTransform myRt) root = myRt;

        LayoutRebuilder.ForceRebuildLayoutImmediate(root);
        Canvas.ForceUpdateCanvases();
    }

    private string BuildIndicator(string glyph, bool visible)
    {
        if (!visible)
            return "<alpha=#00>" + glyph + "<alpha=#FF>";

        if (indicatorColor.a <= 0f)
            return glyph;

        return $"<color=#{ColorUtility.ToHtmlStringRGBA(indicatorColor)}>{glyph}</color>";
    }

    private List<CreditEntry> ActiveList()
    {
        return (entries != null && entries.Count > 0) ? entries : DefaultEntries();
    }

    /// <summary>
    /// Lista por defecto: créditos del equipo de Pelágico.
    /// Si añades / quitas a alguien, basta con tocar esta lista o las
    /// entries del Inspector.
    /// </summary>
    public static List<CreditEntry> DefaultEntries()
    {
        return new List<CreditEntry>
        {
            new CreditEntry { role = "Narrativa",    name = "Patricia (Chirone) Navero Muñoz" },
            new CreditEntry { role = "Dirección",    name = "Patricia (Chirone) Navero Muñoz" },
            new CreditEntry { role = "Producción",   name = "Patricia (Chirone) Navero Muñoz" },
            new CreditEntry { role = "Programación", name = "Neiro Jiménez Barrio" },
            new CreditEntry { role = "Concept art",  name = "Patricia (Chirone) Navero Muñoz y Jhonatan Romero Santo" },
            new CreditEntry { role = "Arte final",   name = "Patricia (Chirone) Navero Muñoz" },
            new CreditEntry { role = "Animación",    name = "Eloisa Herrera Gómez y Patricia (Chirone) Navero Muñoz" },
            new CreditEntry { role = "Iluminación",  name = "Delia Stuben y Neiro Jiménez Barrio" },
            new CreditEntry { role = "Diseño",       name = "Patricia (Chirone) Navero Muñoz" },
            new CreditEntry { role = "Sonido",       name = "Neiro Jiménez Barrio" },
            new CreditEntry { role = "Música",       name = "Pixabay" },
        };
    }
}
