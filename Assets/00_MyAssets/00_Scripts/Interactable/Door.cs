using UnityEngine;

public class Door : MonoBehaviour
{
    public static Door Instance;
    public ObjectForeground objectForeground;

    private void Awake()
    {
        Instance = this;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (WorldState.DoorUnlocked)
        {
            // Una vez desbloqueada, todos los diálogos de la puerta (apuntes /
            // puzzle) los gestiona ItsOpen. Silenciamos el ObjectForeground del
            // barco mientras estemos en la zona de la puerta para que no se
            // dispare un Door.csv encima.
            objectForeground.caninteract = false;
        }
        else
        {
            // Estado inicial: el ObjectForeground del barco muestra Door.csv
            // (texto de "puerta cerrada") que arranca la cinemática.
            objectForeground.textToShow1 = "Door";
            objectForeground.textToShow2 = "Door";
            objectForeground.showImage = false;
            objectForeground.imageType = 2;
            objectForeground.spriteKey = "Door";
            objectForeground.caninteract = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Al salir de la zona de la puerta restauramos el texto al del barco
        // y reactivamos el ObjectForeground para que el barco siga siendo
        // interactuable (si el jugador vuelve a la zona del barco).
        objectForeground.textToShow1 = "Boat";
        objectForeground.textToShow2 = "Boat";
        objectForeground.imageType = 2;
        objectForeground.showImage = false;
        objectForeground.caninteract = true;
    }
}