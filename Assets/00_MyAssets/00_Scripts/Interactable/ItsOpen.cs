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

    [Header("Comportamiento cuando aún faltan los apuntes (Syn ya habló pero no se ha visitado la mesa de Torpere)")]
    [Tooltip("CSV de diálogo que se muestra al intentar abrir la puerta antes de leer los apuntes.")]
    public string needsPapersDialogueCSV = "DoorNeedsPapers";

    [Header("Comportamiento la primera vez que se intenta abrir con apuntes")]
    [Tooltip("CSV de diálogo que se muestra justo antes de entrar al puzzle por primera vez.")]
    public string withPapersDialogueCSV = "DoorWithPapers";

    public void Start()
    {
        firstInteract = true;
    }

    public void ItsInteracting()
    {
        // 1) Puerta todavía bloqueada (Syn y Munin no han hablado con Aster).
        //    Disparamos el flujo "Door.csv → cinemática → BoatFirstTime".
        if (!WorldState.DoorUnlocked)
        {
            if (!WorldState.BoatCinematicScheduled)
            {
                if (!string.IsNullOrEmpty(lockedDialogueCSV))
                {
                    GameManager.instance.currentDialogueCSV = lockedDialogueCSV;
                    GameManager.instance.currentDialogueAction = "BoatFirstInteract";
                    WorldState.BoatCinematicScheduled = true;

                    UIManager.instance.ActivateUI("dialogue", true);
                    GameManager.instance.ChangeState(DataDefinitions.GameStates.Reading);
                }
            }
            return;
        }

        // 2) Puerta desbloqueada pero todavía no hemos leído los apuntes en la
        //    mesa de Torpere → mostramos solo el diálogo "Necesito encontrar
        //    los apuntes...". No abrimos el puzzle ni avanzamos el flujo.
        if (!WorldState.PapersFound)
        {
            if (!string.IsNullOrEmpty(needsPapersDialogueCSV))
            {
                GameManager.instance.currentDialogueCSV = needsPapersDialogueCSV;
                GameManager.instance.currentDialogueAction = "";
                UIManager.instance.ActivateUI("dialogue", true);
                GameManager.instance.ChangeState(DataDefinitions.GameStates.Reading);
            }
            return;
        }

        // 3) Puerta desbloqueada y apuntes encontrados.
        if (firstInteract)
        {
            // Primera vez con apuntes: mostramos "A ver cómo narices...", y al
            // terminar el diálogo la acción OpenDoor encadena el puzzle.
            firstInteract = false;
            StartCoroutine(WaitAndEnableInteraction());

            if (!string.IsNullOrEmpty(withPapersDialogueCSV))
                GameManager.instance.currentDialogueCSV = withPapersDialogueCSV;
            GameManager.instance.currentDialogueAction = "OpenDoor";

            UIManager.instance.ActivateUI("dialogue", true);
            GameManager.instance.ChangeState(DataDefinitions.GameStates.Reading);
        }
        else
        {
            // Interacciones posteriores: directamente al puzzle.
            GameManager.instance.ChangeState(DataDefinitions.GameStates.Puzzle);
        }
    }

    IEnumerator WaitAndEnableInteraction()
    {
        yield return new WaitForSeconds(0.1f);
        objectForeground.caninteract = false;
    }
}
