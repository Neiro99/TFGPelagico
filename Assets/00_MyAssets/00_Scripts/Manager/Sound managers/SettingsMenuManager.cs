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
    }

    public void Close()
    {
        enabled = false;
        settingsCanvas.SetActive(false);
    }
}
