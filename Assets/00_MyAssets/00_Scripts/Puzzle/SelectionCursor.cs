using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SelectionCursor : MonoBehaviour
{
    [Header("References")]
    public PuzzleBoard board;
    [Tooltip("Visual object used as the cursor highlight (e.g. a frame sprite).")]
    public Transform cursorVisual;
    [Tooltip("Child of the cursor used to show the sprite of the held tile.")]
    public Transform heldVisual;

    [Header("Cursor settings")]
    public Vector2Int startCell = new Vector2Int(0, 0);
    public float cursorZOffset = -0.1f;

    [Header("Feedback de acción inválida")]
    [Tooltip("Índice del clip en SoundManager que se reproduce cuando se " +
             "intenta una acción no permitida (mover hacia algo bloqueado, " +
             "rotar una pieza fija, coger una casilla vacía, etc.). " +
             "Configura el sonido en el array 'sfx' del SoundManager y pon " +
             "aquí su índice. Si lo dejas en -1 no suena nada.")]
    public int rejectSfxIndex = -1;

    [Tooltip("Duración total del meneo cuando la acción se rechaza (segundos).")]
    [Range(0.05f, 1f)] public float shakeDuration = 0.22f;

    [Tooltip("Distancia máxima del meneo. Si el cursor es un RectTransform (UI) " +
             "se interpreta en píxeles del Canvas; si es un Transform normal, en " +
             "unidades del mundo. Valores típicos: 8-20 para UI, 0.1-0.3 para mundo.")]
    [Range(0.01f, 50f)] public float shakeAmplitude = 12f;

    [Tooltip("Número de oscilaciones del meneo (mayor = más vibración).")]
    [Range(1, 12)] public int shakeOscillations = 5;


    private Vector2Int cursorPos;


    private PipeTile heldTile = null;
    private Coroutine shakeCoroutine;

    private SpriteRenderer heldRendererSR;
    private Image heldRendererImg;
    private Color heldOriginalColor;

    private SpriteRenderer heldVisualSR;
    private Image heldVisualImg;

    private void Start()
    {
        if (board == null)
        {
            board = FindObjectOfType<PuzzleBoard>();
        }

        if (heldVisual != null)
        {
            heldVisualSR = heldVisual.GetComponent<SpriteRenderer>();
            heldVisualImg = heldVisual.GetComponent<Image>();
            HideHeldVisual();
        }

        cursorPos = startCell;
        ClampCursor();
        UpdateCursorVisualPosition();
    }

    private void OnEnable()
    {
        // El puzzle se comporta como otro menú: bloqueamos el movimiento de Aster
        // y nos suscribimos a los eventos del InputManager.
        if (InputManager.Instance != null)
            InputManager.Instance.canMove = false;

        InputManager.MoveLeftPressedEvent  += MoveLeft;
        InputManager.MoveRightPressedEvent += MoveRight;
        InputManager.MoveUpPressedEvent    += MoveUp;
        InputManager.MoveDownPressedEvent  += MoveDown;
        InputManager.RotatePressedEvent    += TryRotate;
        InputManager.PickDropPressedEvent  += TryPickDrop;
    }

    private void OnDisable()
    {
        InputManager.MoveLeftPressedEvent  -= MoveLeft;
        InputManager.MoveRightPressedEvent -= MoveRight;
        InputManager.MoveUpPressedEvent    -= MoveUp;
        InputManager.MoveDownPressedEvent  -= MoveDown;
        InputManager.RotatePressedEvent    -= TryRotate;
        InputManager.PickDropPressedEvent  -= TryPickDrop;

        // Si Aster vuelve a Play, devolvemos el control del movimiento.
        if (InputManager.Instance != null)
            InputManager.Instance.canMove = true;
    }

    // ---------------------------------------------------------------------
    // Handlers de los eventos del InputManager
    // ---------------------------------------------------------------------

    private void MoveLeft()  { TryMove(Vector2Int.left);  }
    private void MoveRight() { TryMove(Vector2Int.right); }

    // Igual que en la versión original: pulsar "arriba" baja la coordenada Y
    // (porque y=0 está arriba en el board), y pulsar "abajo" la sube.
    private void MoveUp()    { TryMove(Vector2Int.down);  }
    private void MoveDown()  { TryMove(Vector2Int.up);    }

    private void TryMove(Vector2Int delta)
    {
        if (board == null) return;
        // Mientras está el meneo del feedback de error, ignoramos cualquier
        // intento de moverse: si dejásemos pasar, cursorPos cambiaría pero
        // el cursorVisual está siendo controlado por ShakeRoutine, que al
        // terminar restauraría su posición vieja y se vería un "salto".
        if (shakeCoroutine != null) return;

        Vector2Int newPos = cursorPos + delta;

        newPos.x = Mathf.Clamp(newPos.x, 0, board.width - 1);
        newPos.y = Mathf.Clamp(newPos.y, 0, board.height - 1);

        // Caso 1: el cursor está pegado al borde y se intenta salir.
        // El Clamp deja newPos == cursorPos así que no se mueve nada;
        // damos feedback de que la acción no tiene efecto.
        if (newPos == cursorPos)
        {
            PlayRejectFeedback();
            return;
        }

        if (heldTile != null)
        {
            PipeTile targetTile = board.GetTile(newPos);

            // Caso 2: arrastrando una pieza, la casilla destino está ocupada
            // por otra pieza distinta de la que llevamos.
            if (targetTile != null &&
                targetTile != heldTile &&
                targetTile.tileKind != TileKind.Empty)
            {
                PlayRejectFeedback();
                return;
            }
        }

        cursorPos = newPos;
        UpdateCursorVisualPosition();
    }

    private void TryRotate()
    {
        if (board == null) return;
        // Bloqueamos durante el meneo para que la entrada quede coherente
        // con el feedback visual (ver comentario en TryMove).
        if (shakeCoroutine != null) return;

        if (heldTile != null)
        {
            if (heldTile.canRotate)
            {
                heldTile.RotateClockwise();
                UpdateHeldVisualFromHeldTile();
            }
            else
            {
                // Pieza agarrada que no puede rotar.
                PlayRejectFeedback();
            }
            return;
        }

        PipeTile tile = board.GetTile(cursorPos);
        if (tile != null && tile.canRotate)
        {
            tile.RotateClockwise();
        }
        else
        {
            // Casilla vacía o pieza fija: no se puede rotar.
            PlayRejectFeedback();
        }
    }

    private void TryPickDrop()
    {
        if (board == null) return;
        // Bloqueamos durante el meneo (ver comentario en TryMove).
        if (shakeCoroutine != null) return;

        PipeTile currentTile = board.GetTile(cursorPos);

        if (heldTile == null)
        {
            // Intentar coger:
            // - No hay tile bajo el cursor.
            // - O la pieza es fija (no se puede mover).
            // - O la casilla está vacía.
            if (currentTile == null)
            {
                PlayRejectFeedback();
                return;
            }

            if (!currentTile.canMove)
            {
                PlayRejectFeedback();
                return;
            }

            if (currentTile.tileKind == TileKind.Empty)
            {
                PlayRejectFeedback();
                return;
            }

            heldTile = currentTile;
            CacheHeldRenderer(heldTile);
            MakeHeldTileTransparent();
            CopyTileSpriteToHeldVisual(heldTile);
        }
        else
        {
            // Intentar soltar.
            if (currentTile == null)
            {
                PlayRejectFeedback();
                return;
            }

            if (currentTile.tileKind == TileKind.Empty)
            {
                board.SwapTiles(heldTile, currentTile);
            }
            else if (currentTile != heldTile)
            {
                // No es Empty y no es la misma pieza que llevamos: la pieza
                // no se puede colocar aquí. El comportamiento original
                // "soltaba" la pieza igualmente (devolviéndola a su sitio).
                // Mantenemos eso pero damos feedback de posición inválida.
                PlayRejectFeedback();
            }
            // else: soltar en la misma casilla de la que se cogió la pieza
            // es una "cancelación" del agarre, no un error. No suena nada.

            RestoreHeldTileColor();
            heldTile = null;
            heldRendererSR = null;
            heldRendererImg = null;
            HideHeldVisual();
        }
    }

    // ---------------------------------------------------------------------
    // Feedback de acción inválida (sonido + meneo)
    // ---------------------------------------------------------------------

    /// <summary>
    /// Reproduce el SFX configurado y hace un meneo visual del cursor (o de
    /// la pieza, si hay una agarrada) para indicar que la acción que se
    /// acaba de intentar no se puede realizar.
    /// </summary>
    private void PlayRejectFeedback()
    {
        // Sonido a través del SoundManager existente, usando el índice
        // configurado en el Inspector.
        if (rejectSfxIndex >= 0 && SoundManager.instancia != null)
            SoundManager.instancia.PlaySFX(rejectSfxIndex);

        // Meneamos la pieza agarrada si la hay; si no, el propio cursor.
        Transform target = (heldTile != null && heldVisual != null) ? heldVisual : cursorVisual;
        if (target == null) return;

        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeRoutine(target));
    }

    /// <summary>
    /// Oscila la posición del target con amplitud decreciente durante
    /// <see cref="shakeDuration"/> segundos. Detecta si el target es un
    /// RectTransform y, en ese caso, usa <c>anchoredPosition</c> (en
    /// píxeles del Canvas); si es un Transform normal usa
    /// <c>localPosition</c> (en unidades del mundo). Esto evita que en
    /// Canvas Screen Space - Camera con scale pequeña el meneo no se vea.
    ///
    /// Usa Time.unscaledDeltaTime para funcionar aunque el puzzle pause
    /// el juego (Time.timeScale = 0).
    /// </summary>
    private IEnumerator ShakeRoutine(Transform t)
    {
        RectTransform rt = t as RectTransform;

        Vector2 rtOrigin = rt != null ? rt.anchoredPosition : Vector2.zero;
        Vector3 tOrigin  = rt == null ? t.localPosition     : Vector3.zero;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(elapsed / shakeDuration);

            // Onda senoidal con amplitud que se atenúa a 0 al final.
            float wave = Mathf.Sin(u * shakeOscillations * Mathf.PI * 2f);
            float damp = 1f - u;
            float offset = wave * shakeAmplitude * damp;

            if (rt != null)
                rt.anchoredPosition = rtOrigin + new Vector2(offset, 0f);
            else
                t.localPosition = tOrigin + new Vector3(offset, 0f, 0f);

            yield return null;
        }

        if (rt != null) rt.anchoredPosition = rtOrigin;
        else            t.localPosition    = tOrigin;

        shakeCoroutine = null;
    }

    // ---------------------------------------------------------------------
    // Helpers internos (no cambian respecto al original)
    // ---------------------------------------------------------------------

    private void ClampCursor()
    {
        if (board == null) return;

        cursorPos.x = Mathf.Clamp(cursorPos.x, 0, board.width - 1);
        cursorPos.y = Mathf.Clamp(cursorPos.y, 0, board.height - 1);
    }

    private void UpdateCursorVisualPosition()
    {
        if (cursorVisual == null || board == null)
            return;

        PipeTile tile = board.GetTile(cursorPos);
        if (tile == null) return;

        // Igualamos la posición XY del cursor a la del tile usando posición mundial
        // (para que funcione esté donde esté el Canvas), pero forzamos LocalPosition.z = 0
        // para mantenerlo en el plano del Canvas. Si dejamos el Z mundial sin tocar,
        // el cursor hereda el Z del Canvas (por ejemplo -76 en Screen Space - Camera)
        // y termina con un LocalPosition.z != 0, descolocándolo del plano de la UI.
        cursorVisual.position = tile.transform.position;
        Vector3 lp = cursorVisual.localPosition;
        lp.z = 0f;
        cursorVisual.localPosition = lp;
    }

    private void CacheHeldRenderer(PipeTile tile)
    {
        heldRendererSR = tile.GetComponent<SpriteRenderer>();
        if (heldRendererSR != null)
        {
            heldOriginalColor = heldRendererSR.color;
            return;
        }

        heldRendererImg = tile.GetComponent<Image>();
        if (heldRendererImg != null)
        {
            heldOriginalColor = heldRendererImg.color;
        }
    }

    private void MakeHeldTileTransparent()
    {
        if (heldRendererSR != null)
        {
            Color c = heldRendererSR.color;
            c.a = 0f;
            heldRendererSR.color = c;
        }

        if (heldRendererImg != null)
        {
            Color c = heldRendererImg.color;
            c.a = 0f;
            heldRendererImg.color = c;
        }
    }

    private void RestoreHeldTileColor()
    {
        if (heldRendererSR != null)
        {
            heldRendererSR.color = heldOriginalColor;
        }

        if (heldRendererImg != null)
        {
            heldRendererImg.color = heldOriginalColor;
        }
    }

    private void CopyTileSpriteToHeldVisual(PipeTile tile)
    {
        if (heldVisual == null) return;

        Sprite sprite = null;
        SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
        Image img = tile.GetComponent<Image>();

        if (sr != null)
            sprite = sr.sprite;
        else if (img != null)
            sprite = img.sprite;

        if (heldVisualSR != null)
        {
            heldVisualSR.sprite = sprite;
            heldVisualSR.enabled = sprite != null;
        }

        if (heldVisualImg != null)
        {
            heldVisualImg.sprite = sprite;
            heldVisualImg.enabled = sprite != null;
            if (sprite != null)
                heldVisualImg.color = Color.white;
        }

        UpdateHeldVisualFromHeldTile();
    }

    private void UpdateHeldVisualFromHeldTile()
    {
        if (heldTile == null || heldVisual == null)
            return;

        heldVisual.rotation = heldTile.transform.rotation;
    }

    private void HideHeldVisual()
    {
        if (heldVisualSR != null)
        {
            heldVisualSR.enabled = false;
        }

        if (heldVisualImg != null)
        {
            heldVisualImg.enabled = false;
        }
    }
}
