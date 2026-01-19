using UnityEngine;
using UnityEngine.UI;

public enum TileKind
{
    Empty,
    StraightPipe,
    CurvePipe,
    Obstacle,
    StartPoint,
    EndPoint
}

public enum RotationState
{
    Deg0,
    Deg90,
    Deg180,
    Deg270
}

public enum PipeDirection
{
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3
}

[DisallowMultipleComponent]
public class PipeTile : MonoBehaviour
{
    [Header("Tile configuration")]
    public TileKind tileKind;

    [Tooltip("If true, this tile can be moved by the player.")]
    public bool canMove;

    [Range(1, 6)]
    public int gridColumn = 1;

    [Range(1, 4)]
    public int gridRow = 1;

    [Tooltip("If true, this tile can be rotated by the player.")]
    public bool canRotate;

    [Header("Rotation")]
    public RotationState rotationState = RotationState.Deg0;

    [HideInInspector]
    public Vector2Int gridPos;

    [Header("Straight pipe sprites")]
    public Sprite straightStaticSprite;
    public Sprite straightMovableSprite;
    public Sprite straightRotatableSprite;
    public Sprite straightMovableRotatableSprite;

    [Header("Curve pipe sprites")]
    public Sprite curveStaticSprite;
    public Sprite curveMovableSprite;
    public Sprite curveRotatableSprite;
    public Sprite curveMovableRotatableSprite;

    [Header("Obstacle sprite")]
    public Sprite obstacleSprite;

    [Header("Start and End sprites")]
    public Sprite startSprite;
    public Sprite endSprite;

    private Image uiImage;
    private SpriteRenderer spriteRenderer;

    private static readonly float[] columnWorldX = { -400f, -200f, 0f, 200f, 400f, 600f };
    private static readonly float[] rowWorldY = { 250f, 50f, -150f, -350f };

    private void Awake()
    {
        CacheRenderers();
        UpdateGridPositionFromInspector();
        ApplyWorldPositionFromGrid(ignoreCanMove: true);
        ApplyRotationFromState(force: true);
        ApplyVisual();
    }

    private void OnValidate()
    {
        CacheRenderers();
        UpdateGridPositionFromInspector();
        ApplyWorldPositionFromGrid(ignoreCanMove: true);
        ApplyRotationFromState(force: true);
        ApplyVisual();
    }

    private void CacheRenderers()
    {
        if (uiImage == null)
            uiImage = GetComponent<Image>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void UpdateGridPositionFromInspector()
    {
        gridPos = new Vector2Int(gridColumn - 1, gridRow - 1);
    }

    private void ApplyWorldPositionFromGrid(bool ignoreCanMove)
    {
        if (Application.isPlaying && !ignoreCanMove && !canMove)
            return;

        int colIndex = Mathf.Clamp(gridColumn - 1, 0, columnWorldX.Length - 1);
        int rowIndex = Mathf.Clamp(gridRow - 1, 0, rowWorldY.Length - 1);

        Vector3 pos = transform.localPosition;
        pos.x = columnWorldX[colIndex];
        pos.y = rowWorldY[rowIndex];
        transform.localPosition = pos;
    }

    private void ApplyRotationFromState(bool force)
    {
        if (Application.isPlaying && !force && !canRotate)
            return;

        float z = 0f;
        switch (rotationState)
        {
            case RotationState.Deg0: z = 0f; break;
            case RotationState.Deg90: z = -90f; break; 
            case RotationState.Deg180: z = -180f; break;
            case RotationState.Deg270: z = -270f; break;
        }

        transform.localEulerAngles = new Vector3(0f, 0f, z);
    }

    private void ApplyVisual()
    {
        Sprite chosen = null;

        switch (tileKind)
        {
            case TileKind.Empty:
                chosen = null;
                canMove = false;
                canRotate = false;

                if (uiImage != null)
                {
                    uiImage.sprite = null;
                    uiImage.color = new Color(1f, 1f, 1f, 0f);
                    uiImage.raycastTarget = false;
                }
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = null;
                    spriteRenderer.color = new Color(1f, 1f, 1f, 0f);
                }
                return;

            case TileKind.StraightPipe:
                chosen = GetStraightSprite();
                break;

            case TileKind.CurvePipe:
                chosen = GetCurveSprite();
                break;

            case TileKind.Obstacle:
                chosen = obstacleSprite;
                canMove = false;
                canRotate = false;
                break;

            case TileKind.StartPoint:
                chosen = startSprite;
                canMove = false;
                canRotate = false;
                break;

            case TileKind.EndPoint:
                chosen = endSprite;
                canMove = false;
                canRotate = false;
                break;
        }

        if (uiImage != null)
        {
            uiImage.sprite = chosen;
            uiImage.color = Color.white;
            uiImage.raycastTarget = true;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = chosen;
            spriteRenderer.color = Color.white;
        }
    }

    private Sprite GetStraightSprite()
    {
        if (!canMove && !canRotate)
            return straightStaticSprite;

        if (canMove && !canRotate)
            return straightMovableSprite;

        if (!canMove && canRotate)
            return straightRotatableSprite;

        return straightMovableRotatableSprite;
    }

