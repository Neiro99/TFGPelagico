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


    private Vector2Int cursorPos;


    private PipeTile heldTile = null;

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

        Vector2Int newPos = cursorPos + delta;

        newPos.x = Mathf.Clamp(newPos.x, 0, board.width - 1);
        newPos.y = Mathf.Clamp(newPos.y, 0, board.height - 1);

        if (heldTile != null)
        {
            PipeTile targetTile = board.GetTile(newPos);

            if (targetTile != null &&
                targetTile != heldTile &&
                targetTile.tileKind != TileKind.Empty)
            {
                Debug.Log("no pudo moverse");
                return;
            }
        }

        cursorPos = newPos;
        UpdateCursorVisualPosition();
    }

    private void TryRotate()
    {
        if (board == null) return;

        if (heldTile != null)
        {
            if (heldTile.canRotate)
            {
                heldTile.RotateClockwise();
                UpdateHeldVisualFromHeldTile();
            }
            return;
        }

        PipeTile tile = board.GetTile(cursorPos);
        if (tile != null && tile.canRotate)
        {
            tile.RotateClockwise();
        }
    }

    private void TryPickDrop()
    {
        if (board == null) return;

        PipeTile currentTile = board.GetTile(cursorPos);

        if (heldTile == null)
        {
            if (currentTile == null)
                return;

            if (!currentTile.canMove)
                return;

            if (currentTile.tileKind == TileKind.Empty)
                return;

            heldTile = currentTile;
            CacheHeldRenderer(heldTile);
            MakeHeldTileTransparent();
            CopyTileSpriteToHeldVisual(heldTile);
        }
        else
        {
            if (currentTile == null)
            {
                return;
            }

            if (currentTile.tileKind == TileKind.Empty)
            {
                board.SwapTiles(heldTile, currentTile);
            }


            RestoreHeldTileColor();
            heldTile = null;
            heldRendererSR = null;
            heldRendererImg = null;
            HideHeldVisual();
        }
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
