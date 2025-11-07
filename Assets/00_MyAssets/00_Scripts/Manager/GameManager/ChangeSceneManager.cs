using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent (typeof(Canvas), typeof (Animator))]
public class ChangeSceneManager  : MonoBehaviour
{
    public static ChangeSceneManager instance;
    Animator anim;
    string actualScene;
    public int nextSceneInsdex;

    private void Awake()
    {
        instance = this;
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
        SceneManager.LoadScene(nextSceneInsdex);
    }
    void OnFadeComplete()
    {
        GameManager.instance.ChangeState(DataDefinitions.GameStates.Play);
        anim.enabled = false;
    }

}
