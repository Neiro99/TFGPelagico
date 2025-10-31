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
    public static event Action OnGameOver;

    private void Awake()
    {
        SingletonPattern();
    }

    void Start()
    {
        ChangeState(actualState);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeState(GameStates.ChangeScene);
        }
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
            case GameStates.Pause:
                OnPause?.Invoke();
                break;
            case GameStates.Reading:
                OnReading?.Invoke();
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
        if (actualState == GameStates.Play) ChangeState(GameStates.Pause);
        else if (actualState == GameStates.Pause) ChangeState(GameStates.Play);
    }
}
