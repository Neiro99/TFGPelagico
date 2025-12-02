using System.Collections;
using UnityEngine;

public class ItsOpen : MonoBehaviour, Interactable
{
    bool firstInteract;
    public ObjectForeground objectForeground;
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
