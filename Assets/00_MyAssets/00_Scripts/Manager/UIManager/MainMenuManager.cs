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



    private void OnEnable()
    {
        menuIndex = 0;
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
            

        switch (menuIndex)
        {
            case 0:
                StartGame();
                break;
            case 1:
                OpenSettings();
                break;
            case 2:
                OpenExtras();
                break;
            case 3:
                OpenCredits();
                break;
            case 4:
                QuitGame();
                break;
        }
    }

    public void StartGame()
    {
        ChangeSceneManager.instance.nextSceneInsdex = 2;
        ChangeSceneManager.instance.typeOfFade = "StandarFade";
        GameManager.instance.ChangeState(DataDefinitions.GameStates.ChangeScene);
    }

    public void OpenSettings()
    {
        Debug.Log("Abrir configuración");
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
}
