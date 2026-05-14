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
    [Tooltip("Si se rellena, esta cadena se asigna a GameManager.currentDialogueAction la " +
             "primera vez que se interactúa con el objeto, para encadenar una cinemática " +
             "o un cambio de estado después del diálogo inicial. " +
             "Ejemplos: \"BoatFirstInteract\" (barco), \"FindPapers\" (mesa de Torpere). " +
             "Cada acción comprueba sus propias condiciones en DialogueUIManager.EndDialogue.")]
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

            // Si hay acción configurada se la asignamos. La acción se procesa en
            // DialogueUIManager.EndDialogue y es allí donde cada caso comprueba
            // si todavía toca ejecutarse (por ejemplo BoatFirstInteract solo
            // dispara la cinemática si la puerta sigue bloqueada).
            if (!string.IsNullOrEmpty(firstInteractAction))
                GameManager.instance.currentDialogueAction = firstInteractAction;

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
