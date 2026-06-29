using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor de diálogos en Unity. Permite editar los CSV de
/// Assets/00_MyAssets/08_Dialogues/Resources/*.csv sin tocar el archivo a
/// mano. Muestra cada línea con personaje y texto, ofrece botones de
/// formato (negrita / cursiva / color) que envuelven la selección actual
/// del campo de texto, y muestra previsualización en vivo del formato.
///
/// Menú: Tools -> Dialogue Editor.
/// </summary>
public class DialogueEditorWindow : EditorWindow
{
    private List<DialogueRow> rows = new List<DialogueRow>();
    private string currentFile;
    private string currentFilePath;
    private bool dirty;

    private Vector2 scrollLeft;
    private Vector2 scrollRight;

    private List<string> csvList = new List<string>();

    private const string CsvRelativeFolder = "Assets/00_MyAssets/08_Dialogues/Resources";

    private GUIStyle previewStyle;

    private class DialogueRow
    {
        public string name;
        public string text;
        public string isDecision;
        public string options;
        public string nextLines;
        public string affinityChange;

        public bool IsDecisionRow =>
            !string.IsNullOrWhiteSpace(isDecision) &&
            isDecision.Trim().ToLowerInvariant() == "true";
    }

    [MenuItem("Tools/Dialogue Editor")]
    public static void Open()
    {
        var w = GetWindow<DialogueEditorWindow>("Dialogue Editor");
        w.minSize = new Vector2(720, 420);
        w.Show();
    }

    private void OnEnable()
    {
        RefreshCsvList();
        wantsMouseMove = true;
    }

    private void RefreshCsvList()
    {
        csvList.Clear();
        string abs = Path.GetFullPath(Path.Combine(Application.dataPath, "..", CsvRelativeFolder));
        if (!Directory.Exists(abs)) return;
        foreach (var f in Directory.GetFiles(abs, "*.csv"))
            csvList.Add(Path.GetFileNameWithoutExtension(f));
        csvList.Sort();
    }

    private void OnGUI()
    {
        if (previewStyle == null)
        {
            previewStyle = new GUIStyle(EditorStyles.label)
            {
                richText = true,
                wordWrap = true,
                padding = new RectOffset(6, 6, 6, 6),
                fontSize = 12,
            };
        }

        DrawToolbar();

        EditorGUILayout.BeginHorizontal();
        DrawSidebar();
        DrawEditor();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("Recargar lista", EditorStyles.toolbarButton, GUILayout.Width(110)))
            RefreshCsvList();

        GUI.enabled = !string.IsNullOrEmpty(currentFile);
        if (GUILayout.Button("Guardar (Ctrl+S)", EditorStyles.toolbarButton, GUILayout.Width(140)))
            SaveCurrent();
        GUI.enabled = true;

        GUILayout.FlexibleSpace();

        string status;
        if (string.IsNullOrEmpty(currentFile))
            status = "Selecciona un diálogo a la izquierda";
        else
            status = "Editando: " + currentFile + (dirty ? "  •  (cambios sin guardar)" : "");
        GUILayout.Label(status, EditorStyles.miniLabel);

        EditorGUILayout.EndHorizontal();

