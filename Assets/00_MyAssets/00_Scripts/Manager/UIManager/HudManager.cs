using UnityEngine;
using TMPro;
using UnityEngine.UI;



/// <summary>
/// DESCRIPCION:
/// 
/// </summary>

public class HudManager : MonoBehaviour
{
// ***********************************************
    #region 1) Definicion de variables
    public static HudManager instancia;

    public GameObject[] elementosHud;

    [Header("CONTENIDO => HUD")]
    public GameObject[] balasIconos;

    #endregion
    // ***********************************************
    #region 2) Funciones de Unity
    private void Awake() {  
        instancia = this;
    }

    private void OnEnable()
    {
        GameManager.OnPlay += OnJugando;
        GameManager.OnPause += OnJuegoPausado;
        GameManager.OnGameOver += OnJuegoFinalizado;
    }

    private void OnDisable()
    {
        GameManager.OnPlay -= OnJugando;
        GameManager.OnPause -= OnJuegoPausado;
        GameManager.OnGameOver -= OnJuegoFinalizado;
    }


    void OnJugando()
    {
        VisibilidadElementosHub(0, true);   // activo mirilla 
        VisibilidadElementosHub(1, true);   // activo hud general    
    }    

    void OnJuegoPausado()
    {
        VisibilidadElementosHub(0, false);
    }    

    void OnJuegoFinalizado()
    {
        VisibilidadElementosHub(0, false);   // desactivo mirilla 
        VisibilidadElementosHub(1, false);   // desactivo hud general    
    }
    #endregion
    // ***********************************************
    #region 3) Funciones originales
    public void VisibilidadElementosHub (int _elemento, bool _estado)
    {
        elementosHud[_elemento].SetActive(_estado);
    }

    #endregion
    // ***********************************************
}
