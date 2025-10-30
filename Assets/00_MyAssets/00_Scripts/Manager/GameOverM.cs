using UnityEngine;

/// <summary>
/// DESCRIPCION:
/// 
/// </summary>

public class GameOverM : MonoBehaviour
{
    // ***********************************************
    #region 1) Definicion de variables
    public static GameOverM instancia;

    public GameObject menu;
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
        menu = transform.GetChild(0).gameObject;
    }

    private void OnEnable()
    {
        GameManager.OnGameOver += GameOver;
    }

    private void OnDisable()
    {
        GameManager.OnGameOver -= GameOver;
    }

    #endregion
    // ***********************************************
    #region 3) Funciones originales
    public void GameOver()
    {
       menu.SetActive(true);
    }

    public void MainMenu()
    {
        //GameManager.instance.CargaEscena_Y_CambioEstado(0, DataDefinitions.GameStates.MainMenu);
    }


    #endregion
    // ***********************************************
}
