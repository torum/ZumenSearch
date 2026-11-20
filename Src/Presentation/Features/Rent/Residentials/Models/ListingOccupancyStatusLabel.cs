using System;
using System.Collections.Generic;
using System.Text;
using ZumenSearch.Domain.Entities.Rent.Residentials;

namespace ZumenSearch.Presentation.Features.Rent.Residentials.Models;

public sealed class ListingOccupancyStatusLabel(OccupancyStatus key)
{
    private static readonly Dictionary<OccupancyStatus, string> Labels = new()
    {
        [OccupancyStatus.Unspecified] = "未指定",
        [OccupancyStatus.AvailableNow] = "即入居可",
        [OccupancyStatus.Vacant] = "空室",
        [OccupancyStatus.VacantSoon] = "空室予定",
        [OccupancyStatus.AvailableFromDate] = "○月○日入居可",
        [OccupancyStatus.NegotiableMoveIn] = "入居可能日相談",
        [OccupancyStatus.Occupied] = "居住中",
        [OccupancyStatus.ScheduledToVacate] = "退去予定",
        [OccupancyStatus.ApplicationPending] = "申込あり",
        [OccupancyStatus.UnderScreening] = "審査中",
        [OccupancyStatus.TemporarilyUnavailable] = "募集中止",
        [OccupancyStatus.TemporarilyReserved] = "仮予約",
        [OccupancyStatus.UnderRenovation] = "リフォーム中",
        [OccupancyStatus.UnderRemodeling] = "リノベーション中",
        [OccupancyStatus.UnderConstruction] = "建築中",
        [OccupancyStatus.NewlyBuilt] = "新築（未入居）",
        [OccupancyStatus.Negotiable] = "要相談",
        [OccupancyStatus.Contracted] = "成約済み"
    };

    public string Label => Labels[Key];

    public OccupancyStatus Key => key;

    public static IReadOnlyList<ListingOccupancyStatusLabel> GetAll() => Enum.GetValues<OccupancyStatus>()
        .Select(value => new ListingOccupancyStatusLabel(value))
        .ToArray();

    public static OccupancyStatus ParseOccupancyStatus(string? value) =>
        Enum.TryParse<OccupancyStatus>(value, out var occupancyStatus) && Enum.IsDefined(occupancyStatus)
            ? occupancyStatus
            : OccupancyStatus.Unspecified;
}


