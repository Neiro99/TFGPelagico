using System.Collections;
using UnityEngine;
using TMPro;

public class ItsOpen : MonoBehaviour, Interactable
{
    bool firstInteract;
    public ObjectForeground objectForeground;
    public TMP_Text ChangeText;

    public void Start()
    {
        firstInteract = true;
    }

    public void ItsInteracting()
    {
        if (firstInteract)
        {
            firstInteract = false;
            StartCoroutine(WaitAndEnableInteraction());
            GameManager.instance.currentDialogueAction = "OpenDoor";
        }
        else
        {
                UIManager.instance.ResetUI();
                UIManager.instance.ActivateUI("background", true);
                UIManager.instance.ActivateUI("puzzle", true);
        }
    }

    IEnumerator WaitAndEnableInteraction()
    {
        yield return new WaitForSeconds(0.1f);
        objectForeground.caninteract = false;
    }
}
