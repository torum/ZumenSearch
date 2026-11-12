namespace ZumenSearch.Models.Rent.Residentials;

public enum FloorPlanExtraType { Unspecified, Loft, Maisonette, Studio }

public sealed class ListingFloorPlanExtraTypeLabel(FloorPlanExtraType key)
{
    private static readonly Dictionary<FloorPlanExtraType, string> Labels = new()
    {
        [FloorPlanExtraType.Unspecified] = "指定なし",
        [FloorPlanExtraType.Loft] = "ロフト付き",
        [FloorPlanExtraType.Maisonette] = "メゾネット",
        [FloorPlanExtraType.Studio] = "スタジオタイプ"
    };

    public string Label => Labels[Key];

    public FloorPlanExtraType Key => key;

    public static IReadOnlyList<ListingFloorPlanExtraTypeLabel> GetAll() => Enum.GetValues<FloorPlanExtraType>()
        .Select(value => new ListingFloorPlanExtraTypeLabel(value))
        .ToArray();

    public static FloorPlanExtraType ParseFloorPlanExtraType(string? value) =>
    Enum.TryParse<FloorPlanExtraType>(value, out var floorPlanExtraType) && Enum.IsDefined(floorPlanExtraType)
    ? floorPlanExtraType
    : FloorPlanExtraType.Unspecified;
}
