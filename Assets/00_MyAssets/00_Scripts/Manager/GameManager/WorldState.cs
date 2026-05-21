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
    /// El jugador ya ha le�do los apuntes/dibujos en la mesa de Torpere y por
    /// tanto puede intentar abrir la puerta del barco (acceder al puzzle).
    /// </summary>
    public static bool PapersFound = false;

    /// <summary>
    /// La cinem�tica posterior al puzzle (PostPuzzleCinematic) ya se ha visto.
    /// Sirve para desbloquear contenido que no debe estar disponible antes,
    /// como la 4� p�gina del diario.
    /// </summary>
    public static bool PostPuzzleCinematicSeen = false;

    /// <summary>
    /// El jugador ha interactuado con las flores de la mesa en la penultima
    /// escena. Es el primer paso del flujo final: abrir el diario, ir a la
    /// pagina 4 y cerrarlo dispara el cambio a la escena de Fin.
    /// </summary>
    public static bool FlowersInteracted = false;

    /// <summary>
    /// El jugador ha llegado a ver la pagina 4 (indice 3) del diario al
    /// menos una vez. Segundo paso del flujo final.
    /// </summary>
    public static bool Page4Seen = false;

    /// <summary>
    /// Se pone a true en el momento en que se dispara la transicion a la
    /// escena Fin para evitar que se dispare mas de una vez si el jugador
    /// vuelve a abrir y cerrar el diario.
    /// </summary>
    public static bool FinalSceneTriggered = false;

    /// <summary>
    /// Restablece el estado. �til para reiniciar la partida desde el men�.
    /// </summary>
    public static void ResetAll()
    {
        DoorUnlocked = false;
        BoatCinematicScheduled = false;
        PapersFound = false;
        PostPuzzleCinematicSeen = false;
        FlowersInteracted = false;
        Page4Seen = false;
        FinalSceneTriggered = false;
    }
}
