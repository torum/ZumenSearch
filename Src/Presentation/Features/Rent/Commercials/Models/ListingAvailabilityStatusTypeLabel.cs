using ZumenSearch.Domain.Entities.Rent.Commercials.Listing;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Models;

public sealed class ListingAvailabilityStatusTypeLabel(ListingAvailabilityStatusType key)
{
    private Dictionary<ListingAvailabilityStatusType, string> ListingAvailabilityStatusTypeDictionary { get; } = new()
    {
        { ListingAvailabilityStatusType.Unspecified, "未指定" },
        { ListingAvailabilityStatusType.ScheduledVacating, "退去予定" },
        { ListingAvailabilityStatusType.Vacant, "空室" },
        { ListingAvailabilityStatusType.ApplicationSubmitted, "申込あり" },
        { ListingAvailabilityStatusType.ContractedAndOccupied, "契約済み・入居中" },
        { ListingAvailabilityStatusType.OtherOrUnknown, "その他・不明" },
    };

    public string Label => ListingAvailabilityStatusTypeDictionary[Key];

    public ListingAvailabilityStatusType Key => key;

    public static IReadOnlyList<ListingAvailabilityStatusTypeLabel> GetAll() => Enum.GetValues<ListingAvailabilityStatusType>()
        .Select(value => new ListingAvailabilityStatusTypeLabel(value))
        .ToArray();

    public static ListingAvailabilityStatusType ParseCurrentStatus(string? value) =>
        Enum.TryParse<ListingAvailabilityStatusType>(value, out var status) && string.Equals(Enum.GetName(status), value, StringComparison.Ordinal)
            ? status
            : ListingAvailabilityStatusType.Unspecified;
}
