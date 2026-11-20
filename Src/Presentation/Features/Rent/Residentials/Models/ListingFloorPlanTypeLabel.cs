using ZumenSearch.Domain.Entities.Rent.Residentials;

namespace ZumenSearch.Presentation.Features.Rent.Residentials.Models;

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
