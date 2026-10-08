using System.Globalization;

namespace ZumenSearch.Models.Rent.Residentials;

public enum RoomLayout { R, K, DK, LDK }
public enum FloorPosition { AboveGround, BelowGround }
public enum ExposureDirection { Unspecified, North, Northeast, East, Southeast, South, Southwest, West, Northwest }
public enum ListingCurrentStatus { Unspecified, ScheduledVacating, Vacant, ApplicationSubmitted, ContractedAndOccupied, OtherOrUnknown }
public enum AvailabilityPeriod { Unspecified, Early, Middle, Late }

public static class ListingOptionExtensions
{
    public static string GetStorageValue(this RoomLayout value) => value.ToString();

    public static string GetStorageValue(this FloorPosition value) => value.ToString();

    public static string GetStorageValue(this ExposureDirection value) => value.ToString();

    public static string GetStorageValue(this ListingCurrentStatus value) => value.ToString();

    public static string GetStorageValue(this AvailabilityPeriod value) => value.ToString();

    public static RoomLayout ParseRoomLayout(string? value) => value switch
    {
        nameof(RoomLayout.K) => RoomLayout.K,
        nameof(RoomLayout.DK) => RoomLayout.DK,
        nameof(RoomLayout.LDK) => RoomLayout.LDK,
        _ => RoomLayout.R
    };

    public static FloorPosition ParseFloorPosition(string? value) => value switch
    {
        nameof(FloorPosition.BelowGround) => FloorPosition.BelowGround,
        _ => FloorPosition.AboveGround
    };

    public static ExposureDirection ParseExposureDirection(string? value) => value switch
    {
        nameof(ExposureDirection.North) => ExposureDirection.North,
        nameof(ExposureDirection.Northeast) => ExposureDirection.Northeast,
        nameof(ExposureDirection.East) => ExposureDirection.East,
        nameof(ExposureDirection.Southeast) => ExposureDirection.Southeast,
        nameof(ExposureDirection.South) => ExposureDirection.South,
        nameof(ExposureDirection.Southwest) => ExposureDirection.Southwest,
        nameof(ExposureDirection.West) => ExposureDirection.West,
        nameof(ExposureDirection.Northwest) => ExposureDirection.Northwest,
        _ => ExposureDirection.Unspecified
    };

    public static ListingCurrentStatus ParseCurrentStatus(string? value) => value switch
    {
        nameof(ListingCurrentStatus.ScheduledVacating) => ListingCurrentStatus.ScheduledVacating,
        nameof(ListingCurrentStatus.Vacant) => ListingCurrentStatus.Vacant,
        nameof(ListingCurrentStatus.ApplicationSubmitted) => ListingCurrentStatus.ApplicationSubmitted,
        nameof(ListingCurrentStatus.ContractedAndOccupied) => ListingCurrentStatus.ContractedAndOccupied,
        nameof(ListingCurrentStatus.OtherOrUnknown) => ListingCurrentStatus.OtherOrUnknown,
        nameof(ListingCurrentStatus.Unspecified) => ListingCurrentStatus.Unspecified,
        _ => ListingCurrentStatus.Unspecified
    };

    public static AvailabilityPeriod ParseAvailabilityPeriod(string? value) => value switch
    {
        nameof(AvailabilityPeriod.Early) => AvailabilityPeriod.Early,
        nameof(AvailabilityPeriod.Middle) => AvailabilityPeriod.Middle,
        nameof(AvailabilityPeriod.Late) => AvailabilityPeriod.Late,
        nameof(AvailabilityPeriod.Unspecified) => AvailabilityPeriod.Unspecified,
        _ => AvailabilityPeriod.Unspecified
    };

    public static IReadOnlyList<string> GetRoomLayoutOptions() => ["R", "K", "DK", "LDK"];
    public static IReadOnlyList<string> GetFloorPositionOptions() => ["地上", "地下"];
    public static IReadOnlyList<string> GetExposureDirectionOptions() => ["北", "北東", "東", "南東", "南", "南西", "西", "北西"];
    public static IReadOnlyList<string> GetCurrentStatusOptions() => ["未指定", "解約・退去予定", "空室", "申し込み済み", "契約済・入居中", "その他・不明"];
    public static IReadOnlyList<string> GetAvailabilityPeriodOptions() => ["上旬", "中旬", "下旬"];

    public static IReadOnlyList<string> GetRoomCountOptions() => ["1", "2", "3", "4"];
    public static IReadOnlyList<string> GetAvailabilityMonthOptions() => ["１月", "２月", "３月", "４月", "５月", "６月", "７月", "８月", "９月", "１０月", "１１月", "１２月"];

    public static int? ParseAvailabilityMonth(string? value) => value switch
    {
        "１月" => 1, "２月" => 2, "３月" => 3, "４月" => 4, "５月" => 5, "６月" => 6,
        "７月" => 7, "８月" => 8, "９月" => 9, "１０月" => 10, "１１月" => 11, "１２月" => 12,
        _ => null
    };
}
