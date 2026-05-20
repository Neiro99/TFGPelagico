using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuManager : MonoBehaviour
{

    public GameObject settingsCanvas;

 
    public List<GameObject> optionRows;
    public List<GameObject> leftMarkers;
    public List<Slider> sliders;

    public int index;
    public float step;
    public float repeatDelay;

    public MonoBehaviour backTarget;


    float horizontalTimer;

    void Start()
    {
        index = 0;
        step = 0.05f;
        repeatDelay = 0.15f;
    }
    void OnEnable()
    {
        ResetState();
    }

    /// <summary>
    /// Reinicia el estado del panel: cursor arriba del todo, sliders
    /// sincronizados con el audio y markers visibles consistentes.
    /// Se llama desde OnEnable y desde <see cref="Open"/> para garantizar
    /// que el panel siempre se ve "en posición 1" la primera vez que se
    /// abre (sin depender del orden exacto entre OnEnable y Open).
    /// </summary>
    private void ResetState()
    {
        index = 0;
        SyncFromAudio();
        RefreshMarkers();
        horizontalTimer = 0f;
    }

    void Update()
    {
        HandleVertical();
        HandleHorizontal();
        HandleSelect();
    }

    void HandleVertical()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            index--;
            if (index < 0) index = optionRows.Count - 1;
            RefreshMarkers();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            index++;
            if (index >= optionRows.Count) index = 0;
            RefreshMarkers();
        }
    }

    void HandleHorizontal()
    {
        if (index > 3) return;

        horizontalTimer -= Time.unscaledDeltaTime;

        if (horizontalTimer > 0f) return;

        float dir = 0f;

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            dir = -1f;
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            dir = 1f;

        if (dir != 0f)
        {
            AdjustSlider(dir * step);
            horizontalTimer = repeatDelay;
        }
    }

    void AdjustSlider(float delta)
    {
        Slider s = sliders[index];
        s.value = Mathf.Clamp01(s.value + delta);
        ApplyIndex(index, s.value);
    }

    void HandleSelect()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (index == 4)
            { 
                backTarget.SendMessage("CloseSettings", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    void RefreshMarkers()
    {
        for (int i = 0; i < leftMarkers.Count; i++)
            leftMarkers[i].SetActive(i == index);
    }

    void SyncFromAudio()
    {
        var a = AudioSettingsManager.instancia;
        if (a == null) return;

        sliders[0].value = a.master;
        sliders[1].value = a.music;
        sliders[2].value = a.sfx;
        sliders[3].value = a.ambient;
    }

    void ApplyIndex(int i, float v)
    {
        var a = AudioSettingsManager.instancia;
        if (a == null) return;

        switch (i)
        {
            case 0: a.SetMaster(v); break;
            case 1: a.SetMusic(v); break;
            case 2: a.SetSFX(v); break;
            case 3: a.SetAmbient(v); break;
        }
    }

    public void Open()
    {
        settingsCanvas.SetActive(true);
        enabled = true;

        // Forzamos el reset explícitamente aquí. Si dejamos esto solo en
        // OnEnable, la primera vez que se abre el panel puede pasar que
        // OnEnable se ejecute antes de que Open() termine de configurar el
        // estado (por ejemplo si el componente arranca con enabled=false
        // serializado), y los markers visuales queden desincronizados
        // (típicamente apareciendo como si index=1). Llamando a
        // ResetState aquí garantizamos que cada apertura deja el panel en
        // index=0 de forma idempotente.
        ResetState();
    }

    public void Close()
    {
        enabled = false;
        settingsCanvas.SetActive(false);
    }
}
