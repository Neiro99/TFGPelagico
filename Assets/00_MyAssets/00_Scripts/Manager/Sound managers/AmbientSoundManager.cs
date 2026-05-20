using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AmbientSoundManager : MonoBehaviour
{
    public static AmbientSoundManager instancia;

    [Header("Ambiente")]
    [SerializeField] private AudioClip ambiente;

    [Range(0f, 1f)][SerializeField] private float volumen;
    [SerializeField] private bool loop = true;

    [Header("Fade")]
    [SerializeField] private float fadeInTime;
    [SerializeField] private float fadeOutTime;
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
        audioSource.volume = 0f;
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
        volumen = 0.4f;
        fadeOutTime = 1.2f;
        fadeInTime = 1.2f;

        AplicarAmbiente(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AplicarAmbiente(scene.buildIndex);
    }

    private void AplicarAmbiente(int buildIndex)
    {
        bool debeSonar = DebeSonarEn(buildIndex);

        if (debeSonar)
        {
            if (ambiente == null) return;

            if (audioSource.isPlaying && audioSource.clip == ambiente && audioSource.volume > 0.01f)
                return;

            if (fadeRoutine != null) StopCoroutine(fadeRoutine);

            audioSource.clip = ambiente;
            audioSource.loop = loop;

            if (!audioSource.isPlaying)
                audioSource.Play();

            fadeRoutine = StartCoroutine(FadeTo(volumen, fadeInTime));
        }
        else
        {
            if (!audioSource.isPlaying) return;

            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeOutAndStop());
        }
    }

    private bool DebeSonarEn(int buildIndex)
    {
        // Tras eliminar la escena 00_Introduccion el orden de buildIndex es:
        //   0 = 01_MainMenu, 1 = 02_OuterWorld, 2 = 03_InnerWorld,
        //   3 = 04_OuterWorld 2, 4 = 05_Fin.
        // Mantenemos el ambiente en MainMenu, InnerWorld y OuterWorld 2.
        return buildIndex == 0 || buildIndex == 2 || buildIndex == 3;
    }

    private IEnumerator FadeOutAndStop()
    {
        yield return FadeTo(0f, fadeOutTime);
        audioSource.Stop();
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

    public void SetVolumen(float v)
    {
        volumen = Mathf.Clamp01(v);
        if (audioSource.isPlaying && audioSource.volume > 0f)
            audioSource.volume = volumen;
    }
}
