using System.Collections;
using UnityEngine;
using TMPro;

public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private float typingSpeed = 0.05f;
    private TextMeshProUGUI textMesh;
    private string fullText;
    private Coroutine typingCoroutine;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        fullText = textMesh.text;
        textMesh.text = "";
    }

    void OnEnable()
    {
        StartTyping();
        InputManager.SelectPressedEvent += SkipText;
    }

   void OnDisable()
   {
        InputManager.SelectPressedEvent -= SkipText;
   }

    public void StartTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(ShowText());
    }

    IEnumerator ShowText()
    {
        textMesh.text = "";

        for (int i = 0; i < fullText.Length; i++)
        {
            textMesh.text += fullText[i];
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void SkipText()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

       if(textMesh.text == fullText)
       {
            UIManager.Instance.DesActivateUI(1);
            GameManager.instance.ChangeState(DataDefinitions.GameStates.Play);
       }

        textMesh.text = fullText;
    }
}
