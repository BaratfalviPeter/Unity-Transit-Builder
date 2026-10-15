using UnityEngine;

/// <summary>
/// Épület/objektum lehelyezõ.
/// - Raycast-tel figyeli, hova mutat az egér a világban.
/// - Bal kattintásra példányosít egy beállított Prefabot a legközelebbi
///   rácspontra a GridManager "Snap to Grid" funkcióját használva.
///
/// Beállítás:
/// - buildingPrefab: a lehelyezendõ 3D objektum (pl. egy egyszerû kocka).
/// - gridManager: a jelenet GridManager-e (ha üresen hagyod, automatikusan megkeresi).
/// - groundLayer: csak ezekre a rétegekre találjon a Raycast (pl. a talaj).
/// </summary>
public class BuildingPlacer : MonoBehaviour
{
    [Header("Hivatkozások")]
    [Tooltip("A lehelyezendõ Prefab (pl. egy Cube).")]
    public GameObject buildingPrefab;

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
        // Bal egérgomb (0) lenyomásakor próbálunk építeni.
        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceBuilding();
        }
    }

    private void TryPlaceBuilding()
    {
        if (!ValidateReferences())
        {
            return;
        }

        Ray ray = placementCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, rayMaxDistance, groundLayer))
        {
            // A találati pontot a legközelebbi rácspontra igazítjuk.
            Vector3 snappedPosition = gridManager.GetNearestGridPoint(hit.point);

            GameObject instance = Instantiate(buildingPrefab, snappedPosition, Quaternion.identity);

            if (parentToGrid)
            {
                instance.transform.SetParent(gridManager.transform, worldPositionStays: true);
            }
        }
    }

    /// <summary>
    /// Ellenõrzi, hogy minden szükséges hivatkozás elérhetõ-e. Hiány esetén
    /// figyelmeztetést ír a konzolra, hogy könnyen beállítható legyen az Inspectorban.
    /// </summary>
    private bool ValidateReferences()
    {
        if (buildingPrefab == null)
        {
            Debug.LogWarning("[BuildingPlacer] Nincs beállítva buildingPrefab. Rendelj hozzá egy Prefabot az Inspectorban.", this);
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
