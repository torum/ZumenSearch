using ZumenSearch.Domain.Entities.Rent.Commercials.Listing;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Models;

public sealed class ListingAvailabilityMonthTypeLabel(ListingAvailabilityMonthType key)
{
    private static readonly Dictionary<ListingAvailabilityMonthType, string> Labels = new()
    {
        [ListingAvailabilityMonthType.Unspecified] = "未指定",
        [ListingAvailabilityMonthType.January] = "１月",
        [ListingAvailabilityMonthType.February] = "２月",
        [ListingAvailabilityMonthType.March] = "３月",
        [ListingAvailabilityMonthType.April] = "４月",
        [ListingAvailabilityMonthType.May] = "５月",
        [ListingAvailabilityMonthType.June] = "６月",
        [ListingAvailabilityMonthType.July] = "７月",
        [ListingAvailabilityMonthType.August] = "８月",
        [ListingAvailabilityMonthType.September] = "９月",
        [ListingAvailabilityMonthType.October] = "１０月",
        [ListingAvailabilityMonthType.November] = "１１月",
        [ListingAvailabilityMonthType.December] = "１２月"
    };

    public string Label => Labels[Key];

    public ListingAvailabilityMonthType Key => key;

    public static IReadOnlyList<ListingAvailabilityMonthTypeLabel> GetAll() => Enum.GetValues<ListingAvailabilityMonthType>()
        .Select(value => new ListingAvailabilityMonthTypeLabel(value))
        .ToArray();

    public static ListingAvailabilityMonthType ParseAvailabilityMonth(string? value) =>
        Enum.TryParse<ListingAvailabilityMonthType>(value, out var month) && string.Equals(Enum.GetName(month), value, StringComparison.Ordinal)
            ? month
            : ListingAvailabilityMonthType.Unspecified;
}
