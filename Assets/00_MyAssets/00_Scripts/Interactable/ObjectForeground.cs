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

    [Header("Acción opcional en la primera interacción")]
    [Tooltip("Si se rellena (por ejemplo \"BoatFirstInteract\"), esta cadena se asigna a " +
             "GameManager.currentDialogueAction la primera vez que se interactúa con el objeto. " +
             "Sirve para encadenar una cinemática después del diálogo inicial. " +
             "Solo se aplica si la puerta sigue bloqueada y no hay otra cinemática programada.")]
    public string firstInteractAction;

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

            // Si está configurada una acción de primera interacción y nadie más ha
            // programado ya la cinemática este frame, la dejamos lista para
            // dispararla al terminar el diálogo.
            if (!string.IsNullOrEmpty(firstInteractAction)
                && !WorldState.DoorUnlocked
                && !WorldState.BoatCinematicScheduled)
            {
                GameManager.instance.currentDialogueAction = firstInteractAction;
                WorldState.BoatCinematicScheduled = true;
            }

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
