using ZumenSearch.Domain.Entities.Rent.Commercials;
using ZumenSearch.Domain.Entities.Rent.Commercials.Listing;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.Models;

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

public sealed class ListingUnitConditionTypeLabel(ListingUnitConditionType key)
{
    public ListingUnitConditionType Key => key;
    public string Label => Key switch { ListingUnitConditionType.Unspecified => "未指定", ListingUnitConditionType.FittedOut => "居抜き", ListingUnitConditionType.Shell => "スケルトン", _ => throw new ArgumentOutOfRangeException(nameof(Key)) };
    public static IReadOnlyList<ListingUnitConditionTypeLabel> GetAll() => Enum.GetValues<ListingUnitConditionType>().Select(value => new ListingUnitConditionTypeLabel(value)).ToArray();
}

public sealed class ListingFixtureTransferTypeLabel(ListingFixtureTransferType key)
{
    public ListingFixtureTransferType Key => key;
    public string Label => Key switch { ListingFixtureTransferType.Unconfirmed => "未確認", ListingFixtureTransferType.Negotiable => "相談可", ListingFixtureTransferType.Unavailable => "不可", _ => throw new ArgumentOutOfRangeException(nameof(Key)) };
    public static IReadOnlyList<ListingFixtureTransferTypeLabel> GetAll() => Enum.GetValues<ListingFixtureTransferType>().Select(value => new ListingFixtureTransferTypeLabel(value)).ToArray();
}
