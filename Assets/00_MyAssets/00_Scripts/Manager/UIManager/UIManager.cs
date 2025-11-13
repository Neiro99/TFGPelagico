using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        GameManager.OnPause += ActivatePause;
        GameManager.ChangeScene += DeactivatePause;
        GameManager.OnPlay += DeactivatePause;
    }

    private void OnDisable()
    {
        GameManager.OnPause -= ActivatePause;
        GameManager.ChangeScene -= DeactivatePause;
        GameManager.OnPlay -= DeactivatePause;
    }
    public void ActivateUI(int uiIndex)
    {
        transform.GetChild(uiIndex).gameObject.SetActive(true);
    }
    public void DesActivateUI(int uiIndex)
    {
        transform.GetChild(uiIndex).gameObject.SetActive(false);
    }

    void ActivatePause()
    {
        ActivateUI(2);
        Time.timeScale = 0f;
    }
    void DeactivatePause()
    {
        DesActivateUI(2);
        Time.timeScale = 1f;
    }
}
