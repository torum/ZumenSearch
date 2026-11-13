namespace ZumenSearch.Models.Rent.Commercials.Listing;

public enum ListingFeeUnitType { Months, Yen }
public enum ListingContractTaxType { Unconfirmed, Included, Excluded }
public enum ListingContractType { Unspecified, Ordinary, FixedTerm }

public sealed class ListingFeeUnitTypeLabel(ListingFeeUnitType key)
{
    public ListingFeeUnitType Key => key;
    public string Label => Key switch { ListingFeeUnitType.Months => "ヵ月", ListingFeeUnitType.Yen => "円", _ => throw new ArgumentOutOfRangeException(nameof(Key)) };
    public static IReadOnlyList<ListingFeeUnitTypeLabel> GetAll() => Enum.GetValues<ListingFeeUnitType>().Select(value => new ListingFeeUnitTypeLabel(value)).ToArray();
}

public sealed class ListingContractTaxTypeLabel(ListingContractTaxType key)
{
    public ListingContractTaxType Key => key;
    public string Label => Key switch { ListingContractTaxType.Unconfirmed => "未確認", ListingContractTaxType.Included => "税込", ListingContractTaxType.Excluded => "税別", _ => throw new ArgumentOutOfRangeException(nameof(Key)) };
    public static IReadOnlyList<ListingContractTaxTypeLabel> GetAll() => Enum.GetValues<ListingContractTaxType>().Select(value => new ListingContractTaxTypeLabel(value)).ToArray();
}

public sealed class ListingContractTypeLabel(ListingContractType key)
{
    public ListingContractType Key => key;
    public string Label => Key switch { ListingContractType.Unspecified => "未指定", ListingContractType.Ordinary => "普通借家契約", ListingContractType.FixedTerm => "定期借家契約", _ => throw new ArgumentOutOfRangeException(nameof(Key)) };
    public static IReadOnlyList<ListingContractTypeLabel> GetAll() => Enum.GetValues<ListingContractType>().Select(value => new ListingContractTypeLabel(value)).ToArray();
}
