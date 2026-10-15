using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Nézetváltó: normál <-> "Tervezõ nézet".
/// - A Tab gomb megnyomására minden lehelyezett objektum (Placeable) renderereinek
///   anyaga egységes sötétszürke "blueprint" anyagra vált.
/// - Újabb Tab megnyomásra mindegyik visszakapja az EREDETI anyagait.
///
/// Megközelítés:
/// - A váltáskor elmentjük minden érintett Renderer eredeti anyagtömbjét
///   (sharedMaterials), így több renderer / több anyag esetén is pontos a visszaállítás.
/// - A Placeable komponens alapján találjuk meg az objektumokat, ezért tedd rá
///   a Building és a Road Prefabra is.
/// </summary>
public class ViewManager : MonoBehaviour
{
    [Header("Tervezõ nézet anyaga")]
    [Tooltip("Az egységes sötétszürke 'blueprint' anyag. Hozd létre a Unity-ben és húzd ide.")]
    public Material blueprintMaterial;

    [Header("Beállítások")]
    [Tooltip("A nézetváltás billentyûje.")]
    public KeyCode toggleKey = KeyCode.Tab;

    /// <summary>Igaz, ha jelenleg a Tervezõ nézet aktív.</summary>
    public bool IsBlueprintView { get; private set; }

    // Az eredeti anyagok mentése: Renderer -> eredeti sharedMaterials tömb.
    private readonly Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleView();
        }
    }

    /// <summary>
    /// Átvált a két nézet között.
    /// </summary>
    public void ToggleView()
    {
        if (IsBlueprintView)
        {
            RestoreOriginalMaterials();
        }
        else
        {
            ApplyBlueprintView();
        }

        IsBlueprintView = !IsBlueprintView;
    }

    /// <summary>
    /// Minden lehelyezett objektum renderereit sötétszürke anyagra váltja,
    /// miután elmentette az eredeti anyagokat.
    /// </summary>
    private void ApplyBlueprintView()
    {
        if (blueprintMaterial == null)
        {
            Debug.LogWarning("[ViewManager] Nincs beállítva blueprintMaterial. Rendelj hozzá egy Material-t az Inspectorban.", this);
            return;
        }

        _originalMaterials.Clear();

        // Az összes aktív Placeable megkeresése a jelenetben.
        Placeable[] placeables = FindObjectsOfType<Placeable>();

        foreach (Placeable placeable in placeables)
        {
            // Egy objektumnak lehet több rendererje (gyerekek is), mindet kezeljük.
            Renderer[] renderers = placeable.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || _originalMaterials.ContainsKey(renderer))
                {
                    continue;
                }

                // Eredeti anyagok mentése (sharedMaterials, hogy ne hozzunk létre példányokat).
                _originalMaterials[renderer] = renderer.sharedMaterials;

                // Egységes blueprint anyag minden anyag-slotra.
                Material[] blueprintArray = new Material[renderer.sharedMaterials.Length];
                for (int i = 0; i < blueprintArray.Length; i++)
                {
                    blueprintArray[i] = blueprintMaterial;
                }

                renderer.sharedMaterials = blueprintArray;
            }
        }
    }

    /// <summary>
    /// Visszaállítja az elmentett eredeti anyagokat.
    /// </summary>
    private void RestoreOriginalMaterials()
    {
        foreach (KeyValuePair<Renderer, Material[]> entry in _originalMaterials)
        {
            // A renderer közben törlõdhetett (pl. objektum eltávolítva).
            if (entry.Key != null)
            {
                entry.Key.sharedMaterials = entry.Value;
            }
        }

        _originalMaterials.Clear();
    }
}
