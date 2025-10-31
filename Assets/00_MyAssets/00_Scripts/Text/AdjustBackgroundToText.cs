using UnityEngine;
using TMPro;

[ExecuteAlways]
public class TextAndBgAutoSize : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textTMP;     // el texto
    [SerializeField] private RectTransform background;    // la imagen de fondo
    [SerializeField] private float padding = 20f;         // margen lateral para el fondo

    private void Update()
    {
        if (textTMP == null) return;

        // obligamos a TMP a calcular el tamaño
        textTMP.ForceMeshUpdate();

        // ancho que el texto NECESITA para una línea
        float textWidth = textTMP.preferredWidth;
        float textHeight = textTMP.preferredHeight;

        // 1) agrandamos el rect del TEXTO
        var textRect = textTMP.rectTransform;
        textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
        textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);

        // 2) si hay fondo, lo igualamos + padding
        if (background != null)
        {
            background.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth + padding);
            background.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight + padding * 0.5f);
        }
    }
}
