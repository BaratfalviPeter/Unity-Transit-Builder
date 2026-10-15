using UnityEngine;

/// <summary>
/// Jelölõ komponens a lehelyezhetõ objektumokhoz (Épület, Út).
/// Tedd rá a Building és a Road Prefabra is.
///
/// Két szerepe van:
/// 1. A ViewManager ez alapján találja meg az összes lehelyezett objektumot
///    (nem kell név vagy tag szerint keresgélni).
/// 2. Opcionálisan megkülönböztethetõ a típus (Building / Road), ha késõbb
///    típusfüggõ logika kell.
/// </summary>
public class Placeable : MonoBehaviour
{
    public enum PlaceableType
    {
        Building,
        Road
    }

    [Tooltip("Az objektum típusa (csak információ / késõbbi logikához).")]
    public PlaceableType type = PlaceableType.Building;
}
