using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject PauseUI;
    public List<GameObject> menuButtons;
    public int menuIndex;

    private void OnEnable()
    {
        menuIndex = 0;

        GameManager.OnPause += ActivatePause;
        GameManager.ChangeScene += DeactivatePause;
        GameManager.OnPlay += DeactivatePause;
        InputManager.MoveUpPressedEvent += () => NavigatePause(true);
        InputManager.MoveDownPressedEvent += () => NavigatePause(false);
        InputManager.SelectPressedEvent += SelectOption;
    }

    private void OnDisable()
    {
        GameManager.OnPause -= ActivatePause;
        GameManager.ChangeScene -= DeactivatePause;
        GameManager.OnPlay -= DeactivatePause;
        InputManager.MoveUpPressedEvent -= () => NavigatePause(true);
        InputManager.MoveDownPressedEvent -= () => NavigatePause(false);
        InputManager.SelectPressedEvent -= SelectOption;
    }
    private void ActivatePause()
    {
        PauseUI.SetActive(true);
    }
    private void DeactivatePause()
    {
        PauseUI.SetActive(false);
    }

    private void NavigatePause(bool itsUp)
    {
        UpdatePauseSelection(0);

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
        menuButtons[menuIndex].transform.GetChild(0).GetComponent<Image>().color = new Color(1f, 1f, 1f, changeColor);
        menuButtons[menuIndex].transform.GetChild(2).GetComponent<Image>().color = new Color(1f, 1f, 1f, changeColor);
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
        ChangeSceneManager.instance.nextSceneInsdex = 1;
        ChangeSceneManager.instance.typeOfFade = "StandarFade";
        GameManager.instance.ChangeState(DataDefinitions.GameStates.ChangeScene);
    }

    public void Configuration()
    {
        Debug.Log("Abrir configuración");
    }

    public void Exit()
    {
        Debug.Log("Abrir extras");
    }

}
