using ZumenSearch.Domain.Entities.Rent.Commercials.Listing;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Models;

public sealed class ListingContractTaxTypeLabel(ListingContractTaxType key)
{
    private static readonly Dictionary<ListingContractTaxType, string> Labels = new()
    {
        [ListingContractTaxType.Unconfirmed] = "未確認",
        [ListingContractTaxType.Included] = "税込",
        [ListingContractTaxType.Excluded] = "税別"
    };

    public string Label => Labels[Key];
    public ListingContractTaxType Key => key;

    public static IReadOnlyList<ListingContractTaxTypeLabel> GetAll() => Enum.GetValues<ListingContractTaxType>()
        .Select(value => new ListingContractTaxTypeLabel(value))
        .ToArray();
}
