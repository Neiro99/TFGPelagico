using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TypewriterEffect typewriter;
    [SerializeField] private int UIPosition;

    [Header("Datos del diálogo")]
    [SerializeField] private List<DialogueLine> dialogueLines = new();

    private int currentLine = 0;
    private bool isTyping = false;

    void Start()
    {

    }

    void OnEnable()
    {
        ResetDialogue();
        dialogueText.text = "";
        nameText.text = "";

        ShowLine();
        InputManager.SelectPressedEvent += SkipText; 
    }
    void OnDisable() 
    {
        InputManager.SelectPressedEvent -= SkipText; 
    }
    void SkipText()
    {
        if (isTyping)
        {
            typewriter.SkipText(dialogueLines[currentLine].text);
            isTyping = false;
        }
        else
        {
            NextLine();
        }
    }

    void ShowLine()
    {
        if (currentLine >= dialogueLines.Count)
        {
            EndDialogue();
            return;
        }

        nameText.text = dialogueLines[currentLine].Name;
        StartCoroutine(PlayTypewriter(dialogueLines[currentLine].text));
    }

    IEnumerator PlayTypewriter(string text)
    {
        isTyping = true;
        typewriter.StartTyping(text);
        yield return new WaitUntil(() => dialogueText.text == text);
        isTyping = false;
    }

    void NextLine()
    {
        currentLine++;
        ShowLine();
    }

    void EndDialogue()
    {
        UIManager.Instance.DesActivateUI(UIPosition);
        GameManager.instance.ChangeState(DataDefinitions.GameStates.Play);
    }
    void ResetDialogue()
    {
        currentLine = 0;
        isTyping = false;
        dialogueText.text = "";
        nameText.text = "";
        typewriter.ResetEffect(); 
    }
}

[System.Serializable]
public class DialogueLine
{
    public string Name;
    [TextArea(2, 4)] public string text;
}
