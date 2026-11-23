using ZumenSearch.Domain.Entities.Rent.Commercials.Listing;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Models;

public sealed class ListingFloorGroundTypeLabel(ListingFloorGroundType key)
{
    private static readonly Dictionary<ListingFloorGroundType, string> Labels = new()
    {
        [ListingFloorGroundType.AboveGround] = "地上",
        [ListingFloorGroundType.BelowGround] = "地下"
    };

    public string Label => Labels[Key];

    public ListingFloorGroundType Key => key;

    public static IReadOnlyList<ListingFloorGroundTypeLabel> GetAll() => Enum.GetValues<ListingFloorGroundType>()
        .Select(value => new ListingFloorGroundTypeLabel(value))
        .ToArray();

    public static ListingFloorGroundType ParseFloorGroundType(string? value) =>
        Enum.TryParse<ListingFloorGroundType>(value, out var floor) && string.Equals(Enum.GetName(floor), value, StringComparison.Ordinal)
            ? floor
            : ListingFloorGroundType.AboveGround;
}
