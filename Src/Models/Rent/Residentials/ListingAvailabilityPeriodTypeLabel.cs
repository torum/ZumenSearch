using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Residentials;

public enum AvailabilityPeriodType { Unspecified, Early, Middle, Late }

public sealed class ListingAvailabilityPeriodTypeLabel(AvailabilityPeriodType key)
{
    private static readonly Dictionary<AvailabilityPeriodType, string> Labels = new()
    {
        [AvailabilityPeriodType.Unspecified] = "未指定",
        [AvailabilityPeriodType.Early] = "上旬",
        [AvailabilityPeriodType.Middle] = "中旬",
        [AvailabilityPeriodType.Late] = "下旬"
    };

    public string Label => Labels[Key];

    public AvailabilityPeriodType Key => key;

    public static IReadOnlyList<ListingAvailabilityPeriodTypeLabel> GetAll() => Enum.GetValues<AvailabilityPeriodType>()
        .Select(value => new ListingAvailabilityPeriodTypeLabel(value))
        .ToArray();

    public static AvailabilityPeriodType ParseAvailabilityPeriod(string? value) => value switch
    {
        nameof(AvailabilityPeriodType.Early) => AvailabilityPeriodType.Early,
        nameof(AvailabilityPeriodType.Middle) => AvailabilityPeriodType.Middle,
        nameof(AvailabilityPeriodType.Late) => AvailabilityPeriodType.Late,
        nameof(AvailabilityPeriodType.Unspecified) => AvailabilityPeriodType.Unspecified,
        _ => AvailabilityPeriodType.Unspecified
    };
}
