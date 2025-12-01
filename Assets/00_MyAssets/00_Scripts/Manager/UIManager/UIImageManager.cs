using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SpriteEntry
{
    public string key;
    public Sprite sprite;
}

public class UIImageManager : MonoBehaviour
{
    public static UIImageManager instance;

    public Image[] position;

    public SpriteEntry[] entries;

    private Dictionary<string, Sprite> spriteDict;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        spriteDict = new Dictionary<string, Sprite>();
        foreach (var e in entries)
        {
            if (!string.IsNullOrEmpty(e.key) && e.sprite != null)
                spriteDict[e.key] = e.sprite;
        }
    }

    public void ShowObjectImage(int viewIndex, string spriteKey)
    {
        position[viewIndex].sprite = spriteDict.ContainsKey(spriteKey) ? spriteDict[spriteKey] : null;
    }
}
