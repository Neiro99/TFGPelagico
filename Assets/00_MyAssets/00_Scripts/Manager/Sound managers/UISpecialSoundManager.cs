using UnityEngine;
using static DataDefinitions;

public class UISpecialSoundManager : MonoBehaviour
{
    [Header("SFX Indices (SoundManager)")]
    [SerializeField] private int uiSelectSfx = 0;
    [SerializeField] private int uiMoveSfx = 1;
    [SerializeField] private int pauseSfx = 2;
    [SerializeField] private int pauseSfxout = 3;

    private bool wasInPause = false;
    private GameStates currentState;

    private void OnEnable()
    {
        GameManager.OnMainMenu += OnMainMenuState;
        GameManager.OnReading += OnReadingState;
        GameManager.OnPause += OnPauseState;
        GameManager.OnPlay += OnPlayState;

        InputManager.SelectPressedEvent += OnUISelect;
        InputManager.MoveDownPressedEvent += OnUIMove;
        InputManager.MoveUpPressedEvent += OnUIMove;
    }

    private void OnDisable()
    {
        GameManager.OnMainMenu -= OnMainMenuState;
        GameManager.OnReading -= OnReadingState;
        GameManager.OnPause -= OnPauseState;
        GameManager.OnPlay -= OnPlayState;

        InputManager.SelectPressedEvent -= OnUISelect;
        InputManager.MoveDownPressedEvent -= OnUIMove;
        InputManager.MoveUpPressedEvent -= OnUIMove;
    }

    private void OnMainMenuState()
    {
        SalidaDePauseSiToca();
        currentState = GameStates.MainMenu;
    }

    private void OnReadingState()
    {
        SalidaDePauseSiToca();
        currentState = GameStates.Reading;
    }

    private void OnPlayState()
    {
        currentState = GameStates.Play;
        SalidaDePauseSiToca();
    }

    private void OnPauseState()
    {
        currentState = GameStates.Pause;
        wasInPause = true;
        SoundManager.instancia.PlaySFX(pauseSfx);
    }

    private void SalidaDePauseSiToca()
    {
        if (wasInPause)
        {
            wasInPause = false;
            SoundManager.instancia.PlaySFX(pauseSfxout);
        }
    }

    private void OnUISelect()
    {
        if (!EsEstadoUI()) return;
        SoundManager.instancia.PlaySFX(uiSelectSfx);
    }

    private void OnUIMove()
    {
        if (!EsEstadoUI()) return;
        SoundManager.instancia.PlaySFX(uiMoveSfx);
    }

    private bool EsEstadoUI()
    {
        return currentState == GameStates.MainMenu
            || currentState == GameStates.Reading
            || currentState == GameStates.Pause;
    }
}