    private Sprite GetCurveSprite()
    {
        if (!canMove && !canRotate)
            return curveStaticSprite;

        if (canMove && !canRotate)
            return curveMovableSprite;

        if (!canMove && canRotate)
            return curveRotatableSprite;

        return curveMovableRotatableSprite;
    }
    public void RotateClockwise()
    {
        if (!canRotate)
            return;

        switch (rotationState)
        {
            case RotationState.Deg0: rotationState = RotationState.Deg90; break;
            case RotationState.Deg90: rotationState = RotationState.Deg180; break;
            case RotationState.Deg180: rotationState = RotationState.Deg270; break;
            case RotationState.Deg270: rotationState = RotationState.Deg0; break;
        }

        ApplyRotationFromState(force: false);
    }

    public void SetGridPosition(int column, int row, bool ignoreCanMove = false)
    {
        if (Application.isPlaying && !ignoreCanMove && !canMove)
            return;

        gridColumn = Mathf.Clamp(column, 1, 6);
        gridRow = Mathf.Clamp(row, 1, 4);
        UpdateGridPositionFromInspector();
        ApplyWorldPositionFromGrid(ignoreCanMove);
    }

    public bool IsPipe =>
        tileKind == TileKind.StraightPipe || tileKind == TileKind.CurvePipe;

    public bool HasConnection(PipeDirection dir)
    {

        if (tileKind == TileKind.Empty || tileKind == TileKind.Obstacle)
            return false;

        if (tileKind == TileKind.StraightPipe)
        {
            bool vertical = (rotationState == RotationState.Deg0 ||
                             rotationState == RotationState.Deg180);

            if (vertical)
            {
                return dir == PipeDirection.Up || dir == PipeDirection.Down;
            }
            else
            {
                return dir == PipeDirection.Left || dir == PipeDirection.Right;
            }
        }

        if (tileKind == TileKind.CurvePipe)
        {
            switch (rotationState)
            {
                case RotationState.Deg0:
                    return dir == PipeDirection.Left || dir == PipeDirection.Down;

                case RotationState.Deg90:
                    return dir == PipeDirection.Left || dir == PipeDirection.Up;

                case RotationState.Deg180:
                    return dir == PipeDirection.Up || dir == PipeDirection.Right;

                case RotationState.Deg270:
                    return dir == PipeDirection.Down || dir == PipeDirection.Right;
            }
        }

        if (tileKind == TileKind.StartPoint || tileKind == TileKind.EndPoint)
        {
            return true;
        }

        return false;
    }

    public bool TryGetExitDirection(PipeDirection incoming, out PipeDirection exit)
    {
        exit = incoming;

        if (!IsPipe)
            return false;

        if (tileKind == TileKind.StraightPipe)
        {
            bool vertical = (rotationState == RotationState.Deg0 ||
                             rotationState == RotationState.Deg180);

            if (vertical)
            {
                if (incoming == PipeDirection.Up && HasConnection(PipeDirection.Up))
                {
                    exit = PipeDirection.Down;
                    return true;
                }
                if (incoming == PipeDirection.Down && HasConnection(PipeDirection.Down))
                {
                    exit = PipeDirection.Up;
                    return true;
                }
                return false;
            }
            else
            {
                if (incoming == PipeDirection.Left && HasConnection(PipeDirection.Left))
                {
                    exit = PipeDirection.Right;
                    return true;
                }
                if (incoming == PipeDirection.Right && HasConnection(PipeDirection.Right))
                {
                    exit = PipeDirection.Left;
                    return true;
                }
                return false;
            }
        }


        if (tileKind == TileKind.CurvePipe)
        {
            switch (rotationState)
            {
                case RotationState.Deg0:

                    if (incoming == PipeDirection.Left && HasConnection(PipeDirection.Left))
                    {
                        exit = PipeDirection.Down;
                        return true;
                    }
                    if (incoming == PipeDirection.Down && HasConnection(PipeDirection.Down))
                    {
                        exit = PipeDirection.Left;
                        return true;
                    }
                    return false;

                case RotationState.Deg90:
          
                    if (incoming == PipeDirection.Left && HasConnection(PipeDirection.Left))
                    {
                        exit = PipeDirection.Up;
                        return true;
                    }
                    if (incoming == PipeDirection.Up && HasConnection(PipeDirection.Up))
                    {
                        exit = PipeDirection.Left;
                        return true;
                    }
                    return false;

                case RotationState.Deg180:
             
                    if (incoming == PipeDirection.Up && HasConnection(PipeDirection.Up))
                    {
                        exit = PipeDirection.Right;
                        return true;
                    }
                    if (incoming == PipeDirection.Right && HasConnection(PipeDirection.Right))
                    {
                        exit = PipeDirection.Up;
                        return true;
                    }
                    return false;

                case RotationState.Deg270:
    
                    if (incoming == PipeDirection.Down && HasConnection(PipeDirection.Down))
                    {
                        exit = PipeDirection.Right;
                        return true;
                    }
                    if (incoming == PipeDirection.Right && HasConnection(PipeDirection.Right))
                    {
                        exit = PipeDirection.Down;
                        return true;
                    }
                    return false;
            }
        }

        return false;
    }
}
