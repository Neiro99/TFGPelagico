using UnityEngine;

public class AudioSettingsManager : MonoBehaviour
{
    public static AudioSettingsManager instancia;

    [Range(0f, 1f)] public float master = 1f;
    [Range(0f, 1f)] public float music = 1f;
    [Range(0f, 1f)] public float sfx = 1f;
    [Range(0f, 1f)] public float ambient = 1f;

    const string K_MASTER = "vol_master";
    const string K_MUSIC = "vol_music";
    const string K_SFX = "vol_sfx";
    const string K_AMBIENT = "vol_ambient";

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        instancia = this;
        DontDestroyOnLoad(gameObject);

        Cargar();
        AplicarTodo();
    }

    public void AplicarTodo()
    {
        AudioListener.volume = master;

        if (MusicManager.instancia != null) MusicManager.instancia.SetVolumen(music);
        if (SoundManager.instancia != null) SoundManager.instancia.SetVolumen(sfx);
        if (AmbientSoundManager.instancia != null) AmbientSoundManager.instancia.SetVolumen(ambient);
    }

    public void SetMaster(float v) { master = Mathf.Clamp01(v); AudioListener.volume = master; Guardar(); }
    public void SetMusic(float v) { music = Mathf.Clamp01(v); if (MusicManager.instancia != null) MusicManager.instancia.SetVolumen(music); Guardar(); }
    public void SetSFX(float v) { sfx = Mathf.Clamp01(v); if (SoundManager.instancia != null) SoundManager.instancia.SetVolumen(sfx); Guardar(); }
    public void SetAmbient(float v) { ambient = Mathf.Clamp01(v); if (AmbientSoundManager.instancia != null) AmbientSoundManager.instancia.SetVolumen(ambient); Guardar(); }

    void Guardar()
    {
        PlayerPrefs.SetFloat(K_MASTER, master);
        PlayerPrefs.SetFloat(K_MUSIC, music);
        PlayerPrefs.SetFloat(K_SFX, sfx);
        PlayerPrefs.SetFloat(K_AMBIENT, ambient);
        PlayerPrefs.Save();
    }

    void Cargar()
    {
        master = PlayerPrefs.GetFloat(K_MASTER, 1f);
        music = PlayerPrefs.GetFloat(K_MUSIC, 1f);
        sfx = PlayerPrefs.GetFloat(K_SFX, 1f);
        ambient = PlayerPrefs.GetFloat(K_AMBIENT, 1f);
    }
}
