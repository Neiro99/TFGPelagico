using UnityEngine;

/// <summary>
/// DESCRIPCION:
/// 
/// </summary>

public class MainMenuM : MonoBehaviour
{
    // ***********************************************
    #region 1) Definicion de variables
    public static MainMenuM instancia;

    public GameObject[] menus;
    #endregion
        // ***********************************************
    #region 2) Funciones de Unity
    private void Awake()
    {
        instancia = this;
    }

    private void OnEnable()
    {
        GameManager.OnMainMenu += OnMenuIncial;
    }

    private void OnDisable()
    {
        GameManager.OnMainMenu -= OnMenuIncial;
    }
    #endregion
    // ***********************************************
    #region 3) Funciones originales
    void OnMenuIncial()
    {
        VisibilidadMenu(0, true);
        VisibilidadMenu(1, false);
    }

    public void VisibilidadMenu (int _indice, bool estado)
    {
        menus[_indice].SetActive(estado);
    }

    #region 3) Funcionalidad Botones
    public void Boton_Jugar()
    {
        SoundManager.instancia.Reproducir_SonidoInterfaz(0);
        //GameManager.instance.CargaEscena_Y_CambioEstado(1, DataDefinitions.GameStates.Play);
    }

    public void Boton_Salir()
    {
        SoundManager.instancia.Reproducir_SonidoInterfaz(1);
        VisibilidadMenu(0, false);
        VisibilidadMenu(1, true);
    }

    public void Boton_SalirConfirmar()
    {
        Application.Quit();

    }

    public void Boton_SalirCancelar()
    {
        SoundManager.instancia.Reproducir_SonidoInterfaz(1);

        VisibilidadMenu(0, true);
        VisibilidadMenu(1, false);
    }

    #endregion
    #endregion
    // ***********************************************
}
