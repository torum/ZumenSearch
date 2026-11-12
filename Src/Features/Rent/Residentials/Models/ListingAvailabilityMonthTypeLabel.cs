using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Residentials;

public enum AvailabilityMonthType { Unspecified, January, February, March, April, May, June, July, August, September, October, November, December }

public sealed class ListingAvailabilityMonthTypeLabel(AvailabilityMonthType key)
{
    private static readonly Dictionary<AvailabilityMonthType, string> Labels = new()
    {
        [AvailabilityMonthType.Unspecified] = "未指定",
        [AvailabilityMonthType.January] = "１月",
        [AvailabilityMonthType.February] = "２月",
        [AvailabilityMonthType.March] = "３月",
        [AvailabilityMonthType.April] = "４月",
        [AvailabilityMonthType.May] = "５月",
        [AvailabilityMonthType.June] = "６月",
        [AvailabilityMonthType.July] = "７月",
        [AvailabilityMonthType.August] = "８月",
        [AvailabilityMonthType.September] = "９月",
        [AvailabilityMonthType.October] = "１０月",
        [AvailabilityMonthType.November] = "１１月",
        [AvailabilityMonthType.December] = "１２月"
    };

    public string Label => Labels[Key];

    public AvailabilityMonthType Key => key;

    public static IReadOnlyList<ListingAvailabilityMonthTypeLabel> GetAll() => Enum.GetValues<AvailabilityMonthType>()
        .Select(value => new ListingAvailabilityMonthTypeLabel(value))
        .ToArray();

    public static AvailabilityMonthType ParseAvailabilityMonth(string? value) => value switch
    {
        nameof(AvailabilityMonthType.January) => AvailabilityMonthType.January,
        nameof(AvailabilityMonthType.February) => AvailabilityMonthType.February,
        nameof(AvailabilityMonthType.March) => AvailabilityMonthType.March,
        nameof(AvailabilityMonthType.April) => AvailabilityMonthType.April,
        nameof(AvailabilityMonthType.May) => AvailabilityMonthType.May,
        nameof(AvailabilityMonthType.June) => AvailabilityMonthType.June,
        nameof(AvailabilityMonthType.July) => AvailabilityMonthType.July,
        nameof(AvailabilityMonthType.August) => AvailabilityMonthType.August,
        nameof(AvailabilityMonthType.September) => AvailabilityMonthType.September,
        nameof(AvailabilityMonthType.October) => AvailabilityMonthType.October,
        nameof(AvailabilityMonthType.November) => AvailabilityMonthType.November,
        nameof(AvailabilityMonthType.December) => AvailabilityMonthType.December,
        nameof(AvailabilityMonthType.Unspecified) => AvailabilityMonthType.Unspecified,
        _ => AvailabilityMonthType.Unspecified
    };
};