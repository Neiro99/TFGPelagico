using UnityEngine;

public class PuzzleChecker : MonoBehaviour
{
    public PuzzleBoard board;
    public GameObject puzzleRoot;
    public GameObject background;
    private void Start()
    {
        if (board == null)
        {
            board = Object.FindFirstObjectByType<PuzzleBoard>();
        }
    }

    private void OnEnable()
    {
        // Nos suscribimos al evento "Aceptar" (Return → SelectPressedEvent).
        // El mapa UI está activo durante el estado Puzzle (InputManager.OnPuzzle
        // → OnUI), así que este evento sí se dispara mientras estamos en el puzzle.
        InputManager.SelectPressedEvent += TrySolve;
    }

    private void OnDisable()
    {
        InputManager.SelectPressedEvent -= TrySolve;
    }

    private void TrySolve()
    {
        bool solved = IsSolved();
        if (!solved) return;

        puzzleRoot.SetActive(false);
        background.SetActive(false);
        ChangeSceneManager.instance.nextSceneInsdex = 4;
        ChangeSceneManager.instance.typeOfFade = "StandarFade";
        GameManager.instance.ChangeState(DataDefinitions.GameStates.ChangeScene);
    }

    public bool IsSolved()
    {
        if (board == null || board.tiles == null)
        {
            Debug.LogWarning("PuzzleChecker: board or tiles not set.");
            return false;
        }

        PipeTile start = FindTileOfKind(TileKind.StartPoint);
        PipeTile end = FindTileOfKind(TileKind.EndPoint);

        if (start == null || end == null)
        {
            Debug.LogWarning("PuzzleChecker: StartPoint or EndPoint not found.");
            return false;
        }

        Vector2Int startPos = start.gridPos;

        foreach (PipeDirection dirFromStart in System.Enum.GetValues(typeof(PipeDirection)))
        {
            Vector2Int delta = DirectionToDelta(dirFromStart);
            Vector2Int neighbourPos = startPos + delta;

            PipeTile neighbour = board.GetTile(neighbourPos);
            if (neighbour == null || !neighbour.IsPipe)
                continue;


            PipeDirection sideFacingStart = Opposite(dirFromStart);

            if (!neighbour.HasConnection(sideFacingStart))
                continue;

            if (FollowPath(neighbour, sideFacingStart, end))
                return true;
        }

        return false;
    }

    private PipeTile FindTileOfKind(TileKind kind)
    {
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                var t = board.tiles[x, y];
                if (t != null && t.tileKind == kind)
                    return t;
            }
        }
        return null;
    }

    private bool FollowPath(PipeTile current, PipeDirection incomingDir, PipeTile endTile)
    {
        int maxSteps = board.width * board.height * 4;

        PipeTile tile = current;
        PipeDirection entryDir = incomingDir;

        for (int step = 0; step < maxSteps; step++)
        {
            if (tile == endTile)
                return true;

            PipeDirection exitDir;
            if (!tile.TryGetExitDirection(entryDir, out exitDir))
                return false;
            Vector2Int delta = DirectionToDelta(exitDir);
            Vector2Int nextPos = tile.gridPos + delta;
            PipeTile nextTile = board.GetTile(nextPos);

            if (nextTile == null)
                return false;

           
            if (nextTile == endTile)
            {
            
                PipeDirection sideIntoEnd = Opposite(exitDir);
                if (endTile.HasConnection(sideIntoEnd))
                    return true;
                else
                    return false;
            }

            if (!nextTile.IsPipe)
                return false;

            PipeDirection sideFacingBack = Opposite(exitDir);
            if (!nextTile.HasConnection(sideFacingBack))
                return false;


            tile = nextTile;
            entryDir = sideFacingBack;
        }

        return false;
    }

    private Vector2Int DirectionToDelta(PipeDirection dir)
    {
        switch (dir)
        {
            case PipeDirection.Up: return new Vector2Int(0, -1);
            case PipeDirection.Right: return new Vector2Int(1, 0);
            case PipeDirection.Down: return new Vector2Int(0, 1);
            case PipeDirection.Left: return new Vector2Int(-1, 0);
        }
        return Vector2Int.zero;
    }

    private PipeDirection Opposite(PipeDirection dir)
    {
        switch (dir)
        {
            case PipeDirection.Up: return PipeDirection.Down;
            case PipeDirection.Right: return PipeDirection.Left;
            case PipeDirection.Down: return PipeDirection.Up;
            case PipeDirection.Left: return PipeDirection.Right;
        }
        return PipeDirection.Up;
    }
}
