using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }


    [Header("UI roots")]
    public GameObject background;
    public GameObject characters;
    public GameObject objectView;
    public GameObject puzzle;
    public GameObject dialogue;
    public GameObject pause;
    public GameObject diario;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void OnEnable()
    {
        GameManager.OnPause += ActivatePause;
        GameManager.ChangeScene += DeactivatePause;
        GameManager.OnPlay += DeactivatePause;
        GameManager.OnDiary += ActivateDiary;
        GameManager.OnPlay += DeactivateDiary;
        GameManager.ChangeScene += DeactivateDiary;
    }

    private void OnDisable()
    {
        GameManager.OnPause -= ActivatePause;
        GameManager.ChangeScene -= DeactivatePause;
        GameManager.OnPlay -= DeactivatePause;
        GameManager.OnDiary -= ActivateDiary;
        GameManager.OnPlay -= DeactivateDiary;
        GameManager.ChangeScene -= DeactivateDiary;
    }

    public void ActivateUI(string objectUI, bool active)
    {
        switch (objectUI)
        {
            case "background":
                background.SetActive(active);
                break;
            case "characters":
                characters.SetActive(active);
                break;
            case "objectView":
                objectView.SetActive(active);
                break;
            case "puzzle":
                puzzle.SetActive(active);
                break;
            case "dialogue":
                dialogue.SetActive(active);
                break;
            case "pause":
                pause.SetActive(active);
                break;
            case "diario":
                diario.SetActive(active);
                break;
        }
    }
    public void DeactivateUI(string uiKey)
    {
        ActivateUI(uiKey, false);
    }

    public void ResetUI()
    {
        background.SetActive(false);
        characters.SetActive(false);
        objectView.SetActive(false);
        puzzle.SetActive(false);
        dialogue.SetActive(false);
        pause.SetActive(false);
        diario.SetActive(false);
    }

    private void ActivatePause()
    {
        ActivateUI("pause", true);
    }

    private void DeactivatePause()
    {
        ActivateUI("pause", false);
    }

    private void ActivateDiary()
    {
        ActivateUI("diario", true);
    }

    private void DeactivateDiary()
    {
        ActivateUI("diario", false);
    }

}
