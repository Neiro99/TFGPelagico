using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueSpeakerStyler : MonoBehaviour
{
    [Serializable]
    public class SpeakerStyle
    {
        public string speakerName;      // "Syn", "Aster", etc (tal cual en el CSV)
        public Color nameColor = Color.white;
        public Color textColor = Color.white;
    }

    [Header("Referencias TMP (los de tu UI)")]
    [SerializeField] private TextMeshProUGUI textNameTMP;      // TextName (TMP)
    [SerializeField] private TextMeshProUGUI textDialogueTMP;  // TextDialogue (TMP)

    [Header("Estilos por personaje (ampliable)")]
    [SerializeField] private List<SpeakerStyle> styles = new();

    [Header("Default (si no existe el personaje)")]
    [SerializeField] private Color defaultNameColor = Color.white;
    [SerializeField] private Color defaultTextColor = Color.white;

    private Dictionary<string, SpeakerStyle> styleMap;

    private void Awake()
    {
        styleMap = new Dictionary<string, SpeakerStyle>(StringComparer.OrdinalIgnoreCase);

        foreach (var s in styles)
        {
            if (s == null) continue;
            if (string.IsNullOrWhiteSpace(s.speakerName)) continue;

            // Guardamos sin espacios y sin importar mayúsculas/minúsculas
            styleMap[s.speakerName.Trim()] = s;
        }
    }

    /// Llamas a esto cada vez que vas a mostrar una línea del CSV.
    public void ApplyStyle(string speakerName)
    {
        // Default si viene vacío
        if (string.IsNullOrWhiteSpace(speakerName))
        {
            SetColors(defaultNameColor, defaultTextColor);
            return;
        }

        var key = speakerName.Trim();

        // Si existe estilo para el personaje, úsalo; si no, default
        if (styleMap != null && styleMap.TryGetValue(key, out var style))
            SetColors(style.nameColor, style.textColor);
        else
            SetColors(defaultNameColor, defaultTextColor);
    }

    private void SetColors(Color nameCol, Color textCol)
    {
        if (textNameTMP != null) textNameTMP.color = nameCol;
        if (textDialogueTMP != null) textDialogueTMP.color = textCol;
    }
}
