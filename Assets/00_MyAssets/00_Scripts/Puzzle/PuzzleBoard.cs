using UnityEngine;

public class PuzzleBoard : MonoBehaviour
{
    [Header("Board size")]
    public int width = 6; 
    public int height = 4; 


    public PipeTile[,] tiles;

    private void Awake()
    {
        BuildGridFromChildren();
    }

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

    public PipeTile GetTile(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
            return null;

        return tiles[pos.x, pos.y];
    }

    public void MoveTileToCell(PipeTile tile, Vector2Int newPos, bool ignoreCanMove = false)
    {
        if (tile == null) return;

        if (Application.isPlaying && !ignoreCanMove && !tile.canMove)
            return;

        Vector2Int oldPos = tile.gridPos;

        tiles[oldPos.x, oldPos.y] = null;

        tiles[newPos.x, newPos.y] = tile;

        tile.SetGridPosition(newPos.x + 1, newPos.y + 1, ignoreCanMove: true);
    }

    public void SwapTiles(PipeTile a, PipeTile b)
    {
        if (a == null || b == null) return;

        Vector2Int posA = a.gridPos;
        Vector2Int posB = b.gridPos;

        tiles[posA.x, posA.y] = b;
        tiles[posB.x, posB.y] = a;

        a.SetGridPosition(posB.x + 1, posB.y + 1, ignoreCanMove: true);
        b.SetGridPosition(posA.x + 1, posA.y + 1, ignoreCanMove: true);
    }
}
