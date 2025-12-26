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
        InputManager.Instance.canMove = false;
    }

    private void Update()
    {
        HandleMovementInput();
        HandleRotateInput();
        HandlePickDropInput();
    }

    private void HandleMovementInput()
    {
        Vector2Int delta = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            delta += Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            delta += Vector2Int.right;
      
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            delta += Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            delta += Vector2Int.up;

        if (delta == Vector2Int.zero)
            return;

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


    private void HandleRotateInput()
    {
        if (!Input.GetKeyDown(KeyCode.G))
            return;

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

    private void HandlePickDropInput()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

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
        if (tile != null)
        {
            Vector3 p = tile.transform.position;
            p.z += cursorZOffset;
            cursorVisual.position = p;
        }
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
