using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menú de la escena final (05_Fin). Replica el patrón del MainMenuManager
/// pero solo con dos opciones: "Volver al menú principal" y "Salir del
/// juego". Al volver al menú resetea WorldState para que una nueva partida
/// arranque limpia.
///
/// Estructura esperada de cada botón en menuButtons (igual que el Main Menu):
///   Child 0: Image del marcador izquierdo (alpha 0 cuando no seleccionado).
///   Child 1: Texto del botón.
///   Child 2: Image del marcador derecho (alpha 0 cuando no seleccionado).
/// </summary>
public class FinMenuManager : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("GameObject raíz de la UI del menú de fin. Se activa con OnEnable, " +
             "no es estrictamente necesario tenerlo asignado.")]
    public GameObject menuUI;

    [Tooltip("Lista de los dos botones (en orden): " +
             "[0] = Volver al menú principal, [1] = Salir.")]
    public List<GameObject> menuButtons;

    public int menuIndex;

    [Header("Comportamiento del cambio de escena")]
    [Tooltip("BuildIndex del Main Menu. Tras eliminar 00_Introduccion: 0.")]
    public int mainMenuBuildIndex = 0;

    [Tooltip("Tipo de fade que se pasa al ChangeSceneManager para volver al menú.")]
    public string fadeType = "StandarFade";

    private void OnEnable()
    {
        menuIndex = 0;

        InputManager.MoveUpPressedEvent += MoveUp;
        InputManager.MoveDownPressedEvent += MoveDown;
        InputManager.SelectPressedEvent += SelectOption;
    }

    private void OnDisable()
    {
        InputManager.MoveUpPressedEvent -= MoveUp;
        InputManager.MoveDownPressedEvent -= MoveDown;
        InputManager.SelectPressedEvent -= SelectOption;
    }

    private void Start()
    {
        // Forzamos el estado MainMenu para que el InputManager active el
        // mapa UI (Select, MoveUp/Down). En esta escena no existe otro
        // MainMenuManager que capture OnMainMenu, así que es seguro
        // reusar el estado: la única consecuencia visible es que se nos
        // habilitan los inputs de navegación de menú.
        if (GameManager.instance != null)
            GameManager.instance.ChangeState(DataDefinitions.GameStates.MainMenu);

        // Inicializamos el selector en la primera opción.
        ResetMenuSelector();
        if (menuButtons != null && menuButtons.Count > 0)
            UpdateMenuSelection(1);
    }

    // -----------------------------------------------------------------------
    // Navegación
    // -----------------------------------------------------------------------

    private void MoveUp()   => NavigateMenu(true);
    private void MoveDown() => NavigateMenu(false);

    private void NavigateMenu(bool itsUp)
    {
        if (menuButtons == null || menuButtons.Count == 0) return;

        UpdateMenuSelection(0);
        ResetMenuSelector();

        if (itsUp)
        {
            menuIndex--;
            if (menuIndex < 0)
                menuIndex = menuButtons.Count - 1;
        }
        else
        {
            menuIndex++;
            if (menuIndex >= menuButtons.Count)
                menuIndex = 0;
        }

        UpdateMenuSelection(1);
    }

    private void UpdateMenuSelection(int changeColor)
    {
        if (menuButtons == null) return;
        if (menuIndex < 0 || menuIndex >= menuButtons.Count) return;
        var btn = menuButtons[menuIndex];
        if (btn == null) return;
        if (btn.transform.childCount < 3) return;

        var img0 = btn.transform.GetChild(0).GetComponent<Image>();
        var img2 = btn.transform.GetChild(2).GetComponent<Image>();
        if (img0 != null) img0.color = new Color(1f, 1f, 1f, changeColor);
        if (img2 != null) img2.color = new Color(1f, 1f, 1f, changeColor);
    }

    private void ResetMenuSelector()
    {
        if (menuButtons == null) return;
        for (int i = 0; i < menuButtons.Count; i++)
        {
            var btn = menuButtons[i];
            if (btn == null || btn.transform.childCount < 3) continue;
            var img0 = btn.transform.GetChild(0).GetComponent<Image>();
            var img2 = btn.transform.GetChild(2).GetComponent<Image>();
            if (img0 != null) img0.color = new Color(1f, 1f, 1f, 0);
            if (img2 != null) img2.color = new Color(1f, 1f, 1f, 0);
        }
    }

    // -----------------------------------------------------------------------
    // Acciones
    // -----------------------------------------------------------------------

    private void SelectOption()
    {
        switch (menuIndex)
        {
            case 0: BackToMainMenu(); break;
            case 1: QuitGame();       break;
        }
    }

    /// <summary>
    /// Vuelve al Main Menu con fade. Resetea WorldState para que una
    /// nueva partida arranque sin flags arrastrados (puerta desbloqueada,
    /// papeles encontrados, flores vistas, etc.).
    /// </summary>
    public void BackToMainMenu()
    {
        WorldState.ResetAll();

        if (ChangeSceneManager.instance == null || GameManager.instance == null)
            return;

        ChangeSceneManager.instance.nextSceneInsdex = mainMenuBuildIndex;
        ChangeSceneManager.instance.typeOfFade = fadeType;
        GameManager.instance.ChangeState(DataDefinitions.GameStates.ChangeScene);
    }

    /// <summary>
    /// Cierra el juego. En el Editor para Play; en build cierra el .exe.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Salir del juego");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
