using UnityEngine;

public class OpenIpad : MonoBehaviour
{
    [Header("Hint de teclado (texto 'Pulsa Q')")]
    [SerializeField] private GameObject hintText;

    private void OnEnable()
    {
        GameManager.OnPlay  += ShowHint;
        GameManager.OnDiary += HideHint;
        GameManager.ChangeScene += HideHint;
    }

    private void OnDisable()
    {
        GameManager.OnPlay  -= ShowHint;
        GameManager.OnDiary -= HideHint;
        GameManager.ChangeScene -= HideHint;
    }

    void ShowHint()
    {
        if (hintText != null) hintText.SetActive(true);
    }

    void HideHint()
    {
        if (hintText != null) hintText.SetActive(false);
    }
}