using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static DataDefinitions;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    public GameStates actualState;

    public static event Action OnMainMenu;
    public static event Action ChangeScene;
    public static event Action OnPlay;
    public static event Action OnPause;
    public static event Action OnReading;
    public static event Action OnDiary;
    public static event Action OnPuzzle;
    public static event Action OnGameOver;
    public static event Action OnCinematic;

    public string currentDialogueCSV;
    public string currentDialogueAction;

    private void Awake()
    {
        SingletonPattern();
    }

    void Start()
    {
        ChangeState(actualState);
    }
    public void ChangeState(GameStates _newState)
    {
        actualState = _newState;

         
        switch (actualState)
        {
            case GameStates.MainMenu:
                OnMainMenu?.Invoke();
                break;
            case GameStates.ChangeScene:
                ChangeScene?.Invoke();
                break;
            case GameStates.Play:
                OnPlay?.Invoke();
                break;
            case GameStates.Cinematic:
                OnCinematic?.Invoke();
                break;
            case GameStates.Pause:
                OnPause?.Invoke();
                break;
            case GameStates.Reading:
                OnReading?.Invoke();
                break;
            case GameStates.Diary:
                OnDiary?.Invoke();
                break;
            case GameStates.Puzzle:
                OnPuzzle?.Invoke();
                break;
            case GameStates.GameOver:
                OnGameOver?.Invoke();
                break;
        }
    }
    void SingletonPattern()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            transform.GetChild(0).gameObject.SetActive(true);
        }
        else Destroy(gameObject);

    }
    public void Pause()
    {
        if (actualState == GameStates.Play)
            ChangeState(GameStates.Pause);
        else if (actualState == GameStates.Pause)
            ChangeState(GameStates.Play);
        else if (actualState == GameStates.Diary)
            ToggleDiary(); // Escape desde el diario lo cierra, no abre el pause
        else if (actualState == GameStates.Puzzle)
            ChangeState(GameStates.Play); // Escape desde el puzzle lo cierra
    }

    public void ToggleDiary()
    {
        if (actualState == GameStates.Play) ChangeState(GameStates.Diary);
        else if (actualState == GameStates.Diary) ChangeState(GameStates.Play);
        // Solo funciona desde Play o Diary; Pause y el resto lo ignoran
    }
}
