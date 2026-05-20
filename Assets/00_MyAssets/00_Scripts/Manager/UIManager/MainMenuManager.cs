using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject mainMenuUI;
    public List<GameObject> menuButtons;
    public int menuIndex;

    public GameObject credits;
    [Tooltip("Panel de la pantalla de Controles. Se comporta exactamente igual " +
             "que el panel de Créditos: se abre desde el botón 'Controles' del " +
             "menú y se cierra con Confirmar (Intro/Espacio).")]
    public GameObject controls;
    public GameObject mainMenu;
    public bool mainMenuWorks;

    public GameObject settingsMenu;
    public SettingsMenuManager settingsMgr;
    public bool settingsWorks;



    private void OnEnable()
    {
        menuIndex = 0;
        settingsWorks = true;
        mainMenuWorks = true;
        GameManager.OnMainMenu += ActivateMenu;

        InputManager.MoveUpPressedEvent += MoveUp;
        InputManager.MoveDownPressedEvent += MoveDown;
        InputManager.SelectPressedEvent += SelectOption;
        InputManager.BackPressedEvent += OnBackPressed;
    }

    private void OnDisable()
    {
        GameManager.OnMainMenu -= ActivateMenu;

        InputManager.MoveUpPressedEvent -= MoveUp;
        InputManager.MoveDownPressedEvent -= MoveDown;
        InputManager.SelectPressedEvent -= SelectOption;
        InputManager.BackPressedEvent -= OnBackPressed;
    }

    /// <summary>
    /// Handler para la tecla "Atrás" (Esc). Cierra el sub-panel actual
    /// (Créditos, Controles o Configuración) sin afectar al estado del
    /// juego, igual que hace Confirmar sobre la opción "Volver".
    /// Si no hay sub-panel abierto, no hace nada (el jugador no debería
    /// poder "salir" del menú principal con Esc).
    /// </summary>
    private void OnBackPressed()
    {
        // Caso 1: Créditos o Controles abierto -> cerrar como en SelectOption.
        if (!mainMenuWorks)
        {
            mainMenuWorks = true;
            mainMenu.SetActive(true);
            credits.SetActive(false);
            if (controls != null) controls.SetActive(false);
            menuIndex = 1;
            NavigateMenu(true);
            return;
        }

        // Caso 2: Configuración abierto -> cerrarlo igual que pulsar
        // "Volver" dentro del panel. Centralizamos el cierre aquí en vez
        // de en SettingsMenuManager para que el evento Back solo tenga
        // efecto en el contexto del Main Menu y no interfiera con el
        // panel de Configuración dentro de la Pausa del juego, donde
        // Esc ya está bindeado a "salir del Pause" en GameManager.
        if (!settingsWorks)
        {
            CloseSettingsFromSettingsMenu();
            return;
        }

        // Caso 3: estamos en el menú raíz -> Esc no hace nada.
    }

    private void MoveUp()
    {
        // Si hay un sub-panel abierto (Créditos / Controles), el menú
        // principal no debe navegarse "por detrás": dejamos que sea el
        // propio sub-panel (p. ej. el carrusel de Controles) quien
        // capture la pulsación. Cuando el jugador cierra el sub-panel,
        // mainMenuWorks vuelve a true y la navegación normal se reanuda.
        if (!mainMenuWorks) return;
        NavigateMenu(true);
    }

    private void MoveDown()
    {
        if (!mainMenuWorks) return;
        NavigateMenu(false);
    }

    private void ActivateMenu()
    {
        mainMenuUI.SetActive(true);
    }
    private void NavigateMenu(bool itsUp)
    {
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
        menuButtons[menuIndex].transform.GetChild(0).GetComponent<Image>().color = new Color(1f, 1f, 1f, changeColor);
        menuButtons[menuIndex].transform.GetChild(2).GetComponent<Image>().color = new Color(1f, 1f, 1f, changeColor);
    }
    private void ResetMenuSelector()
    {
        int i;
        for (i = 0; i < menuButtons.Count; i++)
        {
            menuButtons[i].transform.GetChild(0).GetComponent<Image>().color = new Color(1f, 1f, 1f, 0);
            menuButtons[i].transform.GetChild(2).GetComponent<Image>().color = new Color(1f, 1f, 1f, 0);
        }
    }

    private void SelectOption()
    {
        if (!mainMenuWorks)
        {
            mainMenuWorks = true;
            mainMenu.SetActive(true);
            credits.SetActive(false);
            // El mismo gesto de "Confirmar" cierra el panel de Controles si
            // está abierto. Aceptamos null por si la referencia no se ha
            // asignado todavía en el Inspector.
            if (controls != null) controls.SetActive(false);
            menuIndex = 1;
            NavigateMenu(true);
            return;
        }

        if (!settingsWorks)
            return;

        switch (menuIndex)
        {
            case 0: StartGame(); break;
            case 1: OpenSettings(); break;
            case 2: OpenExtras(); break;
            case 3: OpenCredits(); break;
            case 4: QuitGame(); break;
        }
    }


    public void StartGame()
    {
        // En vez de saltar directamente al cambio de escena, pedimos al
        // LoadingScreenManager que muestre la pantalla de controles ("Controls").
        // Cuando termine, será él quien dispare el cambio a la escena 2.
        // Tras eliminar la escena 00_Introduccion los buildIndex se desplazan:
        // MainMenu pasa a 0, OuterWorld (antes 2) pasa a 1.
        if (LoadingScreenManager.instance != null)
        {
            LoadingScreenManager.instance.StartLoading("Controls", 1);
        }
        else
        {
            // Fallback: si por lo que sea no hay LoadingScreenManager (escena
            // sin la jerarquía persistente), mantenemos el comportamiento antiguo.
            ChangeSceneManager.instance.nextSceneInsdex = 1;
            ChangeSceneManager.instance.typeOfFade = "StandarFade";
            GameManager.instance.ChangeState(DataDefinitions.GameStates.ChangeScene);
        }
    }

    public void OpenSettings()
    {
        settingsWorks = false;

        mainMenu.SetActive(false);
        credits.SetActive(false);
        if (controls != null) controls.SetActive(false);

        settingsMenu.SetActive(true);
        settingsMgr.Open();
    }

    public void CloseSettingsFromSettingsMenu()
    {
        settingsWorks = true;

        settingsMgr.Close();
        settingsMenu.SetActive(false);

        mainMenu.SetActive(true);

        menuIndex = 1;
        ResetMenuSelector();
        UpdateMenuSelection(1);
    }


    /// <summary>
    /// Botón "Controles" del Main Menu. Funciona como <see cref="OpenCredits"/>:
    /// abre un sub-panel dentro del propio menú (sin cambio de escena). El
    /// panel se cierra automáticamente al pulsar Confirmar (gestionado en
    /// <see cref="SelectOption"/> a través del flag <see cref="mainMenuWorks"/>).
    /// El nombre del método se mantiene como OpenExtras por compatibilidad
    /// con referencias antiguas en el Inspector / eventos OnClick.
    /// </summary>
    public void OpenExtras()
    {
        OpenControls();
    }

    /// <summary>
    /// Muestra el panel de Controles. Mismo patrón que OpenCredits: bloquea la
    /// navegación normal del menú y desactiva la UI del menú principal hasta
    /// que el jugador pulse Confirmar.
    /// </summary>
    public void OpenControls()
    {
        if (controls == null)
        {
            // Si todavía no se ha asignado el panel en el Inspector, no
            // hacemos nada (mejor que dejar el menú en un estado raro).
            Debug.LogWarning("[MainMenuManager] El panel 'controls' no está " +
                             "asignado en el Inspector. El botón Controles " +
                             "no hará nada hasta que lo asignes.");
            return;
        }

        mainMenuWorks = false;
        mainMenu.SetActive(false);
        controls.SetActive(true);
    }

    public void OpenCredits()
    {
        mainMenuWorks = false;
        mainMenu.SetActive(false);
        if (controls != null) controls.SetActive(false);
        credits.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Salir del juego");
        Application.Quit();
    }

    public void CloseSettings()
    {
        CloseSettingsFromSettingsMenu();
    }


}
