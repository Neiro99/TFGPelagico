using UnityEngine;

/// <summary>
/// DESCRIPCION:
/// 
/// </summary>

[RequireComponent (typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    // ***********************************************
    #region 1) Definicion de variables
    public static MusicManager instancia;

    AudioSource audioSource;

    public AudioClip[] musicas;
    #endregion
    // ***********************************************
    #region 2) Funciones de Unity
    private void Awake()
    {
        instancia = this;
        audioSource = GetComponent<AudioSource> ();
        audioSource.playOnAwake = false;
    }

    #region GESTION EVENTOS COMO SUBSCRIPTOR
    private void OnEnable()
    {
        GameManager.OnMainMenu += OnMenuInicial;
        GameManager.OnPlay += OnJugando;
        GameManager.OnPause += OnPausado;
        GameManager.OnGameOver += OnJuegoFinalizado;
    }

    private void OnDisable()
    {
        GameManager.OnMainMenu -= OnMenuInicial;
        GameManager.OnPlay -= OnJugando;
        GameManager.OnPause -= OnPausado;
        GameManager.OnGameOver -= OnJuegoFinalizado;
    }

    #region FUNCIONES QUE SE VAN A EJECUTAR SI HAY SUBSCRIPCION Y SE INVOCA EL EVENTO
    void OnMenuInicial()
    {

        CambioVolumen(1f);
        ReproducirMusica(0, true);
    }

    void OnCuentaAtras()
    {

    }

    void OnJugando()
    {
        CambioVolumen(0.4f);
        ReproducirMusica(1, true);
        if (!audioSource.isPlaying) audioSource.UnPause();
    }

    void OnPausado()
    {
        if (audioSource.isPlaying) audioSource.Pause();
    }

    void OnJuegoFinalizado()
    {
        ReproducirMusica(3, true);
    }
    #endregion
    #endregion
    #endregion
    // ***********************************************
    #region 3) Funciones originales
    public void ReproducirMusica(int _indice, bool _enBucle)
    {
        if (EsPrimeraReproduccion() || EsMusicaDistinta(_indice))
        {
            audioSource.clip = musicas[_indice];
            audioSource.loop = _enBucle;

            audioSource.Play();
            //Debug.Log($"Reproduce pista: <color=cyan>{_indice}</color>");
        }
    }

    bool EsPrimeraReproduccion()
    {
        return audioSource.clip == null;
    }

    bool EsMusicaDistinta(int _indice)
    {
        return audioSource.clip != null && audioSource.clip != musicas[_indice]; 
    }

    public void CambioVolumen (float _volumen)
    {
        audioSource.volume = _volumen;
    }
    #endregion
// ***********************************************
}
