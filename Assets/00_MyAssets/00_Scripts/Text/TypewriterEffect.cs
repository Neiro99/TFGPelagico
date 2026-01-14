using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TypewriterEffect : MonoBehaviour
{
    public bool IsTyping { get; private set; }

    [SerializeField] private float typingSpeed = 0.05f;

    private TextMeshProUGUI textMesh;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    public void StartTyping(string newText)
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshProUGUI>();

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(newText ?? ""));
    }

    private IEnumerator TypeText(string fullText)
    {
        IsTyping = true;

        // Poner texto completo y revelar caracteres
        textMesh.text = fullText;
        textMesh.maxVisibleCharacters = 0;

        // MUY IMPORTANTE: a veces TMP necesita un frame para calcular textInfo
        textMesh.ForceMeshUpdate(true, true);
        yield return null;
        textMesh.ForceMeshUpdate(true, true);

        int totalVisible = textMesh.textInfo.characterCount;

        // Fallback por si TMP sigue devolviendo 0 (raro, pero pasa)
        if (totalVisible <= 0 && !string.IsNullOrEmpty(fullText))
            totalVisible = fullText.Length;

        for (int i = 0; i <= totalVisible; i++)
        {
            textMesh.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Asegurar que queda todo visible al final
        textMesh.maxVisibleCharacters = 999999;

        IsTyping = false;
        typingCoroutine = null;
    }

    public void SkipText(string fullText)
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshProUGUI>();

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        textMesh.text = fullText ?? "";
        textMesh.ForceMeshUpdate(true, true);
        textMesh.maxVisibleCharacters = 999999;

        IsTyping = false;
        typingCoroutine = null;
    }

    public void ResetEffect()
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshProUGUI>();

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        textMesh.text = "";
        textMesh.maxVisibleCharacters = 999999;

        IsTyping = false;
        typingCoroutine = null;
    }
}
