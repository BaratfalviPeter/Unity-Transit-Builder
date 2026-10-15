using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 3D rács kezelõ.
/// - Beállítható cellaméret (cellSize) és rácsméret.
/// - Gizmos segítségével kirajzolja a rácsot a Scene nézetben.
/// - Snap to Grid: a megadott világkoordinátát a legközelebbi rácspontra igazítja.
/// - Cellafoglaltság-nyilvántartás: egy mezõn csak egy objektum lehet.
///
/// A rács az XZ síkon fekszik (a logisztikai/építõ játékoknál szokásos "talaj" sík),
/// a GridManager objektum pozíciója adja a rács origóját.
/// </summary>
public class GridManager : MonoBehaviour
{
    /// <summary>
    /// A foglalt cellák nyilvántartása: cella-index -> a cellán álló objektum.
    /// Így O(1)-ben eldönthetõ, hogy egy mezõ szabad-e.
    /// </summary>
    private readonly Dictionary<Vector2Int, GameObject> _occupiedCells = new Dictionary<Vector2Int, GameObject>();

    [Header("Rács beállítások")]
    [Tooltip("Egy cella mérete világegységben.")]
    [Min(0.01f)]
    public float cellSize = 1f;

    [Tooltip("A rács szélessége cellákban (X tengely).")]
    [Min(1)]
    public int gridWidth = 20;

    [Tooltip("A rács hossza cellákban (Z tengely).")]
    [Min(1)]
    public int gridLength = 20;

    [Header("Gizmos megjelenítés")]
    [Tooltip("Rács kirajzolása a Scene nézetben.")]
    public bool drawGizmos = true;

    [Tooltip("A rácsvonalak színe.")]
    public Color gridColor = new Color(1f, 1f, 1f, 0.4f);

    /// <summary>A rács origója világkoordinátában (a GridManager pozíciója).</summary>
    public Vector3 Origin => transform.position;

    /// <summary>
    /// Egy tetszõleges világkoordinátát a legközelebbi rácspontra igazít (Snap to Grid).
    /// Az Y koordinátát a rács origójának Y értékére állítja.
    /// </summary>
    /// <param name="worldPosition">A bemeneti világkoordináta (pl. Raycast találat).</param>
    /// <returns>A legközelebbi rácspont világkoordinátája.</returns>
    public Vector3 GetNearestGridPoint(Vector3 worldPosition)
    {
        Vector3 local = worldPosition - Origin;

        float snappedX = Mathf.Round(local.x / cellSize) * cellSize;
        float snappedZ = Mathf.Round(local.z / cellSize) * cellSize;

        return new Vector3(
            Origin.x + snappedX,
            Origin.y,
            Origin.z + snappedZ
        );
    }

    /// <summary>
    /// Világkoordinátát rács-cella indexre (oszlop, sor) alakít.
    /// Hasznos, ha késõbb logikai térképet (pl. foglaltság) szeretnél tárolni.
    /// </summary>
    public Vector2Int WorldToCell(Vector3 worldPosition)
    {
        Vector3 local = worldPosition - Origin;
        int x = Mathf.RoundToInt(local.x / cellSize);
        int z = Mathf.RoundToInt(local.z / cellSize);
        return new Vector2Int(x, z);
    }

    /// <summary>
    /// Rács-cella indexbõl világkoordinátát ad vissza (a rácspont középpontja).
    /// </summary>
    public Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(
            Origin.x + cell.x * cellSize,
            Origin.y,
            Origin.z + cell.y * cellSize
        );
    }

    // --- Cellafoglaltság ---

    /// <summary>
    /// Igaz, ha a megadott cella szabad (nincs rajta objektum).
    /// </summary>
    public bool IsCellFree(Vector2Int cell)
    {
        return !_occupiedCells.ContainsKey(cell);
    }

    /// <summary>
    /// Igaz, ha a megadott világkoordinátához tartozó cella szabad.
    /// </summary>
    public bool IsWorldPositionFree(Vector3 worldPosition)
    {
        return IsCellFree(WorldToCell(worldPosition));
    }

    /// <summary>
    /// Megjelöl egy cellát foglaltként a hozzá tartozó objektummal.
    /// Visszatér hamissal, ha a cella már foglalt volt (nem írja felül).
    /// </summary>
    public bool OccupyCell(Vector2Int cell, GameObject occupant)
    {
        if (_occupiedCells.ContainsKey(cell))
        {
            return false;
        }

        _occupiedCells[cell] = occupant;
        return true;
    }

    /// <summary>
    /// Felszabadít egy cellát (pl. ha az objektumot töröljük). Igaz, ha volt mit felszabadítani.
    /// </summary>
    public bool FreeCell(Vector2Int cell)
    {
        return _occupiedCells.Remove(cell);
    }

    /// <summary>
    /// Visszaadja a cellán álló objektumot, vagy null-t, ha a cella szabad.
    /// </summary>
    public GameObject GetOccupant(Vector2Int cell)
    {
        _occupiedCells.TryGetValue(cell, out GameObject occupant);
        return occupant;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || cellSize <= 0f)
        {
            return;
        }

        Gizmos.color = gridColor;

        float width = gridWidth * cellSize;
        float length = gridLength * cellSize;

        Vector3 origin = Origin;

        // Vonalak az X tengely mentén (Z pozíciónkénti vonalak)
        for (int z = 0; z <= gridLength; z++)
        {
            Vector3 start = origin + new Vector3(0f, 0f, z * cellSize);
            Vector3 end = start + new Vector3(width, 0f, 0f);
            Gizmos.DrawLine(start, end);
        }

        // Vonalak a Z tengely mentén (X pozíciónkénti vonalak)
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 start = origin + new Vector3(x * cellSize, 0f, 0f);
            Vector3 end = start + new Vector3(0f, 0f, length);
            Gizmos.DrawLine(start, end);
        }
    }
}
