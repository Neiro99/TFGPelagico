using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager instancia;

    [SerializeField] private AudioClip musica1;
    [SerializeField] private AudioClip musica2;
    [SerializeField] private AudioClip musica3;

    [Range(0f, 1f)][SerializeField] private float volumen = 1f;
    [SerializeField] private bool loop = true;

    [Header("Fade")]
    [SerializeField] private float fadeOutTime = 1.5f;
    [SerializeField] private float fadeInTime = 1.5f;

    [SerializeField]
    private AnimationCurve fadeCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private AudioSource audioSource;
    private Coroutine fadeRoutine;

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
        audioSource.loop = loop;
        audioSource.volume = volumen;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        CambiarMusicaPara(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CambiarMusicaPara(scene.buildIndex);
    }

    private void CambiarMusicaPara(int buildIndex)
    {
        AudioClip objetivo = ClipSegunBuildIndex(buildIndex);
        if (objetivo == null) return;

        if (audioSource.clip == objetivo)
        {
            if (!audioSource.isPlaying) audioSource.UnPause();
            return;
        }

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);

        if (audioSource.clip == null || !audioSource.isPlaying)
        {
            audioSource.clip = objetivo;
            audioSource.loop = loop;
            audioSource.volume = 0f;
            audioSource.Play();
            fadeRoutine = StartCoroutine(FadeTo(volumen, fadeInTime));
            return;
        }

        fadeRoutine = StartCoroutine(FadeSwap(objetivo));
    }

    private IEnumerator FadeSwap(AudioClip nuevoClip)
    {
        yield return FadeTo(0f, fadeOutTime);

        audioSource.Stop();
        audioSource.clip = nuevoClip;
        audioSource.loop = loop;
        audioSource.volume = 0f;
        audioSource.Play();

        yield return FadeTo(volumen, fadeInTime);

        fadeRoutine = null;
    }

    private IEnumerator FadeTo(float target, float duration)
    {
        float start = audioSource.volume;

        if (duration <= 0f)
        {
            audioSource.volume = target;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(t / duration);

            float eased = fadeCurve.Evaluate(normalized);

            audioSource.volume = Mathf.Lerp(start, target, eased);
            yield return null;
        }

        audioSource.volume = target;
    }

    private AudioClip ClipSegunBuildIndex(int buildIndex)
    {
        switch (buildIndex)
        {
            case 0:
            case 1:
            case 4:
                return musica1;
            case 2:
                return musica2;
            case 3:
                return musica3;
            default:
                return musica1;
        }
    }

    public void SetVolumen(float v)
    {
        volumen = Mathf.Clamp01(v);
        audioSource.volume = volumen;
    }
}
