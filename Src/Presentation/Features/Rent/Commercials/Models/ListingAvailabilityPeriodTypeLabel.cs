using ZumenSearch.Domain.Entities.Rent.Commercials.Listing;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Models;

public sealed class ListingAvailabilityPeriodTypeLabel(ListingAvailabilityPeriodType key)
{
    private static readonly Dictionary<ListingAvailabilityPeriodType, string> Labels = new()
    {
        [ListingAvailabilityPeriodType.Unspecified] = "未指定",
        [ListingAvailabilityPeriodType.Early] = "上旬",
        [ListingAvailabilityPeriodType.Middle] = "中旬",
        [ListingAvailabilityPeriodType.Late] = "下旬"
    };

    public string Label => Labels[Key];

    public ListingAvailabilityPeriodType Key => key;

    public static IReadOnlyList<ListingAvailabilityPeriodTypeLabel> GetAll() => Enum.GetValues<ListingAvailabilityPeriodType>()
        .Select(value => new ListingAvailabilityPeriodTypeLabel(value))
        .ToArray();

    public static ListingAvailabilityPeriodType ParseAvailabilityPeriod(string? value) =>
        Enum.TryParse<ListingAvailabilityPeriodType>(value, out var period) && string.Equals(Enum.GetName(period), value, StringComparison.Ordinal)
            ? period
            : ListingAvailabilityPeriodType.Unspecified;
}
