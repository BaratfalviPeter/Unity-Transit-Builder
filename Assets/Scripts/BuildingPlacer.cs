using UnityEngine;

/// <summary>
/// Objektum lehelyezõ kétféle típussal.
/// - Raycast-tel figyeli, hova mutat az egér a világban.
/// - BAL kattintás: Épület (buildingPrefab) lehelyezése.
/// - JOBB kattintás: Út (roadPrefab) lehelyezése.
/// - Mindkettõ a legközelebbi rácspontra kerül (GridManager "Snap to Grid").
/// - Egy cellán csak EGY objektum lehet (GridManager cellafoglaltság).
///
/// Beállítás:
/// - buildingPrefab / roadPrefab: a lehelyezendõ 3D objektumok.
/// - gridManager: a jelenet GridManager-e (ha üresen hagyod, automatikusan megkeresi).
/// - groundLayer: csak ezekre a rétegekre találjon a Raycast (pl. a talaj).
/// </summary>
public class BuildingPlacer : MonoBehaviour
{
    [Header("Prefabok")]
    [Tooltip("BAL kattintásra lehelyezendõ Épület Prefab.")]
    public GameObject buildingPrefab;

    [Tooltip("JOBB kattintásra lehelyezendõ Út Prefab.")]
    public GameObject roadPrefab;

    [Header("Hivatkozások")]
    [Tooltip("A rácsot kezelõ GridManager. Üresen hagyva automatikusan megkeresi.")]
    public GridManager gridManager;

    [Tooltip("A kamera, amibõl a Raycast indul. Üresen hagyva a Camera.main-t használja.")]
    public Camera placementCamera;

    [Header("Raycast beállítások")]
    [Tooltip("Mely rétegekre találjon a Raycast (pl. a talaj réteg).")]
    public LayerMask groundLayer = ~0; // alapértelmezetten minden réteg

    [Tooltip("A Raycast maximális hossza.")]
    public float rayMaxDistance = 1000f;

    [Header("Opciók")]
    [Tooltip("Igaz esetén a lehelyezett objektumokat a GridManager alá rendezi a hierarchiában.")]
    public bool parentToGrid = true;

    private void Awake()
    {
        if (placementCamera == null)
        {
            placementCamera = Camera.main;
        }

        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }
    }

    private void Update()
    {
        // Bal egérgomb (0) -> Épület
        if (Input.GetMouseButtonDown(0))
        {
            TryPlace(buildingPrefab, "Épület", Placeable.PlaceableType.Building);
        }
        // Jobb egérgomb (1) -> Út
        else if (Input.GetMouseButtonDown(1))
        {
            TryPlace(roadPrefab, "Út", Placeable.PlaceableType.Road);
        }
    }

    /// <summary>
    /// Megpróbál lehelyezni egy prefabot az egér alatti rácspontra.
    /// A cella csak akkor kap objektumot, ha szabad.
    /// </summary>
    private void TryPlace(GameObject prefab, string label, Placeable.PlaceableType type)
    {
        if (!ValidateReferences(prefab, label))
        {
            return;
        }

        Ray ray = placementCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, rayMaxDistance, groundLayer))
        {
            return;
        }

        // Cella meghatározása és foglaltság-ellenõrzés.
        Vector2Int cell = gridManager.WorldToCell(hit.point);
        if (!gridManager.IsCellFree(cell))
        {
            // A mezõ foglalt -> nem építünk ide semmit.
            return;
        }

        Vector3 snappedPosition = gridManager.GetNearestGridPoint(hit.point);
        GameObject instance = Instantiate(prefab, snappedPosition, Quaternion.identity);

        // Biztosítjuk, hogy a lerakott objektumon legyen Placeable komponens,
        // hogy a ViewManager (Tervezõ nézet) megtalálja. Ha a Prefabon már van,
        // csak a típust állítjuk be.
        Placeable placeable = instance.GetComponent<Placeable>();
        if (placeable == null)
        {
            placeable = instance.AddComponent<Placeable>();
        }
        placeable.type = type;

        if (parentToGrid)
        {
            instance.transform.SetParent(gridManager.transform, worldPositionStays: true);
        }

        // A cella megjelölése foglaltként a frissen lerakott objektummal.
        gridManager.OccupyCell(cell, instance);
    }

    /// <summary>
    /// Ellenõrzi, hogy minden szükséges hivatkozás elérhetõ-e az adott mûvelethez.
    /// </summary>
    private bool ValidateReferences(GameObject prefab, string label)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"[BuildingPlacer] Nincs beállítva a(z) '{label}' Prefab. Rendelj hozzá egyet az Inspectorban.", this);
            return false;
        }

        if (placementCamera == null)
        {
            placementCamera = Camera.main;
            if (placementCamera == null)
            {
                Debug.LogWarning("[BuildingPlacer] Nincs elérhetõ kamera (placementCamera / Camera.main).", this);
                return false;
            }
        }

        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
            if (gridManager == null)
            {
                Debug.LogWarning("[BuildingPlacer] Nincs GridManager a jelenetben.", this);
                return false;
            }
        }

        return true;
    }
}
