using UnityEngine;

/// <summary>
/// Checks if there is a continuous valid path of pipes
/// from StartPoint to EndPoint.
/// </summary>
public class PuzzleChecker : MonoBehaviour
{
    public PuzzleBoard board;

    private void Start()
    {
        if (board == null)
        {
#if UNITY_2023_1_OR_NEWER
            board = Object.FindFirstObjectByType<PuzzleBoard>();
#else
            board = FindObjectOfType<PuzzleBoard>();
#endif
        }
    }

    private void Update()
    {
        // Press Enter to test
        if (Input.GetKeyDown(KeyCode.Return))
        {
            bool solved = IsSolved();
            Debug.Log("Puzzle solved? " + solved);
            if (solved)
            {
                ChangeSceneManager.instance.nextSceneInsdex = 4;
                ChangeSceneManager.instance.typeOfFade = "StandarFade";
                GameManager.instance.ChangeState(DataDefinitions.GameStates.ChangeScene);
            }
        }
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

        // Check the 4 neighbours around Start
        foreach (PipeDirection dirFromStart in System.Enum.GetValues(typeof(PipeDirection)))
        {
            Vector2Int delta = DirectionToDelta(dirFromStart);
            Vector2Int neighbourPos = startPos + delta;

            PipeTile neighbour = board.GetTile(neighbourPos);
            if (neighbour == null || !neighbour.IsPipe)
                continue;

            // The side of the neighbour that faces Start
            PipeDirection sideFacingStart = Opposite(dirFromStart);

            // That neighbour must have an opening facing Start
            if (!neighbour.HasConnection(sideFacingStart))
                continue;

            // We enter the neighbour through sideFacingStart
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

    /// <summary>
    /// Follows the path starting from 'current' (first pipe after Start),
    /// with the flow entering from 'incomingDir'.
    /// Returns true if we reach 'endTile' with valid connections.
    /// </summary>
    private bool FollowPath(PipeTile current, PipeDirection incomingDir, PipeTile endTile)
    {
        int maxSteps = board.width * board.height * 4;

        PipeTile tile = current;
        PipeDirection entryDir = incomingDir;

        for (int step = 0; step < maxSteps; step++)
        {
            // If we reached End, success
            if (tile == endTile)
                return true;

            // Get exit direction given the entry direction
            PipeDirection exitDir;
            if (!tile.TryGetExitDirection(entryDir, out exitDir))
                return false;

            // Move to next cell in exitDir
            Vector2Int delta = DirectionToDelta(exitDir);
            Vector2Int nextPos = tile.gridPos + delta;
            PipeTile nextTile = board.GetTile(nextPos);

            if (nextTile == null)
                return false;

            // If next is End, we are done (we already checked above on next loop,
            // but podemos hacerlo aquí también por claridad)
            if (nextTile == endTile)
            {
                // Check that End "accepts" a connection from this side
                PipeDirection sideIntoEnd = Opposite(exitDir);
                if (endTile.HasConnection(sideIntoEnd))
                    return true;
                else
                    return false;
            }

            // Next tile must be a pipe and must have an opening facing back
            if (!nextTile.IsPipe)
                return false;

            PipeDirection sideFacingBack = Opposite(exitDir);
            if (!nextTile.HasConnection(sideFacingBack))
                return false;

            // Advance
            tile = nextTile;
            entryDir = sideFacingBack;
        }

        // loop guard
        return false;
    }

    private Vector2Int DirectionToDelta(PipeDirection dir)
    {
        // Grid: x derecha, y hacia abajo
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
