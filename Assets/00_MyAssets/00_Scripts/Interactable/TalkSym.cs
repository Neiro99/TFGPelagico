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
        UIManager.Instance.ActivateUI(0);
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
