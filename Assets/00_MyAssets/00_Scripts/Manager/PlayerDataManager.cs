using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;
    public event Action<int, Vector3> DamageCall;
    public event Action<Vector3> Die;
    public event Action<int> HeartHub;
    int maxHealth;
    public int currentHealth;
    public int coins;
    public int keys;
    public int stars;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        maxHealth = 40;
        currentHealth = maxHealth;
    }
    private void OnEnable()
    {
        GameManager.OnMainMenu += ResetMainMenu;
    }

    private void OnDisable()
    {
        GameManager.OnMainMenu -= ResetMainMenu;
    }
    public void TakeDamage(int _Damage, Vector3 _enemyPosition)
    {
        currentHealth -= _Damage;

        if (currentHealth <= 0)
        {
            Die?.Invoke(_enemyPosition);
            HeartHub?.Invoke(currentHealth);
            return;
        }

        DamageCall?.Invoke(_Damage, _enemyPosition);
        HeartHub?.Invoke(currentHealth);
    }

    public void TakeItem(string _item)
    {
        switch (_item)
        {
            case "Heart":
                currentHealth = (currentHealth <= maxHealth - 4) ? currentHealth + 4 : maxHealth;
                HeartHub?.Invoke(currentHealth);
                break;

            case "Coin":
                coins++;
                //HudItemsDisplay.Instance.UpdateText(0, coins);
                break;

            case "Key":
                keys++;
                //HudItemsDisplay.Instance.UpdateText(1, keys);
                break;

            case "Star":
                stars++;
                break;
        }
    }

    public void UpdateKeys()
    {
        keys--;
        //HudItemsDisplay.Instance.UpdateText(1, keys);
    }

    void ResetMainMenu()
    {
        currentHealth = maxHealth;
        coins = 0;
        keys = 0;
        stars = 0;
    }
}
