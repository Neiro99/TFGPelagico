using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager instancia;

    [Header("SFX List (por índice)")]
    [SerializeField] private AudioClip[] sfx;

    [Header("Ajustes")]
    [Range(0f, 1f)][SerializeField] private float volumen = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = 1f;
    }

    public void PlaySFX(int indice)
    {
        PlaySFX(indice, 1f);
    }

    public void PlaySFX(int indice, float volumenExtra)
    {
        if (sfx == null || sfx.Length == 0) return;
        if (indice < 0 || indice >= sfx.Length) return;
        if (sfx[indice] == null) return;

        float finalVol = Mathf.Clamp01(volumen * Mathf.Max(0f, volumenExtra));
        audioSource.PlayOneShot(sfx[indice], finalVol);
    }

    public void SetVolumen(float v)
    {
        volumen = Mathf.Clamp01(v);
    }
}
