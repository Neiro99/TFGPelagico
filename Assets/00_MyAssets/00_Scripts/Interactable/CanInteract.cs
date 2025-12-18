using UnityEngine;

public class CanInteract : MonoBehaviour
{
    bool canInteract;
    private Interactable interactable;


    private void Awake()
    {
        canInteract = false;
        interactable = GetComponent<Interactable>();
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
            SoundManager.instancia.PlaySFX(4);
            transform.GetChild(0).gameObject.SetActive(false);
            interactable.ItsInteracting();
        }
    }

}
