using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent (typeof(Canvas), typeof (Animator))]
public class ChangeSceneManager  : MonoBehaviour
{
    public static ChangeSceneManager instancia;
    Animator anim;
    string actualScene;

    private void Awake()
    {
        instancia = this;
        anim = GetComponent<Animator>();
        anim.enabled = false;
    }
    private void OnEnable()
    {
        GameManager.ChangeScene += ChangeScene;
    }

    private void OnDisable()
    {
        GameManager.ChangeScene -= ChangeScene;
    }

    void ChangeScene()
    {
        actualScene = SceneManager.GetActiveScene().name;
        anim.enabled = true;
    }

    void SelectedScene()
    {
        if (actualScene == "00_inner_world")
        {
            SceneManager.LoadScene("01_External_world");
        }
        else
        {
            SceneManager.LoadScene("00_inner_world");
        }
    }
    void OnFadeComplete()
    {
        GameManager.instance.ChangeState(DataDefinitions.GameStates.Play);
        anim.enabled = false;
    }

}
