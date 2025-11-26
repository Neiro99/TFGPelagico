using UnityEngine;

/// <summary>
/// Manages the logical grid of PipeTile objects and allows moving tiles
/// around the board.
/// Attach this to your "Grid" parent object.
/// </summary>
public class PuzzleBoard : MonoBehaviour
{
    [Header("Board size (must match your design)")]
    public int width = 6;   // columns (x)
    public int height = 4;  // rows (y)

    // Logical grid of tiles. Null means no tile at that cell.
    public PipeTile[,] tiles;

    private void Awake()
    {
        BuildGridFromChildren();
    }

    /// <summary>
    /// Finds all PipeTile children and fills the tiles[,] array using their gridPos.
    /// </summary>
    private void BuildGridFromChildren()
    {
        tiles = new PipeTile[width, height];

        PipeTile[] allTiles = GetComponentsInChildren<PipeTile>();
        foreach (var tile in allTiles)
        {
            Vector2Int gp = tile.gridPos;

            if (gp.x < 0 || gp.x >= width || gp.y < 0 || gp.y >= height)
            {
                Debug.LogWarning($"Tile {tile.name} has gridPos out of range: {gp}");
                continue;
            }

            if (tiles[gp.x, gp.y] != null)
            {
                Debug.LogWarning($"Cell {gp} already occupied by {tiles[gp.x, gp.y].name}, " +
                                 $"tile {tile.name} will override it.");
            }

            tiles[gp.x, gp.y] = tile;
        }
    }

    /// <summary>
    /// Returns the tile at the given grid coordinates, or null if none.
    /// Coordinates are zero-based: (0..width-1, 0..height-1).
    /// </summary>
    public PipeTile GetTile(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
            return null;

        return tiles[pos.x, pos.y];
    }

    /// <summary>
    /// Moves one tile to a new cell, updating the grid and the tile's internal
    /// gridColumn/gridRow plus its transform position.
    /// </summary>
    public void MoveTileToCell(PipeTile tile, Vector2Int newPos, bool ignoreCanMove = false)
    {
        if (tile == null) return;

        if (Application.isPlaying && !ignoreCanMove && !tile.canMove)
            return;

        Vector2Int oldPos = tile.gridPos;

        // Clear old cell
        tiles[oldPos.x, oldPos.y] = null;

        // Register in new cell
        tiles[newPos.x, newPos.y] = tile;

        // Update the tile's grid indices (1-based for SetGridPosition)
        tile.SetGridPosition(newPos.x + 1, newPos.y + 1, ignoreCanMove: true);
    }

    /// <summary>
    /// Swaps two tiles in the grid (including their positions and transforms).
    /// </summary>
    public void SwapTiles(PipeTile a, PipeTile b)
    {
        if (a == null || b == null) return;

        Vector2Int posA = a.gridPos;
        Vector2Int posB = b.gridPos;

        // Swap in the array
        tiles[posA.x, posA.y] = b;
        tiles[posB.x, posB.y] = a;

        // Update their grid indices and world positions
        a.SetGridPosition(posB.x + 1, posB.y + 1, ignoreCanMove: true);
        b.SetGridPosition(posA.x + 1, posA.y + 1, ignoreCanMove: true);
    }
}
