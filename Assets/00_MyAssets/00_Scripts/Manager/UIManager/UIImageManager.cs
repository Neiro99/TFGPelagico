using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SpriteEntry
{
    public string key;
    public Sprite sprite;

    [Tooltip("Tamaño que se le aplicará al RectTransform del slot cuando se muestre " +
             "este sprite. Si se deja a (0,0), no se modifica el tamaño actual.")]
    public Vector2 size = Vector2.zero;
}

public class UIImageManager : MonoBehaviour
{
    public static UIImageManager instance;

    public Image[] position;

    public SpriteEntry[] entries;

    private Dictionary<string, SpriteEntry> spriteDict;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        spriteDict = new Dictionary<string, SpriteEntry>();
        foreach (var e in entries)
        {
            if (!string.IsNullOrEmpty(e.key) && e.sprite != null)
                spriteDict[e.key] = e;
        }
    }

    public void ShowObjectImage(int viewIndex, string spriteKey)
    {
        if (viewIndex < 0 || viewIndex >= position.Length || position[viewIndex] == null)
            return;

        Image img = position[viewIndex];

        if (spriteDict != null && spriteDict.TryGetValue(spriteKey, out SpriteEntry entry))
        {
            img.sprite = entry.sprite;

            // Solo redimensionamos si la entrada define un tamaño válido (>0).
            // Así, dejando size a (0,0) en el Inspector mantenemos el tamaño actual.
            if (entry.size.x > 0f && entry.size.y > 0f)
            {
                RectTransform rt = img.rectTransform;
                rt.sizeDelta = entry.size;
            }
        }
        else
        {
            img.sprite = null;
        }
    }
}
