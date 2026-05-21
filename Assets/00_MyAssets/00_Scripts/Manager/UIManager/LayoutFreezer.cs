using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// "Congelador" de layouts: captura las posiciones / tamaños / anchors /
/// pivots de todos los <see cref="RectTransform"/> hijos en el primer
/// frame en que el GameObject está activo, y los restaura cada vez que
/// vuelve a activarse.
///
/// Sirve para arreglar el caso típico de Unity UI en el que un contenedor
/// tiene un LayoutGroup en el padre + ContentSizeFitter en algún hijo: al
/// activar / desactivar el contenedor, los dos sistemas recalculan tamaños
/// en un orden que puede variar, y los elementos terminan en posiciones
/// ligeramente distintas cada vez ("se mueven solos"). Con este componente
/// la primera vez se confía en Unity y a partir de ahí siempre se reaplican
/// los mismos valores que se vieron correctos.
///
/// Cómo usarlo:
///   1. Añade el componente al GameObject "padre" (p. ej. el
///      <c>SettingsContainer</c>). Capturará TODOS los descendientes con
///      RectTransform.
///   2. Si quieres re-capturar (porque has cambiado el layout en el
///      Editor a propósito), borra el componente y vuélvelo a añadir, o
///      usa el ContextMenu "Recapture Layout" del Inspector.
///
/// El script NO usa <c>[ExecuteAlways]</c>: solo trabaja en Play, para no
/// pelearse con el Editor mientras maquetas la escena.
/// </summary>
public class LayoutFreezer : MonoBehaviour
{
    [System.Serializable]
    private struct RectSnapshot
    {
        public RectTransform target;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot;
        public Vector3 localScale;
    }

    [Header("Comportamiento")]
    [Tooltip("Si está activo, también se incluyen los hijos inactivos al " +
             "hacer la captura. Útil cuando el layout depende de sub-paneles " +
             "que se activan y desactivan (p. ej. el panel de Controles).")]
    public bool includeInactiveChildren = true;

    [Tooltip("Frames a esperar antes de capturar por primera vez. Subirlo a 2 " +
             "si el contenedor depende de animaciones o de layouts complejos " +
             "que tardan más de un frame en estabilizarse.")]
    [Range(1, 5)] public int captureDelayFrames = 1;

    [Tooltip("Si está activo, después de cada activación dejamos pasar un " +
             "frame antes de restaurar. Eso permite que LayoutGroup y " +
             "ContentSizeFitter hagan su trabajo primero y nosotros tengamos " +
             "la última palabra. Déjalo activo casi siempre.")]
    public bool restoreNextFrame = true;

    // Cache de los rects capturados. No se serializa: se reconstruye en
    // runtime al cargar la escena.
    private List<RectSnapshot> snapshots = new List<RectSnapshot>();
    private bool captured;
    private Coroutine pendingCapture;
    private Coroutine pendingRestore;

    private void OnEnable()
    {
        if (!Application.isPlaying) return;

        if (!captured)
        {
            // Primera activación: capturamos el estado "bueno" después de
            // dejar que Unity calcule sus layouts.
            if (pendingCapture != null) StopCoroutine(pendingCapture);
            pendingCapture = StartCoroutine(CaptureAfterFrames());
        }
        else
        {
            // Activaciones posteriores: restauramos los valores capturados.
            if (pendingRestore != null) StopCoroutine(pendingRestore);
            pendingRestore = StartCoroutine(RestoreRoutine());
        }
    }

    private IEnumerator CaptureAfterFrames()
    {
        for (int i = 0; i < captureDelayFrames; i++)
            yield return null;

        Capture();
    }

    private IEnumerator RestoreRoutine()
    {
        if (restoreNextFrame)
            yield return null;

        Restore();
    }

    /// <summary>
    /// Recorre todos los hijos con RectTransform y guarda sus posiciones /
    /// tamaños / anchors / pivots actuales como "valores de referencia".
    /// </summary>
    private void Capture()
    {
        snapshots.Clear();

        var rts = GetComponentsInChildren<RectTransform>(includeInactiveChildren);
        foreach (var rt in rts)
        {
            if (rt == null) continue;
            if (rt == transform) continue; // no nos capturamos a nosotros mismos

            snapshots.Add(new RectSnapshot
            {
                target = rt,
                anchoredPosition = rt.anchoredPosition,
                sizeDelta = rt.sizeDelta,
                anchorMin = rt.anchorMin,
                anchorMax = rt.anchorMax,
                pivot = rt.pivot,
                localScale = rt.localScale,
            });
        }

        captured = true;
    }

    /// <summary>
    /// Aplica los valores capturados a cada RectTransform. Si algún hijo
    /// ha sido destruido entre tanto, lo saltamos.
    /// </summary>
    private void Restore()
    {
        for (int i = 0; i < snapshots.Count; i++)
        {
            var s = snapshots[i];
            if (s.target == null) continue;

            s.target.anchorMin = s.anchorMin;
            s.target.anchorMax = s.anchorMax;
            s.target.pivot = s.pivot;
            s.target.anchoredPosition = s.anchoredPosition;
            s.target.sizeDelta = s.sizeDelta;
            s.target.localScale = s.localScale;
        }
    }

    [ContextMenu("Recapture Layout")]
    private void RecaptureFromMenu()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[LayoutFreezer] 'Recapture Layout' solo " +
                             "funciona en Play (el snapshot necesita los " +
                             "valores que LayoutGroup / ContentSizeFitter " +
                             "calculan en runtime).");
            return;
        }
        Capture();
    }
}
