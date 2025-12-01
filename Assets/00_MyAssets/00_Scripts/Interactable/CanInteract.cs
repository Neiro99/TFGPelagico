using UnityEngine;

public class CanInteract : MonoBehaviour
{
    bool canInteract;
    private IInteractable interactable;


    private void Awake()
    {
        canInteract = false;
        interactable = GetComponent<IInteractable>();
    }

    private void OnEnable()
    {
        InputManager.InteractPressedEvent += TryInteract;
    }
    private void OnDisable()
    {
        InputManager.InteractPressedEvent -= TryInteract;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = true;
            transform.GetChild(0).gameObject.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }
    void TryInteract()
    {
        if (canInteract && interactable != null)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            interactable.ItsInteracting();
        }
    }

}
