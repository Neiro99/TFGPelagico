using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }


    [Header("UI roots")]
    public GameObject background;
    public GameObject characters;
    public GameObject objectView;
    public GameObject puzzle;
    public GameObject dialogue;
    public GameObject pause;
    public GameObject diario;
    [Tooltip("Capa de pantalla de carga (controles + salpa). Debe estar como hijo " +
             "del canvas persistente y, en la jerarquía, por encima del resto para " +
             "que tape todo al activarse.")]
    public GameObject loading;
    [Tooltip("Previsualización a tamaño grande (p. ej. el dibujo del papel de la " +
             "mesa de Torpere a pantalla completa). Vive en este canvas " +
             "persistente y se activa/desactiva desde ObjectForeground.")]
    public GameObject largePreview;

    [Header("Canvases que dependen de la cámara de cada escena")]
    [Tooltip("Lista de Canvas en modo Screen Space - Camera (o World Space) que tienen que " +
             "reasignar su cámara cada vez que se carga una escena nueva. Útil para canvases " +
             "que persisten con DontDestroyOnLoad pero cuya cámara cambia entre escenas.")]
    public List<Canvas> canvasesToRebind = new List<Canvas>();
    [Tooltip("Tag que tiene la cámara principal de cada escena. Por defecto 'MainCamera'.")]
    public string mainCameraTag = "MainCamera";
    [Tooltip("Si está marcado, también se actualiza el componente Plane Distance del Canvas " +
             "según el valor de 'planeDistance' (solo aplica a Screen Space - Camera).")]
    public bool overridePlaneDistance = false;
    [Tooltip("Distancia desde la cámara a la que se sitúa el plano del Canvas (Screen Space - Camera).")]
    public float planeDistance = 10f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        // Primer enganche para la escena en la que arrancamos.
        RebindCanvasesToActiveSceneCamera();
    }

    private void OnEnable()
    {
        GameManager.OnPause += ActivatePause;
        GameManager.ChangeScene += DeactivatePause;
        GameManager.OnPlay += DeactivatePause;
        GameManager.OnDiary += ActivateDiary;
        GameManager.OnPlay += DeactivateDiary;
        GameManager.ChangeScene += DeactivateDiary;
        GameManager.OnPuzzle += ActivatePuzzle;
        GameManager.OnPlay += DeactivatePuzzle;
        GameManager.ChangeScene += DeactivatePuzzle;
        GameManager.OnLoading += ActivateLoading;
        GameManager.OnPlay += DeactivateLoading;
        GameManager.ChangeScene += DeactivateLoading;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        GameManager.OnPause -= ActivatePause;
        GameManager.ChangeScene -= DeactivatePause;
        GameManager.OnPlay -= DeactivatePause;
        GameManager.OnDiary -= ActivateDiary;
        GameManager.OnPlay -= DeactivateDiary;
        GameManager.ChangeScene -= DeactivateDiary;
        GameManager.OnPuzzle -= ActivatePuzzle;
        GameManager.OnPlay -= DeactivatePuzzle;
        GameManager.ChangeScene -= DeactivatePuzzle;
        GameManager.OnLoading -= ActivateLoading;
        GameManager.OnPlay -= DeactivateLoading;
        GameManager.ChangeScene -= DeactivateLoading;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Esperamos un frame: cuando se llama sceneLoaded los GameObjects ya están
        // creados pero ciertos sistemas como Cinemachine pueden tardar un frame extra
        // en estar listos. Con esto evitamos quedarnos con una referencia null.
        StartCoroutine(RebindNextFrame());
    }

    private IEnumerator RebindNextFrame()
    {
        yield return null;
        RebindCanvasesToActiveSceneCamera();
    }

    /// <summary>
    /// Busca la cámara principal de la escena activa (por tag) y se la asigna a
    /// todos los canvases configurados en <see cref="canvasesToRebind"/>.
    /// Llámalo manualmente si necesitas forzar la reasignación.
    /// </summary>
    public void RebindCanvasesToActiveSceneCamera()
    {
        if (canvasesToRebind == null || canvasesToRebind.Count == 0) return;

        Camera cam = FindMainCamera();
        if (cam == null) return;

        foreach (Canvas c in canvasesToRebind)
        {
            if (c == null) continue;

            // Si el canvas no está en modo Screen Space - Camera o World Space,
            // asignar worldCamera no hace nada — pero tampoco rompe.
            c.worldCamera = cam;

            if (overridePlaneDistance && c.renderMode == RenderMode.ScreenSpaceCamera)
                c.planeDistance = planeDistance;
        }
    }

    private Camera FindMainCamera()
    {
        // Camera.main devuelve la primera cámara enabled con el tag "MainCamera".
        // Si el tag por defecto no es ese, hacemos una búsqueda manual.
        if (string.IsNullOrEmpty(mainCameraTag) || mainCameraTag == "MainCamera")
            return Camera.main;

        GameObject go = GameObject.FindGameObjectWithTag(mainCameraTag);
        return go != null ? go.GetComponent<Camera>() : null;
    }

    public void ActivateUI(string objectUI, bool active)
    {
        switch (objectUI)
        {
            case "background":
                background.SetActive(active);
                break;
            case "characters":
                characters.SetActive(active);
                break;
            case "objectView":
                objectView.SetActive(active);
                break;
            case "puzzle":
                puzzle.SetActive(active);
                break;
            case "dialogue":
                dialogue.SetActive(active);
                break;
            case "pause":
                pause.SetActive(active);
                break;
            case "diario":
                diario.SetActive(active);
                break;
            case "loading":
                if (loading != null) loading.SetActive(active);
                break;
            case "largePreview":
                if (largePreview != null) largePreview.SetActive(active);
                break;
        }
    }
    public void DeactivateUI(string uiKey)
    {
        ActivateUI(uiKey, false);
    }

    public void ResetUI()
    {
        background.SetActive(false);
        characters.SetActive(false);
        objectView.SetActive(false);
        puzzle.SetActive(false);
        dialogue.SetActive(false);
        pause.SetActive(false);
        diario.SetActive(false);
        if (loading != null) loading.SetActive(false);
        if (largePreview != null) largePreview.SetActive(false);
    }

    private void ActivatePause()
    {
        ActivateUI("pause", true);
    }

    private void DeactivatePause()
    {
        ActivateUI("pause", false);
    }

    private void ActivateDiary()
    {
        ActivateUI("diario", true);
    }

    private void DeactivateDiary()
    {
        ActivateUI("diario", false);
    }

    private void ActivatePuzzle()
    {
        // Reseteamos primero para no mezclarnos con diálogos o vistas abiertas.
        ResetUI();
        ActivateUI("background", true);
        ActivateUI("puzzle", true);
    }

    private void DeactivatePuzzle()
    {
        ActivateUI("puzzle", false);
        ActivateUI("background", false);
    }

    private void ActivateLoading()
    {
        // Como pantalla "todo encima", reseteamos primero para que no se mezcle
        // con diálogos, diario, pausa, etc. y activamos solo la capa loading.
        ResetUI();
        ActivateUI("loading", true);
    }

    private void DeactivateLoading()
    {
        ActivateUI("loading", false);
    }

}
