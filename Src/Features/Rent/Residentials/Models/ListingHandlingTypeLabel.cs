namespace ZumenSearch.Models.Rent.Residentials;

public enum ListingHandlingType
{
    Unspecified,
    OtherCompanyListing,
    OwnCompanyBrokerage,
    OwnCompanyManagement,
    OwnCompanyOwnership
}

public sealed class ListingHandlingTypeLabel(ListingHandlingType key)
{
    private static readonly Dictionary<ListingHandlingType, string> Labels = new()
    {
        [ListingHandlingType.Unspecified] = "未指定",
        [ListingHandlingType.OtherCompanyListing] = "他社元付（客付）",
        [ListingHandlingType.OwnCompanyBrokerage] = "自社元付（媒介）",
        [ListingHandlingType.OwnCompanyManagement] = "自社管理",
        [ListingHandlingType.OwnCompanyOwnership] = "自社所有（貸主）"
    };

    public string Label => Labels[Key];
    public ListingHandlingType Key => key;

    public static IReadOnlyList<ListingHandlingTypeLabel> GetAll() => Enum.GetValues<ListingHandlingType>()
        .Select(value => new ListingHandlingTypeLabel(value))
        .ToArray();
}
