using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent (typeof(Canvas), typeof (Animator))]
public class ChangeSceneManager  : MonoBehaviour
{
    public static ChangeSceneManager instance;
    Animator anim;
    public int nextSceneInsdex;
    public string typeOfFade;


    private void Awake()
    {
        instance = this;
        anim = GetComponent<Animator>();
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
        anim.SetBool(typeOfFade, true);
    }

    void SelectedScene()
    {
        SceneManager.LoadScene(nextSceneInsdex);
    }
    void OnFadeComplete()
    {
        anim.SetBool(typeOfFade, false);
        GameManager.instance.ChangeState(DataDefinitions.GameStates.Play);
    }

}
