using UnityEngine;

public class TalkSym : MonoBehaviour
{
    private void OnEnable()
    {
        InputManager.Instance.canMove = false;
        InputManager.InteractPressedEvent += ItsInteracting;
    }
    private void OnDisable()
    {
        InputManager.InteractPressedEvent -= ItsInteracting;
    }
    public void ItsInteracting()
    {
        GameManager.instance.currentDialogueCSV = "Syn";
        GameManager.instance.currentDialogueAction = "FinishTalkSym";

        UIManager.instance.ActivateUI("background", true);
        UIManager.instance.ActivateUI("dialogue", true);
        UIManager.instance.ActivateUI("characters", true);

        GameManager.instance.ChangeState(DataDefinitions.GameStates.Reading);
        transform.GetChild(0).gameObject.SetActive(false);
        InputManager.Instance.canMove = true;
        GetComponent<Collider>().enabled = false;

        CanInteract ci = GetComponent<CanInteract>();
        if (ci != null)
            ci.enabled = false;

        this.enabled = false;
    }
}
