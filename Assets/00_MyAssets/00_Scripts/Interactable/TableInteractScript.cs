using System.Collections;
using UnityEngine;
using static DataDefinitions;

public class TableInteractScript : MonoBehaviour, Interactable
{
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
        ChangeSceneManager.instance.typeOfFade = "SwichFade";
        ChangeSceneManager.instance.nextSceneInsdex = 3;
        GameManager.instance.ChangeState(DataDefinitions.GameStates.ChangeScene);
    }

    public void makesound()
    {
        SoundManager.instancia.PlaySFX(5,1);
    }
}
