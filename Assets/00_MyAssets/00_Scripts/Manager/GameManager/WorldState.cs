// Estado global del mundo del juego.
// Centraliza flags simples que tienen que ser consultados por varios sistemas
// y que sobreviven a cambios de escena (mientras el dominio no se recargue).
public static class WorldState
{
    /// <summary>
    /// La puerta del barco solo se puede interactuar (abrir el puzzle) cuando
    /// este flag est� en true. Lo activa la acci�n de di�logo "UnlockDoor"
    /// despu�s de la cinem�tica de Syn y Munin.
    /// </summary>
    public static bool DoorUnlocked = false;

    /// <summary>
    /// Indica si la cinem�tica del barco ya ha sido programada en esta partida,
    /// para evitar que se dispare dos veces si dos interactuables se activan
    /// en el mismo frame (por ejemplo, el ObjectForeground del barco y el
    /// ItsOpen de la puerta al pulsar E en la zona compartida).
    /// </summary>
    public static bool BoatCinematicScheduled = false;

    /// <summary>
    /// Restablece el estado. �til para reiniciar la partida desde el men�.
    /// </summary>
    public static void ResetAll()
    {
        DoorUnlocked = false;
        BoatCinematicScheduled = false;
    }
}
