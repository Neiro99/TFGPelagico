using UnityEngine;

public class Object : MonoBehaviour
{
    bool canInteract;

    private void Awake()
    {
        canInteract = false;
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
        if (canInteract)
        {
            UIManager.Instance.ActivateUI(1);
            GameManager.instance.ChangeState(DataDefinitions.GameStates.Reading);
        }
    }
}
