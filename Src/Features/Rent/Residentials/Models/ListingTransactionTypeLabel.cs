namespace ZumenSearch.Models.Rent.Residentials;

public enum ListingTransactionType
{
    Unspecified,
    GeneralBrokerage,
    ExclusiveBrokerage,
    ExclusiveAgencyBrokerage,
    Agency,
    Lessor
}

public sealed class ListingTransactionTypeLabel(ListingTransactionType key)
{
    private static readonly Dictionary<ListingTransactionType, string> Labels = new()
    {
        [ListingTransactionType.Unspecified] = "未指定",
        [ListingTransactionType.GeneralBrokerage] = "一般媒介",
        [ListingTransactionType.ExclusiveBrokerage] = "専任媒介",
        [ListingTransactionType.ExclusiveAgencyBrokerage] = "専属専任媒介",
        [ListingTransactionType.Agency] = "代理",
        [ListingTransactionType.Lessor] = "貸主"
    };

    public string Label => Labels[Key];
    public ListingTransactionType Key => key;

    public static IReadOnlyList<ListingTransactionTypeLabel> GetAll() => Enum.GetValues<ListingTransactionType>()
        .Select(value => new ListingTransactionTypeLabel(value))
        .ToArray();
}
