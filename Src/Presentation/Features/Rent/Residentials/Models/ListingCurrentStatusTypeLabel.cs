using ZumenSearch.Domain.Entities.Rent.Residentials;

namespace ZumenSearch.Presentation.Features.Rent.Residentials.Models;

public sealed class ListingCurrentStatusTypeLabel(CurrentStatusType key)
{
    private Dictionary<CurrentStatusType, string> ListingAvailabilityStatusTypeDictionary{ get;} = new()
    {
                {CurrentStatusType.Unspecified, "未指定"},
                {CurrentStatusType.ScheduledVacating, "退去予定"},
                {CurrentStatusType.Vacant, "空室"},
                {CurrentStatusType.ApplicationSubmitted, "申込あり"},
                {CurrentStatusType.ContractedAndOccupied, "契約済み・入居中"},
                {CurrentStatusType.OtherOrUnknown, "その他・不明"},
    };

    public string Label => ListingAvailabilityStatusTypeDictionary[Key];

    public CurrentStatusType Key => key;

    public static IReadOnlyList<ListingCurrentStatusTypeLabel> GetAll() => Enum.GetValues<CurrentStatusType>()
    .Select(value => new ListingCurrentStatusTypeLabel(value))
    .ToArray();

    public static CurrentStatusType ParseCurrentStatus(string? value) => value switch
    {
        nameof(CurrentStatusType.ScheduledVacating) => CurrentStatusType.ScheduledVacating,
        nameof(CurrentStatusType.Vacant) => CurrentStatusType.Vacant,
        nameof(CurrentStatusType.ApplicationSubmitted) => CurrentStatusType.ApplicationSubmitted,
        nameof(CurrentStatusType.ContractedAndOccupied) => CurrentStatusType.ContractedAndOccupied,
        nameof(CurrentStatusType.OtherOrUnknown) => CurrentStatusType.OtherOrUnknown,
        nameof(CurrentStatusType.Unspecified) => CurrentStatusType.Unspecified,
        _ => CurrentStatusType.Unspecified
    };
}
