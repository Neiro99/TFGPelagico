using UnityEngine;

public class SynAction : MonoBehaviour
{
    public static SynAction Instance;
    private Animator animator;
    public Animator moveanimator;


    private void Awake()
    {
        Instance = this;
        animator = GetComponent<Animator>();
    }

    public void FinishTalkSym()
    {
        animator.SetBool("SynGo", true);
        moveanimator.SetBool("SynWalk", true);
    }

    public void desactivateSyn()
    {
        InputManager.Instance.canMove = true;
        gameObject.SetActive(false);
    }
}
