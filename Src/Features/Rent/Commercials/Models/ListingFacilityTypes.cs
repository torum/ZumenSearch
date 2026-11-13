namespace ZumenSearch.Models.Rent.Commercials.Listing;

public enum ListingUnitConditionType { Unspecified, FittedOut, Shell }
public enum ListingFixtureTransferType { Unconfirmed, Negotiable, Unavailable }

public sealed class ListingUnitConditionTypeLabel(ListingUnitConditionType key)
{
    public ListingUnitConditionType Key => key;
    public string Label => Key switch { ListingUnitConditionType.Unspecified => "未指定", ListingUnitConditionType.FittedOut => "居抜き", ListingUnitConditionType.Shell => "スケルトン", _ => throw new ArgumentOutOfRangeException(nameof(Key)) };
    public static IReadOnlyList<ListingUnitConditionTypeLabel> GetAll() => Enum.GetValues<ListingUnitConditionType>().Select(value => new ListingUnitConditionTypeLabel(value)).ToArray();
}

public sealed class ListingFixtureTransferTypeLabel(ListingFixtureTransferType key)
{
    public ListingFixtureTransferType Key => key;
    public string Label => Key switch { ListingFixtureTransferType.Unconfirmed => "未確認", ListingFixtureTransferType.Negotiable => "相談可", ListingFixtureTransferType.Unavailable => "不可", _ => throw new ArgumentOutOfRangeException(nameof(Key)) };
    public static IReadOnlyList<ListingFixtureTransferTypeLabel> GetAll() => Enum.GetValues<ListingFixtureTransferType>().Select(value => new ListingFixtureTransferTypeLabel(value)).ToArray();
}
