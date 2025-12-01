using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Collections;


public class DialogueUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TypewriterEffect typewriter;
    [SerializeField] private int UIPosition;

    [Header("Referencias externas")]
    [SerializeField] private DialogueDecisionManager decisionManager;

    private List<DialogueLine> dialogueLines;
    private int currentLine;
    private bool isTyping;
    private string csvFileName;

    void OnEnable()
    {
        csvFileName = GameManager.instance.currentDialogueCSV;
        dialogueLines = DialogueCSVLoader.LoadDialogue(csvFileName);
        ResetDialogue();
        ShowLine();
        InputManager.SelectPressedEvent += SkipText;
    }

    void OnDisable() => InputManager.SelectPressedEvent -= SkipText;

    void SkipText()
    {
        if (isTyping)
        {
            typewriter.SkipText(dialogueLines[currentLine].text);
            isTyping = false;
        }
        else NextLine();
    }

    void ShowLine()
    {
        if (currentLine >= dialogueLines.Count)
        {
            EndDialogue();
            return;
        }

        var line = dialogueLines[currentLine];

        if (line.isDecision)
        {
            dialogueText.text = "";
            decisionManager.ShowDecision(line, this);
            return;
        }

        nameText.text = line.Name;
        StartCoroutine(PlayTypewriter(line.text));
    }

    IEnumerator PlayTypewriter(string text)
    {
        isTyping = true;
        typewriter.StartTyping(text);
        yield return new WaitUntil(() => dialogueText.text == text);
        isTyping = false;
    }

    public void JumpToLine(int lineIndex)
    {
        currentLine = lineIndex;
        ShowLine();
    }

    void NextLine()
    {
        var line = dialogueLines[currentLine];
        if (line.nextLine >= 0)
            currentLine = line.nextLine;
        else
            currentLine++;

        ShowLine();
    }


    void EndDialogue()
    {
        UIManager.instance.ResetUI();
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
