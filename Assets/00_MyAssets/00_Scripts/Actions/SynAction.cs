using UnityEngine;

public class SynAction : MonoBehaviour
{
    public static SynAction Instance;
    private Animator animator;

    private void Awake()
    {
        Instance = this;
        animator = GetComponent<Animator>();
    }

    public void FinishTalkSym()
    {
        animator.SetBool("SynGo", true);
    }

    public void desactivateSyn()
    {
        InputManager.Instance.canMove = true;
        gameObject.SetActive(false);
    }
}
