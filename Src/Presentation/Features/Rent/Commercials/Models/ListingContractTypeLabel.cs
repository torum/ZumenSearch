using ZumenSearch.Domain.Entities.Rent.Commercials.Listing;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Models;

public sealed class ListingContractTypeLabel(ListingContractType key)
{
    private static readonly Dictionary<ListingContractType, string> Labels = new()
    {
        [ListingContractType.Unspecified] = "未指定",
        [ListingContractType.Ordinary] = "普通借家契約",
        [ListingContractType.FixedTerm] = "定期借家契約"
    };

    public string Label => Labels[Key];
    public ListingContractType Key => key;

    public static IReadOnlyList<ListingContractTypeLabel> GetAll() => Enum.GetValues<ListingContractType>()
        .Select(value => new ListingContractTypeLabel(value))
        .ToArray();

    public static ListingContractType ParseContractType(string? value) =>
        Enum.TryParse<ListingContractType>(value, out var contractType) && Enum.IsDefined(contractType)
            ? contractType
            : ListingContractType.Unspecified;
}
