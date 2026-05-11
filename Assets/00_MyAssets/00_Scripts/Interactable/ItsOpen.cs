using System.Collections;
using UnityEngine;
using TMPro;

public class ItsOpen : MonoBehaviour, Interactable
{
    bool firstInteract;
    public ObjectForeground objectForeground;
    public TMP_Text ChangeText;

    [Header("Comportamiento cuando la puerta sigue bloqueada")]
    [Tooltip("CSV de diálogo que se reproduce si el jugador intenta abrir la puerta antes de hablar con el barco. " +
             "Si se deja vacío y nadie más ha programado la cinemática, no se hace nada extra.")]
    public string lockedDialogueCSV = "Door";

    public void Start()
    {
        firstInteract = true;
    }

    public void ItsInteracting()
    {
        // Mientras la puerta no esté desbloqueada, no abrimos el puzzle.
        // En cambio, garantizamos que se dispare el flujo "Door.csv → cinemática
        // → diálogo Syn+Munin", aunque el ObjectForeground del barco no llegue
        // a activarse en este frame.
        if (!WorldState.DoorUnlocked)
        {
            if (!WorldState.BoatCinematicScheduled)
            {
                // Nadie más ha programado la cinemática: la disparamos nosotros
                // tras el diálogo de "puerta cerrada".
                if (!string.IsNullOrEmpty(lockedDialogueCSV))
                {
                    GameManager.instance.currentDialogueCSV = lockedDialogueCSV;
                    GameManager.instance.currentDialogueAction = "BoatFirstInteract";
                    WorldState.BoatCinematicScheduled = true;

                    UIManager.instance.ActivateUI("dialogue", true);
                    GameManager.instance.ChangeState(DataDefinitions.GameStates.Reading);
                }
            }
            // En cualquier caso, no seguimos al flujo del puzzle.
            return;
        }

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
