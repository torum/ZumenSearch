namespace ZumenSearch.Models.Rent.Residentials;

public enum FloorPlanType
{
    Unspecified,
    // 単身向け
    OneR,       // 1R（ワンルーム）
    OneK,       // 1K
    OneDK,      // 1DK
    OneLDK,     // 1LDK

    // 二人暮らし〜小世帯向け
    TwoK,       // 2K
    TwoDK,      // 2DK
    TwoLDK,     // 2LDK

    // ファミリー向け
    ThreeK,     // 3K
    ThreeDK,    // 3DK
    ThreeLDK,   // 3LDK
    FourLDK,    // 4LDK
    FiveLDK,    // 5LDK
    SixLDK,     // 6LDK

    // S（サービスルーム）付き
    OneSLDK,    // 1SLDK
    TwoSLDK,    // 2SLDK
    ThreeSLDK,  // 3SLDK
    FourSLDK,   // 4SLDK
}

public sealed class ListingFloorPlanTypeLabel(FloorPlanType key)
{
    private static readonly Dictionary<FloorPlanType, string> Labels = new()
    {
        [FloorPlanType.Unspecified] = "未指定",
        [FloorPlanType.OneR] = "1R",
        [FloorPlanType.OneK] = "1K",
        [FloorPlanType.OneDK] = "1DK",
        [FloorPlanType.OneLDK] = "1LDK",
        [FloorPlanType.TwoK] = "2K",
        [FloorPlanType.TwoDK] = "2DK",
        [FloorPlanType.TwoLDK] = "2LDK",
        [FloorPlanType.ThreeK] = "3K",
        [FloorPlanType.ThreeDK] = "3DK",
        [FloorPlanType.ThreeLDK] = "3LDK",
        [FloorPlanType.FourLDK] = "4LDK",
        [FloorPlanType.FiveLDK] = "5LDK",
        [FloorPlanType.SixLDK] = "6LDK",
        [FloorPlanType.OneSLDK] = "1SLDK",
        [FloorPlanType.TwoSLDK] = "2SLDK",
        [FloorPlanType.ThreeSLDK] = "3SLDK",
        [FloorPlanType.FourSLDK] = "4SLDK"
    };

    public string Label => Labels[Key];

    public FloorPlanType Key => key;

    public static IReadOnlyList<ListingFloorPlanTypeLabel> GetAll() => Enum.GetValues<FloorPlanType>()
        .Select(value => new ListingFloorPlanTypeLabel(value))
        .ToArray();

    public static FloorPlanType ParseFloorPlanType(string? value) =>
        Enum.TryParse<FloorPlanType>(value, out var floorPlanType) && Enum.IsDefined(floorPlanType)
            ? floorPlanType
            : FloorPlanType.Unspecified;
}
