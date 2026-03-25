using UnityEngine;

public class OpenIpad : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private GameObject Ipad; 
    [SerializeField] private GameObject Text; 
    [SerializeField] private bool startActive = false; 

    private bool isActive = false;
    private void OnEnable()
    {
        GameManager.OnPlay += TextOn;
    }
    void Start()
    {
        // Configurar estado inicial
        if (Ipad != null)
        {
            isActive = startActive;
            Ipad.SetActive(isActive);
        }
    }

    void Update()
    {
        // Activar con Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleObject();
        }

        // Desactivar con Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isActive)
            {
                DeactivateObject();
            }
        }
    }

    void ToggleObject()
    {
        if (Ipad != null)
        {
            isActive = !isActive;
            Ipad.SetActive(isActive);
        }
    }

    void DeactivateObject()
    {
        if (Ipad != null)
        {
            isActive = false;
            Ipad.SetActive(false);
        }
    }

    void TextOn()
    {
        Text.SetActive(true);
    }
}