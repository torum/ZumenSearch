using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Domain.Entities.Rent.Residentials;

namespace ZumenSearch.Presentation.Features.Rent.Residentials.Models;

public sealed class ListingFloorGroundTypeLabel(FloorGroundType key)
{
    private static readonly Dictionary<FloorGroundType, string> Labels = new()
    {
        [FloorGroundType.AboveGround] = "地上",
        [FloorGroundType.BelowGround] = "地下"
    };

    public string Label => Labels[Key];

    public FloorGroundType Key => key;

    public static IReadOnlyList<ListingFloorGroundTypeLabel> GetAll() => Enum.GetValues<FloorGroundType>()
        .Select(value => new ListingFloorGroundTypeLabel(value))
        .ToArray();

    public static FloorGroundType ParseFloorGroundType(string? value) => value switch
    {
        nameof(FloorGroundType.BelowGround) => FloorGroundType.BelowGround,
        _ => FloorGroundType.AboveGround
    };
}

