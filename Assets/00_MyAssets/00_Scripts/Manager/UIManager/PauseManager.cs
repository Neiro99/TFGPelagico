using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Referencias")]
    public List<GameObject> menuButtons;
    public int menuIndex;

    [Header("Settings UI")]
    public GameObject buttonContainer;
    public GameObject settingsContainer;
    public SettingsMenuManager settingsMenu;
    public bool pauseMenuWorks = true;

    private void OnEnable()
    {
        pauseMenuWorks = true;

        buttonContainer.SetActive(true);
        settingsContainer.SetActive(false);

        menuIndex = 1;
        NavigatePause(true);

        InputManager.MoveUpPressedEvent += MoveUp;
        InputManager.MoveDownPressedEvent += MoveDown;
        InputManager.SelectPressedEvent += SelectOption;
        
    }


    private void OnDisable()
    {
        settingsContainer.SetActive(false);
        InputManager.MoveUpPressedEvent -= MoveUp;
        InputManager.MoveDownPressedEvent -= MoveDown;
        InputManager.SelectPressedEvent -= SelectOption;
    }

    private void MoveUp()
    {
        if (!pauseMenuWorks) return;
        NavigatePause(true);
    }

    private void MoveDown()
    {
        if (!pauseMenuWorks) return;
        NavigatePause(false);
    }

    private void SelectOption()
    {
        if (!pauseMenuWorks) return;

        switch (menuIndex)
        {
            case 0:
                Continue();
                break;
            case 1:
                Configuration();
                break;
            case 2:
                Exit();
                break;
        }
    }


    private void NavigatePause(bool itsUp)
    {
        UpdatePauseSelection(0);
        ResetPauseSelector();

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

        UpdatePauseSelection(1);
    }
    private void UpdatePauseSelection(int changeColor)
    {
        Color c = new Color(0.953f, 0.976f, 1f, changeColor);

        menuButtons[menuIndex].transform.GetChild(0).GetComponent<Image>().color = c;
        menuButtons[menuIndex].transform.GetChild(2).GetComponent<Image>().color = c;
    }

    private void ResetPauseSelector()
    {
        int i;
        for (i = 0; i < menuButtons.Count; i++)
        {
            menuButtons[i].transform.GetChild(0).GetComponent<Image>().color = new Color(1f, 1f, 1f, 0);
            menuButtons[i].transform.GetChild(0).GetComponent<Image>().color = new Color(1f, 1f, 1f, 0);
            menuButtons[i].transform.GetChild(2).GetComponent<Image>().color = new Color(1f, 1f, 1f, 0);
        }
    }

    public void Continue()
    {
        GameManager.instance.ChangeState(DataDefinitions.GameStates.Play);
    }

    public void Configuration()
    {
        pauseMenuWorks = false;
        buttonContainer.SetActive(false);
        settingsContainer.SetActive(true);
        settingsMenu.Open();
        menuIndex = 0;
    }

    public void CloseSettingsFromPause()
    {
        pauseMenuWorks = true;

        settingsMenu.Close();
        settingsContainer.SetActive(false);

        buttonContainer.SetActive(true);

        menuIndex = 1;
        ResetPauseSelector();
        UpdatePauseSelection(1);
    }


    public void Exit()
    {
        menuIndex = 1;
        NavigatePause(true);
        // Volvemos al Main Menu. Tras eliminar la escena 00_Introduccion el
        // Main Menu es ahora la escena con buildIndex 0.
        ChangeSceneManager.instance.nextSceneInsdex = 0;
        ChangeSceneManager.instance.typeOfFade = "StandarFade";
        GameManager.instance.ChangeState(DataDefinitions.GameStates.ChangeScene);
    }

    public void CloseSettings()
    {
        CloseSettingsFromPause();
    }

}
