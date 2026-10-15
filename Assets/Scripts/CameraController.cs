using UnityEngine;

/// <summary>
/// RTS-stílusú kamera vezérlő.
/// - WASD / nyílbillentyűk: mozgatás
/// - Egér a képernyő szélén (Edge Pan): mozgatás
/// - Egérgörgő: zoom (a kamera előre/hátra mozog a nézeti irány mentén)
/// - Középső egérgomb nyomva tartása + egér mozgatása: forgatás az Y tengely körül
///
/// Javasolt beállítás: tedd ezt a szkriptet a Main Camera objektumra (vagy annak
/// egy szülő "CameraRig" objektumára). A kamera egy enyhén lefelé nézõ szögben
/// legyen beállítva (pl. X rotáció = 45-60 fok).
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Mozgatás (Pan)")]
    [Tooltip("A kamera vízszintes mozgási sebessége egységben/másodperc.")]
    public float panSpeed = 20f;

    [Tooltip("Igaz esetén a mozgási sebesség arányosan nõ a zoom magassággal.")]
    public bool scaleSpeedWithZoom = true;

    [Header("Edge Pan (képernyõ szél)")]
    [Tooltip("Engedélyezi a mozgatást, ha az egér a képernyõ széléhez közelít.")]
    public bool enableEdgePan = true;

    [Tooltip("A képernyõ szélétõl mért sáv vastagsága pixelben, ami aktiválja az edge pant.")]
    public float edgePanBorderThickness = 10f;

    [Header("Zoom")]
    [Tooltip("A zoom sebessége (görgõ érzékenység).")]
    public float zoomSpeed = 500f;

    [Tooltip("A kamera minimális magassága (legközelebbi zoom).")]
    public float minZoomHeight = 5f;

    [Tooltip("A kamera maximális magassága (legtávolabbi zoom).")]
    public float maxZoomHeight = 80f;

    [Header("Forgatás")]
    [Tooltip("Forgatási sebesség (fok/pixel) középsõ egérgomb lenyomásakor.")]
    public float rotationSpeed = 3f;

    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponentInChildren<Camera>();
        if (_cam == null)
        {
            _cam = Camera.main;
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleZoom();
    }

    /// <summary>
    /// WASD / nyílbillentyûk + Edge Pan kezelése.
    /// A mozgás a kamera horizontális (XZ síkra vetített) irányához igazodik,
    /// így a forgatás után is intuitív marad.
    /// </summary>
    private void HandleMovement()
    {
        Vector3 direction = Vector3.zero;

        // Billentyûzet
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D, bal/jobb
        float vertical = Input.GetAxisRaw("Vertical");     // W/S, fel/le

        direction.x += horizontal;
        direction.z += vertical;

        // Edge Pan
        if (enableEdgePan && Application.isFocused)
        {
            Vector3 mousePos = Input.mousePosition;

            if (mousePos.x >= 0 && mousePos.x <= Screen.width &&
                mousePos.y >= 0 && mousePos.y <= Screen.height)
            {
                if (mousePos.x <= edgePanBorderThickness) direction.x -= 1f;
                if (mousePos.x >= Screen.width - edgePanBorderThickness) direction.x += 1f;
                if (mousePos.y <= edgePanBorderThickness) direction.z -= 1f;
                if (mousePos.y >= Screen.height - edgePanBorderThickness) direction.z += 1f;
            }
        }

        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        // A kamera saját irányához igazított mozgás, de XZ síkra laposítva.
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 move = (right * direction.x + forward * direction.z).normalized;

        float speed = panSpeed;
        if (scaleSpeedWithZoom)
        {
            // Magasabban gyorsabb mozgás a kényelmesebb navigációért.
            speed *= Mathf.Max(1f, transform.position.y / minZoomHeight);
        }

        transform.position += move * speed * Time.deltaTime;
    }

    /// <summary>
    /// Középsõ egérgomb nyomva tartása közben az egér vízszintes mozgása
    /// forgatja a kamerát az Y (függõleges) tengely körül.
    /// </summary>
    private void HandleRotation()
    {
        if (Input.GetMouseButton(2)) // 2 = középsõ egérgomb
        {
            float mouseX = Input.GetAxis("Mouse X");
            transform.Rotate(Vector3.up, mouseX * rotationSpeed, Space.World);
        }
    }

    /// <summary>
    /// Egérgörgõvel zoom: a kamera a saját elõre-irányába mozog,
    /// a magasságot pedig a min/max határok közé szorítjuk.
    /// </summary>
    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Approximately(scroll, 0f))
        {
            return;
        }

        Vector3 pos = transform.position;
        pos += transform.forward * scroll * zoomSpeed * Time.deltaTime;

        // Magasság korlátozása
        pos.y = Mathf.Clamp(pos.y, minZoomHeight, maxZoomHeight);

        transform.position = pos;
    }
}
