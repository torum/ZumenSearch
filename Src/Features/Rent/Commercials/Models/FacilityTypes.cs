namespace ZumenSearch.Models.Rent.Commercials;

public enum GasSupplyType { Unconfirmed, CityGas, Propane, Other }
public enum WaterSupplyType { Unconfirmed, Public, Private, Well, Other }
public enum SewerageType { Unconfirmed, PublicSewer, SepticTank, Other }
public enum ParkingVacancyType { Unconfirmed, Available, Unavailable }
public enum ParkingTaxType { Unconfirmed, Included, Excluded }

public sealed class GasSupplyTypeLabel(GasSupplyType key)
{
    public GasSupplyType Key => key;
    public string Label => Key switch { GasSupplyType.Unconfirmed => "未確認", GasSupplyType.CityGas => "都市ガス", GasSupplyType.Propane => "プロパン", GasSupplyType.Other => "その他", _ => throw new ArgumentOutOfRangeException(nameof(Key)) };
}

public sealed class WaterSupplyTypeLabel(WaterSupplyType key)
{
    public WaterSupplyType Key => key;
    public string Label => Key switch { WaterSupplyType.Unconfirmed => "未確認", WaterSupplyType.Public => "公営", WaterSupplyType.Private => "私営", WaterSupplyType.Well => "井戸", WaterSupplyType.Other => "その他", _ => throw new ArgumentOutOfRangeException(nameof(Key)) };
}

public sealed class SewerageTypeLabel(SewerageType key)
{
    public SewerageType Key => key;
    public string Label => Key switch { SewerageType.Unconfirmed => "未確認", SewerageType.PublicSewer => "公共下水", SewerageType.SepticTank => "浄化槽", SewerageType.Other => "その他", _ => throw new ArgumentOutOfRangeException(nameof(Key)) };
}

public sealed class ParkingVacancyTypeLabel(ParkingVacancyType key)
{
    public ParkingVacancyType Key => key;
    public string Label => Key switch { ParkingVacancyType.Unconfirmed => "未確認", ParkingVacancyType.Available => "空きあり", ParkingVacancyType.Unavailable => "空きなし", _ => throw new ArgumentOutOfRangeException(nameof(Key)) };
}

public sealed class ParkingTaxTypeLabel(ParkingTaxType key)
{
    public ParkingTaxType Key => key;
    public string Label => Key switch { ParkingTaxType.Unconfirmed => "未確認", ParkingTaxType.Included => "税込", ParkingTaxType.Excluded => "税別", _ => throw new ArgumentOutOfRangeException(nameof(Key)) };
}
