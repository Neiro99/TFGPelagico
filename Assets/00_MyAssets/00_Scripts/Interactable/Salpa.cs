using UnityEngine;

public class Salpa : MonoBehaviour, IInteractable
{
    bool firstInteract;
    private void Awake()
    {
        firstInteract = true;
    }
    public void ItsInteracting()
    {
        if(firstInteract)
        {
            GameManager.instance.currentDialogueCSV = "Salpa";
            firstInteract = false;
        }
        else GameManager.instance.currentDialogueCSV = "Salpa2";

        UIManager.Instance.ActivateUI(1);
        GameManager.instance.ChangeState(DataDefinitions.GameStates.Reading);
        transform.GetChild(0).gameObject.SetActive(false);

        
    }
}
