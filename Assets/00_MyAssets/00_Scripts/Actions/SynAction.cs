using UnityEngine;

public class SynAction : MonoBehaviour
{
    public static SynAction Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void FinishTalkSym()
    {
        Debug.Log("Syn se mueve");
    }
}
