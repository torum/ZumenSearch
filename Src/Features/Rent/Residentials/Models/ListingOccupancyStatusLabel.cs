using System;
using System.Collections.Generic;
using System.Text;

namespace ZumenSearch.Models.Rent.Residentials;

public enum OccupancyStatus
{
    Unspecified,
    // 即入居系
    AvailableNow,        // 即入居可
    Vacant,              // 空室
    VacantSoon,          // 空室予定

    // 入居可能日指定系
    AvailableFromDate,   // ○月○日入居可（具体日付は別プロパティで管理）
    NegotiableMoveIn,    // 入居可能日相談

    // 入居中・募集停止系
    Occupied,            // 居住中
    ScheduledToVacate,   // 退去予定
    ApplicationPending,  // 申込あり
    UnderScreening,      // 審査中
    TemporarilyUnavailable, // 募集中止
    TemporarilyReserved, // 仮予約

    // 物件状態系
    UnderRenovation,     // リフォーム中
    UnderRemodeling,     // リノベーション中
    UnderConstruction,   // 建築中
    NewlyBuilt,          // 新築（未入居）

    // その他
    Negotiable,          // 要相談
    Contracted           // 成約済み
}

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


