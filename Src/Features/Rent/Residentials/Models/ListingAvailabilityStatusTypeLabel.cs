using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Residentials;

public enum AvailabilityStatusType { Unspecified, ScheduledVacating, Vacant, ApplicationSubmitted, ContractedAndOccupied, OtherOrUnknown }

public sealed class ListingAvailabilityStatusTypeLabel(AvailabilityStatusType key)
{
    private Dictionary<AvailabilityStatusType, string> ListingAvailabilityStatusTypeDictionary{ get;} = new()
    {
                {AvailabilityStatusType.Unspecified, "未指定"},
                {AvailabilityStatusType.ScheduledVacating, "退去予定"},
                {AvailabilityStatusType.Vacant, "空室"},
                {AvailabilityStatusType.ApplicationSubmitted, "申込あり"},
                {AvailabilityStatusType.ContractedAndOccupied, "契約済み・入居中"},
                {AvailabilityStatusType.OtherOrUnknown, "その他・不明"},
    };

    public string Label => ListingAvailabilityStatusTypeDictionary[Key];

    public AvailabilityStatusType Key => key;

    public static IReadOnlyList<ListingAvailabilityStatusTypeLabel> GetAll() => Enum.GetValues<AvailabilityStatusType>()
    .Select(value => new ListingAvailabilityStatusTypeLabel(value))
    .ToArray();

    public static AvailabilityStatusType ParseCurrentStatus(string? value) => value switch
    {
        nameof(AvailabilityStatusType.ScheduledVacating) => AvailabilityStatusType.ScheduledVacating,
        nameof(AvailabilityStatusType.Vacant) => AvailabilityStatusType.Vacant,
        nameof(AvailabilityStatusType.ApplicationSubmitted) => AvailabilityStatusType.ApplicationSubmitted,
        nameof(AvailabilityStatusType.ContractedAndOccupied) => AvailabilityStatusType.ContractedAndOccupied,
        nameof(AvailabilityStatusType.OtherOrUnknown) => AvailabilityStatusType.OtherOrUnknown,
        nameof(AvailabilityStatusType.Unspecified) => AvailabilityStatusType.Unspecified,
        _ => AvailabilityStatusType.Unspecified
    };
}
