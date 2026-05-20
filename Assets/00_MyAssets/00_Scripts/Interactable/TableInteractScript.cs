using System.Collections;
using UnityEngine;
using static DataDefinitions;

public class TableInteractScript : MonoBehaviour, Interactable
{
    [Header("Pantalla de carga intermedia (opcional)")]
    [Tooltip("Si se rellena, al acabar la animación de la mesa NO se cambia de " +
             "escena directamente: se le pide al LoadingScreenManager que muestre " +
             "la pantalla con esta key, y será él quien dispare el cambio a la " +
             "escena destino.")]
    public string loadingScreenKey = "Outer->Inner";

    [Tooltip("Índice de la escena destino (Build Settings). " +
             "Tras eliminar 00_Introduccion: 03_InnerWorld = buildIndex 2.")]
    public int nextSceneIndex = 2;

    [Tooltip("Fade que se usa SOLO si no hay pantalla de carga configurada.")]
    public string sceneTransitionFade = "SwichFade";

    private Animator anim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    public void ItsInteracting()
    {
        GameManager.instance.ChangeState(DataDefinitions.GameStates.Cinematic);
        anim.SetBool("FallVase", true);
        StartCoroutine(ChangeSceneDelayed());
    }
    private IEnumerator ChangeSceneDelayed()
    {
        yield return new WaitForSeconds(1f);

        if (!string.IsNullOrEmpty(loadingScreenKey) && LoadingScreenManager.instance != null)
        {
            LoadingScreenManager.instance.StartLoading(loadingScreenKey, nextSceneIndex);
        }
        else
        {
            ChangeSceneManager.instance.typeOfFade = sceneTransitionFade;
            ChangeSceneManager.instance.nextSceneInsdex = nextSceneIndex;
            GameManager.instance.ChangeState(DataDefinitions.GameStates.ChangeScene);
        }
    }

    public void makesound()
    {
        SoundManager.instancia.PlaySFX(5,1);
    }
}
