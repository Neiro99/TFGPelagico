using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Canvas), typeof(Animator))]
public class ChangeSceneManager : MonoBehaviour
{
    public static ChangeSceneManager instance { get; private set; }

    private Animator anim;
    public int nextSceneInsdex;
    public string typeOfFade;

    // -- Modo "fade con callback" ------------------------------------------
    // Cuando skipSceneLoad está a true, en el momento de máximo negro NO se
    // carga ninguna escena: se invoca onBlackCallback en su lugar (y el flujo
    // sigue con el fade in). Al terminar el fade in se invoca
    // onCompleteCallback. Lo usa el LoadingScreenManager para encadenar dos
    // ciclos de fade (menú → loading, loading → escena 2) sin saltos visuales.
    private bool skipSceneLoad;
    private Action onBlackCallback;
    private Action onCompleteCallback;

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
        // Camino "clásico": al momento de negro se carga la escena destino y
        // al terminar el fade in se pasa a Play (o MainMenu si el índice es 1).
        skipSceneLoad = false;
        onBlackCallback = null;
        onCompleteCallback = null;
        anim.SetBool(typeOfFade, true);
    }

    /// <summary>
    /// Dispara la misma animación de fade que <see cref="ChangeScene"/> pero
    /// SIN cargar escena: en el momento de máximo negro invoca
    /// <paramref name="onBlack"/> y al acabar el fade in invoca
    /// <paramref name="onComplete"/>. Útil para transiciones intermedias en
    /// las que solo queremos cambiar contenido en pantalla (por ejemplo,
    /// activar la capa de la pantalla de carga) en el frame en que la
    /// pantalla está totalmente negra.
    /// </summary>
    public void PlayFadeWithCallback(string fadeType, Action onBlack, Action onComplete)
    {
        typeOfFade = fadeType;
        skipSceneLoad = true;
        onBlackCallback = onBlack;
        onCompleteCallback = onComplete;
        anim.SetBool(typeOfFade, true);
    }

    // Animation Event: se llama en el frame de máximo negro de la animación.
    void SelectedScene()
    {
        if (skipSceneLoad)
        {
            onBlackCallback?.Invoke();
            return;
        }

        SceneManager.LoadScene(nextSceneInsdex);
    }

    // Animation Event: se llama en el último frame del fade in.
    void OnFadeComplete()
    {
        anim.SetBool(typeOfFade, false);

        if (skipSceneLoad)
        {
            // Modo callback: dejamos el sistema limpio y ejecutamos onComplete.
            var cb = onCompleteCallback;
            skipSceneLoad = false;
            onBlackCallback = null;
            onCompleteCallback = null;
            cb?.Invoke();
            return;
        }

        // Tras eliminar la escena 00_Introduccion, el MainMenu pasa a ser la
        // primera escena (buildIndex 0). Por eso comparamos contra 0.
        if (nextSceneInsdex == 0)
            GameManager.instance.ChangeState(DataDefinitions.GameStates.MainMenu);
        else
            GameManager.instance.ChangeState(DataDefinitions.GameStates.Play);
    }
}
