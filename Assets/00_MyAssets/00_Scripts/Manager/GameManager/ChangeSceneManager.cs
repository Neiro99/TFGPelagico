using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Canvas), typeof(Animator))]
public class ChangeSceneManager : MonoBehaviour
{
    public static ChangeSceneManager instance { get; private set; }

    private Animator anim;
    public int nextSceneInsdex;
    public string typeOfFade;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        anim = GetComponent<Animator>();
        DontDestroyOnLoad(gameObject);
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

        if (nextSceneInsdex == 1)
            GameManager.instance.ChangeState(DataDefinitions.GameStates.MainMenu);
        else
            GameManager.instance.ChangeState(DataDefinitions.GameStates.Play);
    }
}
