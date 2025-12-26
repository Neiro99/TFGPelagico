using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject settingsCanvas;

    // 0 Master, 1 Musica, 2 SFX, 3 Ambiente, 4 Volver
    public List<GameObject> optionRows;
    public List<GameObject> leftMarkers;
    public List<Slider> sliders; // solo 0..3

    [Header("Navegación")]
    public int index = 0;
    public float step = 0.05f;
    public float repeatDelay = 0.15f;

    [Header("Main Menu")]
    public MainMenuManager mainMenuManager;

    float horizontalTimer;

    void OnEnable()
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

    // ───────────────────────────
    // NAVEGACIÓN VERTICAL ↑ ↓
    // ───────────────────────────
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

    // ───────────────────────────
    // AJUSTE HORIZONTAL ← →
    // ───────────────────────────
    void HandleHorizontal()
    {
        if (index > 3) return; // no estamos sobre un slider

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

    // ───────────────────────────
    // SELECT / ENTER
    // ───────────────────────────
    void HandleSelect()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (index == 4) // Volver
            {
                mainMenuManager.CloseSettingsFromSettingsMenu();
            }
        }
    }

    // ───────────────────────────
    // VISUAL
    // ───────────────────────────
    void RefreshMarkers()
    {
        for (int i = 0; i < leftMarkers.Count; i++)
            leftMarkers[i].SetActive(i == index);
    }

    // ───────────────────────────
    // AUDIO
    // ───────────────────────────
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

    // ───────────────────────────
    // ABRIR / CERRAR
    // ───────────────────────────
    public void Open()
    {
        settingsCanvas.SetActive(true);
        enabled = true;
    }

    public void Close()
    {
        enabled = false;
        settingsCanvas.SetActive(false);
    }
}
