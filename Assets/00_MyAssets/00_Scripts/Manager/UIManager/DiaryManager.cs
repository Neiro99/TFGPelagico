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
        StartCoroutine(SlideTo(currentPage + 1, direction: 1));
    }

    void PrevPage()
    {
        if (isAnimating || currentPage <= 0) return;
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
}
