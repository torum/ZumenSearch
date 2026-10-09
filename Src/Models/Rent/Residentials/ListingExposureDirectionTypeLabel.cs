using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Residentials;

public enum ExposureDirectionType { Unspecified, North, Northeast, East, Southeast, South, Southwest, West, Northwest }

public sealed class ListingExposureDirectionTypeLabel(ExposureDirectionType key)
{
    private static readonly Dictionary<ExposureDirectionType, string> Labels = new()
    {
        [ExposureDirectionType.North] = "北",
        [ExposureDirectionType.Northeast] = "北東",
        [ExposureDirectionType.East] = "東",
        [ExposureDirectionType.Southeast] = "南東",
        [ExposureDirectionType.South] = "南",
        [ExposureDirectionType.Southwest] = "南西",
        [ExposureDirectionType.West] = "西",
        [ExposureDirectionType.Northwest] = "北西",
        [ExposureDirectionType.Unspecified] = "指定なし"
    };

    public string Label => Labels[Key];

    public ExposureDirectionType Key => key;

    public static IReadOnlyList<ListingExposureDirectionTypeLabel> GetAll() => Enum.GetValues<ExposureDirectionType>()
        .Select(value => new ListingExposureDirectionTypeLabel(value))
        .ToArray();

    public static ExposureDirectionType ParseExposureDirection(string? value) => value switch
    {
        nameof(ExposureDirectionType.North) => ExposureDirectionType.North,
        nameof(ExposureDirectionType.Northeast) => ExposureDirectionType.Northeast,
        nameof(ExposureDirectionType.East) => ExposureDirectionType.East,
        nameof(ExposureDirectionType.Southeast) => ExposureDirectionType.Southeast,
        nameof(ExposureDirectionType.South) => ExposureDirectionType.South,
        nameof(ExposureDirectionType.Southwest) => ExposureDirectionType.Southwest,
        nameof(ExposureDirectionType.West) => ExposureDirectionType.West,
        nameof(ExposureDirectionType.Northwest) => ExposureDirectionType.Northwest,
        _ => ExposureDirectionType.Unspecified
    };

}