        if (Event.current.type == EventType.KeyDown &&
            Event.current.keyCode == KeyCode.S &&
            (Event.current.control || Event.current.command) &&
            !string.IsNullOrEmpty(currentFile))
        {
            SaveCurrent();
            Event.current.Use();
        }
    }

    private void DrawSidebar()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(180));
        EditorGUILayout.LabelField("Diálogos", EditorStyles.boldLabel);

        scrollLeft = EditorGUILayout.BeginScrollView(scrollLeft);

        foreach (var name in csvList)
        {
            bool isSelected = name == currentFile;
            var style = isSelected ? EditorStyles.boldLabel : EditorStyles.label;
            if (GUILayout.Button(name, style, GUILayout.ExpandWidth(true)))
            {
                if (dirty)
                {
                    bool ok = EditorUtility.DisplayDialog(
                        "Cambios sin guardar",
                        "'" + currentFile + "' tiene cambios sin guardar. ¿Descartarlos?",
                        "Descartar", "Cancelar");
                    if (!ok) return;
                }
                LoadCsv(name);
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawEditor()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        if (string.IsNullOrEmpty(currentFile))
        {
            EditorGUILayout.HelpBox(
                "Selecciona un diálogo de la lista de la izquierda.\n\n" +
                "Cómo usar:\n" +
                "  - Edita Personaje y Texto de cada línea.\n" +
                "  - Para formato, escribe los tags directamente en el texto " +
                "(p. ej. <b>palabra</b>). La cabecera de arriba te lista los tags " +
                "disponibles.\n" +
                "  - La 'Vista previa' debajo de cada línea muestra cómo se verá en el juego.\n" +
                "  - Añade líneas con '+ Añadir línea al final', reordena con ↑↓, borra con X.\n" +
                "  - Guarda con Ctrl+S.\n" +
                "  - Las DECISIONES se muestran en solo lectura para no romperlas.",
                MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }

        // Panel de ayuda con los tags disponibles. Siempre visible mientras
        // estás editando un CSV, así quien escribe los diálogos lo tiene
        // a mano sin tener que recordarlo de memoria.
        EditorGUILayout.HelpBox(
            "Para dar formato escribe los tags directamente en el texto:\n" +
            "    <b>palabra</b> -> negrita\n" +
            "    <i>palabra</i> -> cursiva\n" +
            "    <color=#A00>palabra</color> -> color (cambia el hex)\n" +
            "    <size=120%>palabra</size> -> tamaño\n\n" +
            "La 'Vista previa' debajo de cada línea muestra cómo se verá " +
            "en el juego con TextMeshPro.",
            MessageType.None);

        scrollRight = EditorGUILayout.BeginScrollView(scrollRight);

        for (int i = 0; i < rows.Count; i++)
        {
            DrawRow(i);
            EditorGUILayout.Space(4);
        }

        EditorGUILayout.Space(8);
        if (GUILayout.Button("+ Añadir línea al final", GUILayout.Height(24)))
        {
            rows.Add(new DialogueRow { name = "", text = "" });
            dirty = true;
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawRow(int i)
    {
        var row = rows[i];

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("#" + i, GUILayout.Width(32));
        if (row.IsDecisionRow)
            GUILayout.Label("  [DECISIÓN - solo lectura]", EditorStyles.miniBoldLabel);
        GUILayout.FlexibleSpace();

        GUI.enabled = i > 0;
        if (GUILayout.Button("↑", GUILayout.Width(24)))
        {
            (rows[i], rows[i - 1]) = (rows[i - 1], rows[i]);
            dirty = true;
            GUI.FocusControl(null);
        }
        GUI.enabled = i < rows.Count - 1;
        if (GUILayout.Button("↓", GUILayout.Width(24)))
        {
            (rows[i], rows[i + 1]) = (rows[i + 1], rows[i]);
            dirty = true;
            GUI.FocusControl(null);
        }
        GUI.enabled = true;
        if (GUILayout.Button("X", GUILayout.Width(24)))
        {
            if (EditorUtility.DisplayDialog("Borrar línea", "¿Borrar la línea #" + i + "?", "Borrar", "Cancelar"))
            {
                rows.RemoveAt(i);
                dirty = true;
                GUI.FocusControl(null);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginChangeCheck();
        string nameCtrl = "name_" + i;
        GUI.SetNextControlName(nameCtrl);
        string newName = EditorGUILayout.TextField("Personaje", row.name ?? "");
        if (EditorGUI.EndChangeCheck())
        {
            row.name = newName;
            dirty = true;
        }

        // Botones de formato eliminados: la captura de selección no era
        // fiable en todas las versiones de Unity. Para dar formato se usan
        // los tags directamente en el TextArea de abajo y se ve el
        // resultado en la Vista previa.

        string textCtrl = "text_" + i;
        GUI.SetNextControlName(textCtrl);
        EditorGUI.BeginChangeCheck();
        bool readOnly = row.IsDecisionRow;
        string newText;
        if (readOnly)
        {
            EditorGUILayout.SelectableLabel(row.text ?? "", EditorStyles.textArea, GUILayout.MinHeight(36));
            newText = row.text;
        }
        else
        {
            newText = EditorGUILayout.TextArea(row.text ?? "", GUILayout.MinHeight(48));
        }
        if (EditorGUI.EndChangeCheck() && !readOnly)
        {
            row.text = newText;
            dirty = true;
        }

        EditorGUILayout.LabelField("Vista previa", EditorStyles.miniBoldLabel);
        EditorGUILayout.LabelField(row.text ?? "", previewStyle);

        EditorGUILayout.EndVertical();
    }

    private void LoadCsv(string name)
    {
        string abs = Path.GetFullPath(Path.Combine(
            Application.dataPath, "..", CsvRelativeFolder, name + ".csv"));
        if (!File.Exists(abs))
        {
            EditorUtility.DisplayDialog("No encontrado", "No existe el archivo " + abs, "OK");
            return;
        }

        string[] lines = File.ReadAllText(abs).Replace("\r", "").Split('\n');
        rows = new List<DialogueRow>();
        for (int i = 1; i < lines.Length; i++)
        {
            string raw = lines[i];
            if (string.IsNullOrWhiteSpace(raw)) continue;
            string[] parts = SplitCsvLine(raw);
            System.Array.Resize(ref parts, 6);
            rows.Add(new DialogueRow
            {
                name = parts[0],
                text = parts[1],
                isDecision = parts[2],
                options = parts[3],
                nextLines = parts[4],
                affinityChange = parts[5],
            });
        }

        currentFile = name;
        currentFilePath = abs;
        dirty = false;
        scrollRight = Vector2.zero;
        GUI.FocusControl(null);
    }

    private void SaveCurrent()
    {
        if (string.IsNullOrEmpty(currentFilePath)) return;

        var sb = new StringBuilder();
        sb.AppendLine("Name,Text,IsDecision,Options,NextLines,AffinityChange");
        foreach (var r in rows)
        {
            sb.Append(EscapeCsv(r.name));       sb.Append(',');
            sb.Append(EscapeCsv(r.text));       sb.Append(',');
            sb.Append(EscapeCsv(r.isDecision)); sb.Append(',');
            sb.Append(EscapeCsv(r.options));    sb.Append(',');
            sb.Append(EscapeCsv(r.nextLines));  sb.Append(',');
            sb.Append(EscapeCsv(r.affinityChange));
            sb.Append('\n');
        }

        File.WriteAllText(currentFilePath, sb.ToString());
        AssetDatabase.Refresh();
        dirty = false;

        ShowNotification(new GUIContent("Guardado " + currentFile + ".csv"));
    }

    private static string EscapeCsv(string s)
    {
        if (s == null) return "";
        bool needsQuotes = s.Contains(",") || s.Contains("\"") || s.Contains("\n");
        if (s.Contains("\""))
            s = s.Replace("\"", "\"\"");
        return needsQuotes ? ("\"" + s + "\"") : s;
    }

    private static string[] SplitCsvLine(string line)
    {
        var res = new List<string>();
        bool inQuotes = false;
        var cur = new StringBuilder();
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '\"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                {
                    cur.Append('\"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                res.Add(cur.ToString());
                cur.Length = 0;
            }
            else
            {
                cur.Append(c);
            }
        }
        res.Add(cur.ToString());
        return res.ToArray();
    }
}

/// <summary>
/// Mini-dialog para pedir una cadena al usuario.
/// </summary>
public class EditorInputDialog : EditorWindow
{
    private string description;
    private string text;
    private bool initFocus;
    private bool confirmed;
    private bool finished;

    public static string Show(string title, string description, string defaultText)
    {
        var w = CreateInstance<EditorInputDialog>();
        w.titleContent = new GUIContent(title);
        w.description = description;
        w.text = defaultText ?? "";
        w.position = new Rect(Screen.width / 2f, Screen.height / 2f, 380, 160);
        w.ShowModalUtility();
        if (!w.confirmed) return null;
        return w.text;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField(description, EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space();

        GUI.SetNextControlName("InputDialogField");
        text = EditorGUILayout.TextField(text);
        if (!initFocus)
        {
            GUI.FocusControl("InputDialogField");
            initFocus = true;
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Cancelar", GUILayout.Width(80)))
        {
            confirmed = false;
            finished = true;
            Close();
        }
        if (GUILayout.Button("Aceptar", GUILayout.Width(80)) ||
            (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
        {
            confirmed = true;
            finished = true;
            Close();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void OnDestroy()
    {
        if (!finished) confirmed = false;
    }
}
