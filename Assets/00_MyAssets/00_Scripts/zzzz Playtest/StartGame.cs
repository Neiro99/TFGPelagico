using UnityEngine;

// NOTA: Este script solo se usaba en la antigua escena 00_Introduccion para
// saltar al Main Menu con Enter. Esa escena se ha eliminado del proyecto, por
// lo que en la práctica este componente ya no está colocado en ninguna escena
// del build. Se deja por si se quiere reutilizar como atajo de playtest.
// Tras la eliminación, el Main Menu es buildIndex 0.
public class StartGame : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ChangeSceneManager.instance.nextSceneInsdex = 0;
            ChangeSceneManager.instance.typeOfFade = "StandarFade";
            GameManager.instance.ChangeState(DataDefinitions.GameStates.ChangeScene);
        }
    }
}
