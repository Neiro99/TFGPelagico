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
    }

    private void OnDisable()
    {
        GameManager.OnMainMenu -= ActivateMenu;

        InputManager.MoveUpPressedEvent -= MoveUp;
        InputManager.MoveDownPressedEvent -= MoveDown;
        InputManager.SelectPressedEvent -= SelectOption;
    }

    private void MoveUp() => NavigateMenu(true);
    private void MoveDown() => NavigateMenu(false);

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
        if (LoadingScreenManager.instance != null)
        {
            LoadingScreenManager.instance.StartLoading("Controls", 2);
        }
        else
        {
            // Fallback: si por lo que sea no hay LoadingScreenManager (escena
            // sin la jerarquía persistente), mantenemos el comportamiento antiguo.
            ChangeSceneManager.instance.nextSceneInsdex = 2;
            ChangeSceneManager.instance.typeOfFade = "StandarFade";
            GameManager.instance.ChangeState(DataDefinitions.GameStates.ChangeScene);
        }
    }

    public void OpenSettings()
    {
        settingsWorks = false;

        mainMenu.SetActive(false);
        credits.SetActive(false);

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


    public void OpenExtras()
    {
        ChangeSceneManager.instance.nextSceneInsdex = 0;
        ChangeSceneManager.instance.typeOfFade = "StandarFade";
        GameManager.instance.ChangeState(DataDefinitions.GameStates.ChangeScene);
    }

    public void OpenCredits()
    {
        mainMenuWorks = false;
        mainMenu.SetActive(false);
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
