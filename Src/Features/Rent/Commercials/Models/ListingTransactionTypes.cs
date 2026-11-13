namespace ZumenSearch.Models.Rent.Commercials.Listing;

public enum ListingHandlingType { Unspecified, OtherCompanyListing, OwnCompanyBrokerage, OwnCompanyManagement, OwnCompanyOwnership }
public enum ListingTransactionType { Unspecified, GeneralBrokerage, ExclusiveBrokerage, ExclusiveAgencyBrokerage, Agency, Lessor }
public enum ListingCommissionUnitType { Percent, Yen }

public sealed class ListingHandlingTypeLabel(ListingHandlingType key)
{
    public ListingHandlingType Key => key;
    public string Label => Key switch { ListingHandlingType.Unspecified => "未指定", ListingHandlingType.OtherCompanyListing => "他社元付（客付）", ListingHandlingType.OwnCompanyBrokerage => "自社元付（媒介）", ListingHandlingType.OwnCompanyManagement => "自社管理", ListingHandlingType.OwnCompanyOwnership => "自社所有（貸主）", _ => throw new ArgumentOutOfRangeException(nameof(Key)) };
    public static IReadOnlyList<ListingHandlingTypeLabel> GetAll() => Enum.GetValues<ListingHandlingType>().Select(value => new ListingHandlingTypeLabel(value)).ToArray();
}

public sealed class ListingTransactionTypeLabel(ListingTransactionType key)
{
    public ListingTransactionType Key => key;
    public string Label => Key switch { ListingTransactionType.Unspecified => "未指定", ListingTransactionType.GeneralBrokerage => "一般媒介", ListingTransactionType.ExclusiveBrokerage => "専任媒介", ListingTransactionType.ExclusiveAgencyBrokerage => "専属専任媒介", ListingTransactionType.Agency => "代理", ListingTransactionType.Lessor => "貸主", _ => throw new ArgumentOutOfRangeException(nameof(Key)) };
    public static IReadOnlyList<ListingTransactionTypeLabel> GetAll() => Enum.GetValues<ListingTransactionType>().Select(value => new ListingTransactionTypeLabel(value)).ToArray();
}

public sealed class ListingCommissionUnitTypeLabel(ListingCommissionUnitType key)
{
    public ListingCommissionUnitType Key => key;
    public string Label => Key switch { ListingCommissionUnitType.Percent => "％", ListingCommissionUnitType.Yen => "円", _ => throw new ArgumentOutOfRangeException(nameof(Key)) };
    public static IReadOnlyList<ListingCommissionUnitTypeLabel> GetAll() => Enum.GetValues<ListingCommissionUnitType>().Select(value => new ListingCommissionUnitTypeLabel(value)).ToArray();
}
