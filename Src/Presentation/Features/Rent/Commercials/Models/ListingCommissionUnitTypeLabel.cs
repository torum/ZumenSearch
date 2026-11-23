using ZumenSearch.Domain.Entities.Rent.Commercials.Listing;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Models;

public sealed class ListingCommissionUnitTypeLabel(ListingCommissionUnitType key)
{
    private static readonly Dictionary<ListingCommissionUnitType, string> Labels = new()
    {
        [ListingCommissionUnitType.Percent] = "％",
        [ListingCommissionUnitType.Yen] = "円"
    };

    public string Label => Labels[Key];
    public ListingCommissionUnitType Key => key;

    public static IReadOnlyList<ListingCommissionUnitTypeLabel> GetAll() => Enum.GetValues<ListingCommissionUnitType>()
        .Select(value => new ListingCommissionUnitTypeLabel(value))
        .ToArray();
}
