namespace ZumenSearch.Models.Rent.Residentials;

public enum FeeUnitType
{
    Months,
    Yen
}

public sealed class ListingFeeUnitTypeLabel(FeeUnitType key)
{
    private static readonly Dictionary<FeeUnitType, string> Labels = new()
    {
        [FeeUnitType.Months] = "ヵ月",
        [FeeUnitType.Yen] = "円"
    };

    public string Label => Labels[Key];

    public FeeUnitType Key => key;

    public static IReadOnlyList<ListingFeeUnitTypeLabel> GetAll() => Enum.GetValues<FeeUnitType>()
        .Select(value => new ListingFeeUnitTypeLabel(value))
        .ToArray();

    public static FeeUnitType ParseFeeUnitType(string? value) =>
        Enum.TryParse<FeeUnitType>(value, out var unit) && Enum.IsDefined(unit)
            ? unit
            : FeeUnitType.Months;
}
