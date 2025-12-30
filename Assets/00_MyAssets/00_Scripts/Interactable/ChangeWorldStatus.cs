using System.Collections;
using UnityEngine;

public class ChangeWorldStatus : MonoBehaviour
{
    public static ChangeWorldStatus Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void StartChanges()
    {
        print("Changing World Status");
        StartCoroutine(DelayedAutoPressE());
    }

    private IEnumerator DelayedAutoPressE()
    {
        yield return new WaitForSeconds(1f);
        InputManager.Instance.AutopressE();
    }
}
