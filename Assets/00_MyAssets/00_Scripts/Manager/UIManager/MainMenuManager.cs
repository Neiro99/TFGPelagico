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

    private void OnEnable()
    {
        menuIndex = 0;
        GameManager.OnMainMenu += ActivateMenu;
        GameManager.ChangeScene += DeactivateMenu;
        GameManager.OnPlay += DeactivateMenu;

        InputManager.MoveUpPressedEvent += MoveUp;
        InputManager.MoveDownPressedEvent += MoveDown;
        InputManager.SelectPressedEvent += SelectOption;
    }

    private void OnDisable()
    {
        GameManager.OnMainMenu -= ActivateMenu;
        GameManager.ChangeScene -= DeactivateMenu;
        GameManager.OnPlay -= DeactivateMenu;

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
    private void DeactivateMenu()
    {
        //aun no se muy bien que hacer aqui
        //mainMenuUI.SetActive(false);
    }

    private void NavigateMenu(bool itsUp)
    {
        UpdateMenuSelection(0);

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

    private void SelectOption()
    {
        switch(menuIndex)
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
        print("Iniciar juego");
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
        Debug.Log("Abrir extras");
    }

    public void OpenCredits()
    {
        Debug.Log("Abrir créditos");
    }

    public void QuitGame()
    {
        Debug.Log("Salir del juego");
        Application.Quit();
    }
}
