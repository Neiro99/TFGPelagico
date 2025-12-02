using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ObjectForeground : MonoBehaviour, Interactable
{
    bool firstInteract;
    public bool showImage;

    public string textToShow1;
    public string textToShow2;

    public int imageType; 
    public string spriteKey;

    public bool caninteract;

    public void Start()
    {
        caninteract = true;
        firstInteract = true;
    }
    public void ItsInteracting()
    {
        if (!caninteract) return;

        if (firstInteract)
        {
            GameManager.instance.currentDialogueCSV = textToShow1;
            firstInteract = false;
        }
        else GameManager.instance.currentDialogueCSV = textToShow2;

        UIManager.instance.ActivateUI("dialogue", true);

        if (showImage)
        {
            UIManager.instance.ActivateUI("background", true);
            UIManager.instance.ActivateUI("objectView", true);
            UIImageManager.instance.ShowObjectImage(imageType, spriteKey);
        }

        GameManager.instance.ChangeState(DataDefinitions.GameStates.Reading);
        transform.GetChild(0).gameObject.SetActive(false);
    }
}
