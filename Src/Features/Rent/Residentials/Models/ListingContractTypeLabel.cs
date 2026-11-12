namespace ZumenSearch.Models.Rent.Residentials;

public enum ContractType
{
    Unspecified,
    Ordinary,
    FixedTerm
}

public sealed class ListingContractTypeLabel(ContractType key)
{
    private static readonly Dictionary<ContractType, string> Labels = new()
    {
        [ContractType.Unspecified] = "未指定",
        [ContractType.Ordinary] = "普通借家契約",
        [ContractType.FixedTerm] = "定期借家契約"
    };

    public string Label => Labels[Key];

    public ContractType Key => key;

    public static IReadOnlyList<ListingContractTypeLabel> GetAll() => Enum.GetValues<ContractType>()
        .Select(value => new ListingContractTypeLabel(value))
        .ToArray();

    public static ContractType ParseContractType(string? value) =>
        Enum.TryParse<ContractType>(value, out var contractType) && Enum.IsDefined(contractType)
            ? contractType
            : ContractType.Unspecified;
}
