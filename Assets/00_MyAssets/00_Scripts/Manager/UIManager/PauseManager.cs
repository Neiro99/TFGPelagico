using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Referencias")]
    public List<GameObject> menuButtons;
    public int menuIndex;

    private void OnEnable()
    {
        menuIndex = 1;
        NavigatePause(true);

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

    private void MoveUp() => NavigatePause(true);
    private void MoveDown() => NavigatePause(false);

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


    private void SelectOption()
    {
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

    public void Continue()
    {
        GameManager.instance.ChangeState(DataDefinitions.GameStates.Play);
    }

    public void Configuration()
    {
        Debug.Log("Abrir configuración");
    }

    public void Exit()
    {
        menuIndex = 1;
        NavigatePause(true);
        ChangeSceneManager.instance.nextSceneInsdex = 0;
        ChangeSceneManager.instance.typeOfFade = "StandarFade";
        GameManager.instance.ChangeState(DataDefinitions.GameStates.ChangeScene);
    }

}
