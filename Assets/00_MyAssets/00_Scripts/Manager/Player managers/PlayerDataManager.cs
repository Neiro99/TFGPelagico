using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;
    public int affinity;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void OnEnable()
    {
        GameManager.OnMainMenu += ResetMainMenu;
    }

    private void OnDisable()
    {
        GameManager.OnMainMenu -= ResetMainMenu;
    }
    public void ModifyAffinity(int addAffinity)
    {
        affinity += addAffinity;
        print(affinity);
    }
    void ResetMainMenu()
    {
        affinity = 0;
    }
}
