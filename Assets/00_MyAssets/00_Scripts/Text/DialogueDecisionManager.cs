using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueDecisionManager : MonoBehaviour
{
    [SerializeField] private GameObject decisionPanel;
    [SerializeField] private List<TextMeshProUGUI> optionTexts;

    private DialogueLine currentDecisionLine;
    private int decisionIndex;
    private DialogueUIManager dialogueManager;

    public void ShowDecision(DialogueLine line, DialogueUIManager manager)
    {
        currentDecisionLine = line;
        dialogueManager = manager;

        decisionPanel.SetActive(true);

        for (int i = 0; i < optionTexts.Count; i++)
        {
            if (i < line.options.Count)
            {
                optionTexts[i].gameObject.SetActive(true);
                optionTexts[i].text = line.options[i];
            }
            else optionTexts[i].gameObject.SetActive(false);
        }

        decisionIndex = 0;
        UpdateDecisionSelection();

        InputManager.MoveUpPressedEvent += MoveUp;
        InputManager.MoveDownPressedEvent += MoveDown;
        InputManager.SelectPressedEvent += Confirm;
    }
    void MoveUp()
    {
        decisionIndex = (decisionIndex - 1 + currentDecisionLine.options.Count) % currentDecisionLine.options.Count;
        UpdateDecisionSelection();
    }

    void MoveDown()
    {
        decisionIndex = (decisionIndex + 1) % currentDecisionLine.options.Count;
        UpdateDecisionSelection();
    }

    void Confirm()
    {
        decisionPanel.SetActive(false);

        int nextIndex = currentDecisionLine.nextLineIndex[decisionIndex];
        int affinityDelta = 0;

        if (currentDecisionLine.affinityChange != null &&
        decisionIndex < currentDecisionLine.affinityChange.Count)
        {
            affinityDelta = currentDecisionLine.affinityChange[decisionIndex];
            PlayerDataManager.Instance.ModifyAffinity(affinityDelta);
        }

        dialogueManager.JumpToLine(nextIndex);

        InputManager.MoveUpPressedEvent -= MoveUp;
        InputManager.MoveDownPressedEvent -= MoveDown;
        InputManager.SelectPressedEvent -= Confirm;
    }

    void UpdateDecisionSelection()
    {
        for (int i = 0; i < optionTexts.Count; i++)
        {
            if (!optionTexts[i].gameObject.activeSelf) continue;
            optionTexts[i].color = (i == decisionIndex) ? Color.yellow : Color.white;
        }
    }
}
