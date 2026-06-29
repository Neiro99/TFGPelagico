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
    [SerializeField] private DialogueSpeakerStyler speakerStyler;


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

        StartCoroutine(ApplyStyleNextFrame(line.Name));

        StartCoroutine(PlayTypewriter(line.text));
    }

    IEnumerator ApplyStyleNextFrame(string speakerName)
    {
        yield return null; // 1 frame
        if (speakerStyler != null)
            speakerStyler.ApplyStyle(speakerName);
    }



    IEnumerator PlayTypewriter(string text)
    {
        isTyping = true;
        typewriter.StartTyping(text);

        yield return new WaitUntil(() => !typewriter.IsTyping);

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
        string action = GameManager.instance.currentDialogueAction;

        switch (action)
        {
            case "FinishTalkSym":
                    SynAction.Instance.FinishTalkSym();
                break;

            case "OpenDoor":
                   ChangeWorldStatus.Instance.StartChanges();
                break;

            case "BoatFirstInteract":
                    // Primer contacto con el barco (o intento de abrir la puerta
                    // cerrada): arranca la cinemática en la que Syn y Munin
                    // caminan hasta el barco y, al llegar, lanzan su diálogo.
                    GameManager.instance.currentDialogueAction = "";
                    UIManager.instance.ResetUI();
                    BoatCinematicAction.Instance.StartCinematic();
                    return; // la cinemática se encarga de cambiar el estado del juego

            case "UnlockDoor":
                    // Fin del diálogo de Syn y Munin: la puerta queda
                    // desbloqueada y a partir de aquí ItsOpen puede abrir el puzzle.
                    WorldState.DoorUnlocked = true;
                break;

            case "FindPapers":
                    // Fin del diálogo de la mesa de Torpere. Solo damos por
                    // "encontrados" los apuntes si la 2ª aparición de Syn
                    // (la cinemática Syn-Munin del barco) ya ha ocurrido
                    // — esa cinemática es la que activa DoorUnlocked.
                    // Si el jugador interactúa con la mesa ANTES de esa
                    // cinemática, ve el texto pero el progreso de la
                    // historia no se considera "leer los papeles" todavía.
                    if (WorldState.DoorUnlocked)
                        WorldState.PapersFound = true;
                break;

            case "InteractFlowers":
                    // Fin del diálogo al interactuar con las flores en la
                    // escena final. A partir de aquí el siguiente paso del
                    // flujo es que el jugador abra el diario y vea la
                    // página 4: cuando lo cierre, el DiaryManager se
                    // encarga de la transición a la escena Fin.
                    WorldState.FlowersInteracted = true;
                break;

            case "EndPuzzleSequence":
                    // Conversación posterior al puzzle terminada. Antes de cambiar
                    // de escena se reproduce una cinemática a pantalla completa.
                    // Cuando termine, PostPuzzleCinematic se encarga del cambio
                    // de escena con el fade configurado (SwichFade → escena 4).
                    GameManager.instance.currentDialogueAction = "";
                    UIManager.instance.ResetUI();
                    PostPuzzleCinematic.Instance.PlayCinematic();
                    return; // la cinemática se encarga del estado a partir de aquí
        }

        GameManager.instance.currentDialogueAction = "";

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
