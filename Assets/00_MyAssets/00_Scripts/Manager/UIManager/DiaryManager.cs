using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiaryManager : MonoBehaviour
{
    [Header("Imágenes de página")]
    [SerializeField] private Image         pageA;
    [SerializeField] private RectTransform pageARt;
    [SerializeField] private Image         pageB;
    [SerializeField] private RectTransform pageBRt;

    [Header("Sprites de las páginas (en orden)")]
    [SerializeField] private List<Sprite> pages;

    [Header("Animación")]
    [SerializeField] private float slideOffset   = 2000f;
    [SerializeField] private float slideDuration = 0.3f;

    [Header("Flechas de navegación (UI Images)")]
    [Tooltip("Image de la flecha izquierda. Su sprite se cambia según el estado.")]
    [SerializeField] private Image leftArrow;
    [Tooltip("Image de la flecha derecha. Su sprite se cambia según el estado.")]
    [SerializeField] private Image rightArrow;

    [Header("Sprites de la flecha izquierda")]
    [Tooltip("Sprite cuando se puede ir hacia la izquierda (página anterior disponible).")]
    [SerializeField] private Sprite leftArrowNormal;
    [Tooltip("Sprite cuando NO se puede ir hacia la izquierda (estamos en la primera página).")]
    [SerializeField] private Sprite leftArrowDimmed;
    [Tooltip("Sprite mientras dura la animación de paso de página hacia la izquierda.")]
    [SerializeField] private Sprite leftArrowPressed;

    [Header("Sprites de la flecha derecha")]
    [Tooltip("Sprite cuando se puede ir hacia la derecha (página siguiente disponible).")]
    [SerializeField] private Sprite rightArrowNormal;
    [Tooltip("Sprite cuando NO se puede ir hacia la derecha (estamos en la última página).")]
    [SerializeField] private Sprite rightArrowDimmed;
    [Tooltip("Sprite mientras dura la animación de paso de página hacia la derecha.")]
    [SerializeField] private Sprite rightArrowPressed;

    private RectTransform currentRt;
    private Image         currentImg;
    private RectTransform nextRt;
    private Image         nextImg;

    private int  currentPage;
    private bool isAnimating;

    private void OnEnable()
    {
        currentPage = 0;
        isAnimating = false;

        currentRt  = pageARt;
        currentImg = pageA;
        nextRt     = pageBRt;
        nextImg    = pageB;

        currentRt.anchoredPosition = Vector2.zero;
        nextRt.anchoredPosition    = new Vector2(slideOffset, 0f);

        ShowCurrentPage();
        UpdateArrowStates();

        InputManager.MoveRightPressedEvent += NextPage;
        InputManager.MoveLeftPressedEvent  += PrevPage;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        InputManager.MoveRightPressedEvent -= NextPage;
        InputManager.MoveLeftPressedEvent  -= PrevPage;
    }

    void NextPage()
    {
        if (isAnimating || currentPage >= pages.Count - 1) return;

        // Mostramos la flecha derecha "marcada" durante la transición.
        if (rightArrow != null && rightArrowPressed != null)
            rightArrow.sprite = rightArrowPressed;

        StartCoroutine(SlideTo(currentPage + 1, direction: 1));
    }

    void PrevPage()
    {
        if (isAnimating || currentPage <= 0) return;

        // Mostramos la flecha izquierda "marcada" durante la transición.
        if (leftArrow != null && leftArrowPressed != null)
            leftArrow.sprite = leftArrowPressed;

        StartCoroutine(SlideTo(currentPage - 1, direction: -1));
    }

    IEnumerator SlideTo(int newPage, int direction)
    {
        isAnimating = true;

        Vector2 nextStart    = new Vector2( slideOffset * direction, 0f);
        Vector2 currentEnd   = new Vector2(-slideOffset * direction, 0f);

        nextImg.sprite            = pages[newPage];
        nextRt.anchoredPosition   = nextStart;

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / slideDuration));

            currentRt.anchoredPosition = Vector2.Lerp(Vector2.zero, currentEnd,  t);
            nextRt.anchoredPosition    = Vector2.Lerp(nextStart,    Vector2.zero, t);

            yield return null;
        }

        currentRt.anchoredPosition = currentEnd;
        nextRt.anchoredPosition    = Vector2.zero;

        currentPage = newPage;
        SwapBuffers();

        isAnimating = false;

        // Al acabar la animación, restauramos las flechas al estado correcto
        // según la nueva página (normal u opacada).
        UpdateArrowStates();
    }

    void SwapBuffers()
    {
        (currentRt,  nextRt)  = (nextRt,  currentRt);
        (currentImg, nextImg) = (nextImg, currentImg);
    }

    void ShowCurrentPage()
    {
        if (pages == null || pages.Count == 0) return;
        currentImg.sprite = pages[currentPage];
    }

    /// <summary>
    /// Ajusta el sprite de cada flecha a "normal" si se puede navegar en esa
    /// dirección, o a "opacada" si no. Se llama al abrir el diario y al final
    /// de cada transición de página.
    /// </summary>
    void UpdateArrowStates()
    {
        bool canGoLeft  = currentPage > 0;
        bool canGoRight = pages != null && currentPage < pages.Count - 1;

        if (leftArrow != null)
        {
            Sprite s = canGoLeft ? leftArrowNormal : leftArrowDimmed;
            if (s != null) leftArrow.sprite = s;
        }

        if (rightArrow != null)
        {
            Sprite s = canGoRight ? rightArrowNormal : rightArrowDimmed;
            if (s != null) rightArrow.sprite = s;
        }
    }
}
