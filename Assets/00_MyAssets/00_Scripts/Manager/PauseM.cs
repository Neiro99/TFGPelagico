using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// DESCRIPCION:
/// 
/// </summary>

public class PauseM : MonoBehaviour
{
    // ***********************************************
    #region 1) Definicion de variables
    public static PauseM instancia;

    public GameObject[] menus;

    public Image[] starColor;
    #endregion
    // ***********************************************
    #region 2) Funciones de Unity
    private void Awake()
    {
        instancia = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    private void OnEnable()
    {
        GameManager.OnPause += OnPausado;
        GameManager.OnPlay += OnJugando;
    }

    private void OnDisable()
    {
        GameManager.OnPause -= OnPausado;
        GameManager.OnPlay -= OnJugando;
    }

    #endregion
    // ***********************************************
    #region 3) Funciones originales
    public void VisibilidadMenu(int _elemento, bool _estado)
    {
        menus[_elemento].SetActive(_estado);
    }

    #region 3.2) 
    void OnPausado()
    {
        VisibilidadMenu(0, true);
        VisibilidadMenu(1, true);
        Time.timeScale = 0f;
        CountStars();
    }

    void OnJugando()
    {
        VisibilidadMenu(0, false);
        VisibilidadMenu(1, false);
        Time.timeScale = 1f;

    }
    #endregion

    public void Boton_Continuar()
    {
        GameManager.instance.ChangeState(DataDefinitions.GameStates.Play);
    }

    public void Boton_Salir()
    {
        //GameManager.instance.CargaEscena_Y_CambioEstado(0, DataDefinitions.GameStates.MainMenu);
        Time.timeScale = 1f;
    }

    void CountStars()
    {
        int Stars = PlayerDataManager.Instance.stars;
        Color newColor = new Color(1, 1, 1, 1f);

        for (int i = 0; i < Stars; i++)
        {
            starColor[i].color = newColor;
        }
    }

    #endregion
    // ***********************************************
}
