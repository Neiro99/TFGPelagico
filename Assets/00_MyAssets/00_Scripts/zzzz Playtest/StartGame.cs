using UnityEngine;

public class StartGame : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ChangeSceneManager.instance.nextSceneInsdex = 1;
            ChangeSceneManager.instance.typeOfFade = "StandarFade";
            GameManager.instance.ChangeState(DataDefinitions.GameStates.ChangeScene);
        }
    }
}
