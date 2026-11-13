namespace ZumenSearch.Models.Rent.Commercials.Listing;

public enum ListingFloorGroundType { AboveGround, BelowGround }
public enum ListingUseType { Unspecified, Office, Retail, Restaurant, Warehouse, Factory, Other }
public enum ListingAvailabilityStatusType { Unspecified, ScheduledVacating, Vacant, ApplicationSubmitted, ContractedAndOccupied, OtherOrUnknown }
public enum ListingAvailabilityMonthType { Unspecified, January, February, March, April, May, June, July, August, September, October, November, December }
public enum ListingAvailabilityPeriodType { Unspecified, Early, Middle, Late }

public sealed class ListingFloorGroundTypeLabel(ListingFloorGroundType key)
{
    public ListingFloorGroundType Key => key;
    public string Label => Key switch { ListingFloorGroundType.AboveGround => "地上", ListingFloorGroundType.BelowGround => "地下", _ => throw new ArgumentOutOfRangeException(nameof(key)) };
    public static IReadOnlyList<ListingFloorGroundTypeLabel> GetAll() => Enum.GetValues<ListingFloorGroundType>().Select(value => new ListingFloorGroundTypeLabel(value)).ToArray();
}

public sealed class ListingUseTypeLabel(ListingUseType key)
{
    public ListingUseType Key => key;
    public string Label => Key switch { ListingUseType.Unspecified => "未指定", ListingUseType.Office => "事務所", ListingUseType.Retail => "店舗", ListingUseType.Restaurant => "飲食店", ListingUseType.Warehouse => "倉庫", ListingUseType.Factory => "工場", ListingUseType.Other => "その他", _ => throw new ArgumentOutOfRangeException(nameof(key)) };
    public static IReadOnlyList<ListingUseTypeLabel> GetAll() => Enum.GetValues<ListingUseType>().Select(value => new ListingUseTypeLabel(value)).ToArray();
}

public sealed class ListingAvailabilityStatusTypeLabel(ListingAvailabilityStatusType key)
{
    public ListingAvailabilityStatusType Key => key;
    public string Label => Key switch { ListingAvailabilityStatusType.Unspecified => "未指定", ListingAvailabilityStatusType.ScheduledVacating => "退去予定", ListingAvailabilityStatusType.Vacant => "空室", ListingAvailabilityStatusType.ApplicationSubmitted => "申込あり", ListingAvailabilityStatusType.ContractedAndOccupied => "契約済み・入居中", ListingAvailabilityStatusType.OtherOrUnknown => "その他・不明", _ => throw new ArgumentOutOfRangeException(nameof(key)) };
    public static IReadOnlyList<ListingAvailabilityStatusTypeLabel> GetAll() => Enum.GetValues<ListingAvailabilityStatusType>().Select(value => new ListingAvailabilityStatusTypeLabel(value)).ToArray();
}

public sealed class ListingAvailabilityMonthTypeLabel(ListingAvailabilityMonthType key)
{
    public ListingAvailabilityMonthType Key => key;
    public string Label => Key == ListingAvailabilityMonthType.Unspecified ? "未指定" : Enum.IsDefined(Key) ? $"{(int)Key}月" : throw new ArgumentOutOfRangeException(nameof(key));
    public static IReadOnlyList<ListingAvailabilityMonthTypeLabel> GetAll() => Enum.GetValues<ListingAvailabilityMonthType>().Select(value => new ListingAvailabilityMonthTypeLabel(value)).ToArray();
}

public sealed class ListingAvailabilityPeriodTypeLabel(ListingAvailabilityPeriodType key)
{
    public ListingAvailabilityPeriodType Key => key;
    public string Label => Key switch { ListingAvailabilityPeriodType.Unspecified => "未指定", ListingAvailabilityPeriodType.Early => "上旬", ListingAvailabilityPeriodType.Middle => "中旬", ListingAvailabilityPeriodType.Late => "下旬", _ => throw new ArgumentOutOfRangeException(nameof(key)) };
    public static IReadOnlyList<ListingAvailabilityPeriodTypeLabel> GetAll() => Enum.GetValues<ListingAvailabilityPeriodType>().Select(value => new ListingAvailabilityPeriodTypeLabel(value)).ToArray();
}
