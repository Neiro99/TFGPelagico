using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private float typingSpeed = 0.05f;
    private TextMeshProUGUI textMesh;
    private Coroutine typingCoroutine;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    public void StartTyping(string newText)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(ShowText(newText));
    }

    IEnumerator ShowText(string text)
    {
        textMesh.text = "";
        foreach (char c in text)
        {
            textMesh.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void SkipText(string fullText)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        textMesh.text = fullText;
    }
    public void ResetEffect()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (textMesh == null)
            textMesh = GetComponent<TextMeshProUGUI>();

        textMesh.text = "";
    }


}
