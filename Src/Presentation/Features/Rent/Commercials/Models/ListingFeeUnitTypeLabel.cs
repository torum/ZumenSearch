using ZumenSearch.Domain.Entities.Rent.Commercials.Listing;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Models;

public sealed class ListingFeeUnitTypeLabel(ListingFeeUnitType key)
{
    private static readonly Dictionary<ListingFeeUnitType, string> Labels = new()
    {
        [ListingFeeUnitType.Months] = "ヵ月",
        [ListingFeeUnitType.Yen] = "円"
    };

    public string Label => Labels[Key];
    public ListingFeeUnitType Key => key;

    public static IReadOnlyList<ListingFeeUnitTypeLabel> GetAll() => Enum.GetValues<ListingFeeUnitType>()
        .Select(value => new ListingFeeUnitTypeLabel(value))
        .ToArray();

    public static ListingFeeUnitType ParseFeeUnitType(string? value) =>
        Enum.TryParse<ListingFeeUnitType>(value, out var unit) && Enum.IsDefined(unit)
            ? unit
            : ListingFeeUnitType.Months;
}
