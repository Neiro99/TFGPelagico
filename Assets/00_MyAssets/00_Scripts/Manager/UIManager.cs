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
    public void ActivateUI(int uiIndex)
    {
        transform.GetChild(uiIndex).gameObject.SetActive(true);
    }
    public void DesActivateUI(int uiIndex)
    {
        transform.GetChild(uiIndex).gameObject.SetActive(false);
    }
}
