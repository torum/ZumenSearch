namespace ZumenSearch.Models.Rent.Residentials;

public sealed class FacilityGasTypeLabel(FacilityGasType key)
{
    public FacilityGasType Key => key;
    public string Label => Key switch
    {
        FacilityGasType.Unspecified => "指定なし",
        FacilityGasType.CityGas => "都市ガス",
        FacilityGasType.Propane => "プロパンガス",
        _ => Key.ToString()
    };
    public static IReadOnlyList<FacilityGasTypeLabel> GetAll() => Enum.GetValues<FacilityGasType>().Select(value => new FacilityGasTypeLabel(value)).ToArray();
}

public sealed class FacilityTapWaterTypeLabel(FacilityTapWaterType key)
{
    public FacilityTapWaterType Key => key;
    public string Label => Key switch
    {
        FacilityTapWaterType.Unspecified => "指定なし",
        FacilityTapWaterType.Public => "公営",
        FacilityTapWaterType.Private => "私営",
        FacilityTapWaterType.Other => "他",
        _ => Key.ToString()
    };
    public static IReadOnlyList<FacilityTapWaterTypeLabel> GetAll() => Enum.GetValues<FacilityTapWaterType>().Select(value => new FacilityTapWaterTypeLabel(value)).ToArray();
}

public sealed class FacilitySewerageTypeLabel(FacilitySewerageType key)
{
    public FacilitySewerageType Key => key;
    public string Label => Key switch
    {
        FacilitySewerageType.Unspecified => "指定なし",
        FacilitySewerageType.Public => "公共下水",
        FacilitySewerageType.SepticTank => "浄化槽",
        FacilitySewerageType.Other => "他",
        _ => Key.ToString()
    };
    public static IReadOnlyList<FacilitySewerageTypeLabel> GetAll() => Enum.GetValues<FacilitySewerageType>().Select(value => new FacilitySewerageTypeLabel(value)).ToArray();
}

public sealed class FacilityParkingContractTypeLabel(FacilityParkingContractType key)
{
    public FacilityParkingContractType Key => key;
    public string Label => Key switch
    {
        FacilityParkingContractType.Optional => "任意",
        FacilityParkingContractType.Required => "必須",
        _ => Key.ToString()
    };
    public static IReadOnlyList<FacilityParkingContractTypeLabel> GetAll() => Enum.GetValues<FacilityParkingContractType>().Select(value => new FacilityParkingContractTypeLabel(value)).ToArray();
}

public sealed class FacilityAvailabilityTypeLabel(FacilityAvailabilityType key)
{
    public FacilityAvailabilityType Key => key;
    public string Label => Key switch
    {
        FacilityAvailabilityType.Unspecified => "指定なし",
        FacilityAvailabilityType.Available => "空き有り",
        FacilityAvailabilityType.Full => "空き無し",
        FacilityAvailabilityType.Nearby => "近隣",
        _ => Key.ToString()
    };
    public static IReadOnlyList<FacilityAvailabilityTypeLabel> GetAll() => Enum.GetValues<FacilityAvailabilityType>().Select(value => new FacilityAvailabilityTypeLabel(value)).ToArray();
}

public sealed class FacilityTaxTypeLabel(FacilityTaxType key)
{
    public FacilityTaxType Key => key;
    public string Label => Key switch
    {
        FacilityTaxType.Included => "税込み",
        FacilityTaxType.Excluded => "税抜き",
        _ => Key.ToString()
    };
    public static IReadOnlyList<FacilityTaxTypeLabel> GetAll() => Enum.GetValues<FacilityTaxType>().Select(value => new FacilityTaxTypeLabel(value)).ToArray();
}
