using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static DataDefinitions;

/// <summary>
/// Gestiona el pequeño icono del diario que aparece en pantalla durante el
/// juego. El icono:
///   - Está visible SOLO durante el estado Play (in-game).
///   - En cualquier otro estado (Reading, Pause, Diary, MainMenu, Cinematic,
///     ChangeScene, GameOver) se oculta.
///   - Al pulsar Q desde Play: cambia al sprite "marcado" durante un segundo
///     y, al terminar, se oculta y abre el diario.
///   - Al pulsar Q desde Diary: cierra el diario directamente, sin animación.
/// </summary>
public class DiaryIconManager : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Image del icono del diario (el que se ve en pantalla durante el juego).")]
    [SerializeField] private Image iconImage;
    [Tooltip("GameObject que contiene el icono. Si se asigna, se activa/desactiva entero. " +
             "Si se deja vacío, se usa el GameObject del propio Image.")]
    [SerializeField] private GameObject iconRoot;

    [Header("Sprites del icono")]
    [Tooltip("Sprite normal del icono (visible durante Play).")]
    [SerializeField] private Sprite normalSprite;
    [Tooltip("Sprite \"marcado\" que se muestra durante el delay antes de abrir el diario.")]
    [SerializeField] private Sprite markedSprite;

    [Header("Tiempos")]
    [Tooltip("Segundos que se muestra el sprite marcado antes de abrir el diario.")]
    [SerializeField] private float openDelay = 1f;

    private Coroutine openRoutine;
    private bool isOpening;

    private GameObject Root => iconRoot != null
        ? iconRoot
        : (iconImage != null ? iconImage.gameObject : null);

    private void OnEnable()
    {
        // Estado inicial: ocultos hasta que GameManager nos diga que estamos en Play.
        SetVisible(false);

        GameManager.OnPlay      += HandlePlay;
        GameManager.OnReading   += HandleHide;
        GameManager.OnPause     += HandleHide;
        GameManager.OnDiary     += HandleHide;
        GameManager.OnMainMenu  += HandleHide;
        GameManager.OnCinematic += HandleHide;
        GameManager.OnGameOver  += HandleHide;
        GameManager.ChangeScene += HandleHide;

        InputManager.DiaryKeyPressedEvent += OnDiaryKeyPressed;

        // Si ya estamos en Play cuando este script arranca, mostramos el icono.
        if (GameManager.instance != null && GameManager.instance.actualState == GameStates.Play)
            HandlePlay();
    }

    private void OnDisable()
    {
        GameManager.OnPlay      -= HandlePlay;
        GameManager.OnReading   -= HandleHide;
        GameManager.OnPause     -= HandleHide;
        GameManager.OnDiary     -= HandleHide;
        GameManager.OnMainMenu  -= HandleHide;
        GameManager.OnCinematic -= HandleHide;
        GameManager.OnGameOver  -= HandleHide;
        GameManager.ChangeScene -= HandleHide;

        InputManager.DiaryKeyPressedEvent -= OnDiaryKeyPressed;

        if (openRoutine != null)
        {
            StopCoroutine(openRoutine);
            openRoutine = null;
        }
        isOpening = false;
    }

    private void HandlePlay()
    {
        // Cancelamos cualquier coroutine en curso, restauramos sprite normal y mostramos.
        if (openRoutine != null)
        {
            StopCoroutine(openRoutine);
            openRoutine = null;
        }
        isOpening = false;

        if (iconImage != null && normalSprite != null)
            iconImage.sprite = normalSprite;

        SetVisible(true);
    }

    private void HandleHide()
    {
        if (openRoutine != null)
        {
            StopCoroutine(openRoutine);
            openRoutine = null;
        }
        isOpening = false;

        SetVisible(false);
    }

    private void OnDiaryKeyPressed()
    {
        if (GameManager.instance == null) return;

        GameStates state = GameManager.instance.actualState;

        // Desde Diary, Q cierra el diario directamente (sin delay).
        if (state == GameStates.Diary)
        {
            GameManager.instance.ToggleDiary();
            return;
        }

        // Solo permitimos abrir desde Play.
        if (state != GameStates.Play) return;

        // Evitamos doble pulsación durante el delay.
        if (isOpening) return;

        isOpening = true;
        openRoutine = StartCoroutine(OpenWithDelay());
    }

    private IEnumerator OpenWithDelay()
    {
        // Cambiamos al sprite marcado.
        if (iconImage != null && markedSprite != null)
            iconImage.sprite = markedSprite;

        yield return new WaitForSeconds(openDelay);

        // Ocultamos el icono manualmente (sin pasar por HandleHide porque eso
        // pararía nuestra propia corrutina) y disparamos el diario.
        if (Root != null) Root.SetActive(false);

        openRoutine = null;
        isOpening = false;

        GameManager.instance.ToggleDiary();
    }

    private void SetVisible(bool visible)
    {
        var root = Root;
        if (root == null) return;
        if (root.activeSelf != visible) root.SetActive(visible);
    }
}
