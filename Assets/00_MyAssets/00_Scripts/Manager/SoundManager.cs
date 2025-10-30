using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

/// <summary>
/// DESCRIPCION:
/// 
/// </summary>

[RequireComponent (typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    // ***********************************************
    #region 1) Definicion de variables
    public static SoundManager instancia;
    AudioSource audioSource;

    public AudioClip[] sonidoInterfaz;
    public AudioClip coinSound;
    public AudioClip lifeSound;
    public AudioClip starSound;
    public AudioClip keySound;

    #endregion
    // ***********************************************
    #region 2) Funciones de Unity
    private void Awake()
    {
        instancia = this;
        audioSource = GetComponent<AudioSource>();
    }
    #endregion
// ***********************************************
    #region 3) Funciones originales


    public void Reproducir_SonidoInterfaz(int _indice)
    {
        audioSource.PlayOneShot(sonidoInterfaz[_indice]);
    }

    public void ItemsSound(string _item)
    {
        if (_item == "Coin") audioSource.PlayOneShot(coinSound);
        if (_item == "Key") audioSource.PlayOneShot(keySound);
        if (_item == "Heart") audioSource.PlayOneShot(lifeSound);
        if (_item == "Star") audioSource.PlayOneShot(starSound);
    }

    #endregion
// ***********************************************
}
